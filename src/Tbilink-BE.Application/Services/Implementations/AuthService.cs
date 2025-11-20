using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Tbilink_BE.Application.Common;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Domain.Constants;
using Tbilink_BE.Models;
using Tbilink_BE.Services.Helpers;
using Tbilink_BE.Services.Interfaces;

namespace Tbilink_BE.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IEmailService _emailService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IWebHostEnvironment _env;
        private readonly SmtpSettings _smtp;

        public AuthService(IAuthRepository authRepository, IEmailService emailService, ITokenProvider tokenProvider, IWebHostEnvironment env, IOptions<SmtpSettings> smtpOptions)
        {
            _authRepository = authRepository;
            _emailService = emailService;
            _tokenProvider = tokenProvider;
            _env = env;
            _smtp = smtpOptions.Value;
        }

        public async Task<ServiceResponse<LoginResultDto>> Login(LoginDTO loginDTO)
        {
            if (loginDTO == null ||
                string.IsNullOrWhiteSpace(loginDTO.Email) ||
                string.IsNullOrWhiteSpace(loginDTO.Password))
            {
                return ServiceResponse<LoginResultDto>.Fail("Email and password are required.");
            }

            var userExist = await _authRepository.GetUserByEmail(loginDTO.Email.Trim());

            if (userExist == null)
            {
                return ServiceResponse<LoginResultDto>.Fail("Invalid Email or Password.", 404);
            }

            if (!ValidationHelper.IsValidPassword(loginDTO.Password))
            {
                return ServiceResponse<LoginResultDto>.Fail("Password must be at least 8 characters long and contain at least one uppercase letter, one number, and one special character.");
            }

            if (!OtpHelper.VerifyPasswordHash(loginDTO.Password, userExist.PasswordHash, userExist.PasswordSalt))
            {
                return ServiceResponse<LoginResultDto>.Fail("Invalid Email or Password.");
            }

            if (!userExist.IsEmailVerified)
            {
                return ServiceResponse<LoginResultDto>.Fail("User's email is not verified.", 403);
            }

            var userModel = new UserDTO
            {
                Id = userExist.Id,
                FirstName = userExist.FirstName,
                LastName = userExist.LastName,
                UserName = userExist.UserName,
                Email = userExist.Email,
                ProfilePhotoUrl = userExist.ProfilePhotoUrl,
                Role = userExist.Role,
                IsEmailVerified = userExist.IsEmailVerified,
            };

            var (accessToken, expiresAt) = _tokenProvider.CreateAccessToken(userModel);
            var (refreshToken, refreshTokenExpiresAt) = _tokenProvider.CreateRefreshToken();

            await _authRepository.SaveRefreshToken(userExist, refreshToken, refreshTokenExpiresAt);

            if (!await _authRepository.SaveChangesAsync())
            {
                return ServiceResponse<LoginResultDto>.Fail("Unable to save refresh token.", 500);
            }

            var result = new LoginResultDto
            {
                Tokens = new AuthTokensDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt
                },
                Data = userModel
            };

            return ServiceResponse<LoginResultDto>.Success(result, "Login Successful.");
        }

        public async Task<ServiceResponse<RegisterResultDTO>> Register(RegisterDTO registerDTO)
        {
            if (registerDTO == null)
            {
                return ServiceResponse<RegisterResultDTO>.Fail("Registration details required.");
            }

            if (string.IsNullOrWhiteSpace(registerDTO.FirstName) || registerDTO.FirstName.Length < 1)
            {
                return ServiceResponse<RegisterResultDTO>.Fail("First name is required (min length 1).");
            }

            if (string.IsNullOrWhiteSpace(registerDTO.LastName) || registerDTO.LastName.Length < 1)
            {
                return ServiceResponse<RegisterResultDTO>.Fail("Last name is required (min length 1).");
            }

            if (!ValidationHelper.IsValidEmail(registerDTO.Email))
            {
                return ServiceResponse<RegisterResultDTO>.Fail("Wrong email format.");
            }

            if (!ValidationHelper.IsValidPassword(registerDTO.Password))
            {
                return ServiceResponse<RegisterResultDTO>.Fail("Password must be at least 8 characters long and contain at least one uppercase letter, one number, and one special character.");
            }

            if (!ValidationHelper.PasswordCompare(registerDTO.Password, registerDTO.RepPassword))
            {
                return ServiceResponse<RegisterResultDTO>.Fail("Passwords do not match.");
            }

            var userExists = await _authRepository.UserExistCheck(registerDTO.Email);

            if (userExists)
            {
                return ServiceResponse<RegisterResultDTO>.Fail("Email is already in use.", 409);
            }

            var usernameAvailable = await _authRepository.UsernameIsAvailable(registerDTO.UserName.ToLower().Trim());

            if (usernameAvailable)
            {
                return ServiceResponse<RegisterResultDTO>.Fail("Username is already in use.", 409);
            }

            OtpHelper.CreatePasswordHash(registerDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                FirstName = registerDTO.FirstName.Trim(),
                LastName = registerDTO.LastName.Trim(),
                UserName = registerDTO.UserName.Trim(),
                Email = registerDTO.Email.ToLower().Trim(),
                Role = "User",
                RegisterDate = DateTime.UtcNow,
                IsEmailVerified = false,

                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,

                PostCount = 0,
                FollowersCount = 0,
                FollowingCount = 0,
                IsPublicProfile = true,
                ShowEmail = false,
                ShowPhone = false,
                AllowTagging = true,
                EmailNotifications = true,
                PushNotifications = true,
                SmsNotifications = false,
                MarketingEmails = false,
                Language = "en",
                TimeZone = "UTC",
                IsOnline = false,
                LastActive = DateTime.UtcNow
            };

            await _authRepository.RegisterUser(user);

            var result = await _authRepository.SaveChangesAsync();

            if (!result)
            {
                return ServiceResponse<RegisterResultDTO>.Fail("Failed to create user.", 500);
            }

            var senderResponse = await SendtVerificationCode(registerDTO.Email, CodeType.EmailVerification);

            if (senderResponse != null)
            {
                return ServiceResponse<RegisterResultDTO>.Fail($"{senderResponse.Message}", senderResponse.StatusCode);
            }

            var userModel = new UserDTO
            {
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                ProfilePhotoUrl = null,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
            };

            var response = new RegisterResultDTO
            {
                Data = userModel 
            };

            return ServiceResponse<RegisterResultDTO>.Success(response, "User created successfully.", 201);
        }

        public async Task<ServiceResponse<string>> SendtVerificationCode(string email, CodeType codeType)
        {

            if (string.IsNullOrWhiteSpace(email) || !ValidationHelper.IsValidEmail(email))
            {
                return ServiceResponse<string>.Fail("Invalid email format.", 400);
            }

            var userExists = await _authRepository.GetUserByEmail(email);

            if (userExists == null)
            {
                return ServiceResponse<string>.Fail("User not found.", 404);
            }

            if (userExists.IsEmailVerified)
            {
                return ServiceResponse<string>.Fail("User email is already verified.", 409);
            }

            var existingRecord = await _authRepository.GetEmailVerificationRecords(userExists.Id);

            if (existingRecord != null && existingRecord.CodeType == CodeType.EmailVerification && existingRecord.IsVerified)
            {
                return ServiceResponse<string>.Fail("User email is already verified.", 409);
            }

            if (existingRecord != null && existingRecord.CodeType == CodeType.PasswordRecovery && existingRecord.IsVerified)
            {
                return ServiceResponse<string>.Fail("User email is already verified.", 409);
            }

            if (existingRecord != null && existingRecord.CodeType == CodeType.PasswordRecovery && existingRecord.ExpiresAt > DateTime.UtcNow)
            {
                return ServiceResponse<string>.Fail("A verification code has already been sent. Please wait until it expires.", 409);
            }

            var code = OtpHelper.GenerateCode();
            var codeHash = OtpHelper.HashCode(code);

            var record = new EmailVerification
            {
                UserId = userExists.Id,
                CodeHash = codeHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                CodeType = codeType,
                IsVerified = false
            };

            await _authRepository.AddEmailVerificationRecords(record);

            if (!await _authRepository.SaveChangesAsync())
            {
                return ServiceResponse<string>.Fail("Unable to add record to email verification table", 500);
            }

            var templatePath = Path.Combine(_env.ContentRootPath, "Resources/Templates", "EmailVerification.html");
            var htmlTemplate = await File.ReadAllTextAsync(templatePath);
            var body = htmlTemplate.Replace("{{CODE}}", code);

            if (string.IsNullOrWhiteSpace(userExists.Email)) throw new InvalidOperationException("User email is null. Cannot send verification code.");

            var emailResult = await _emailService.SendEmail(userExists.Email, "Verify your email", body);

            if (!emailResult.IsSuccess)
            {
                return ServiceResponse<string>.Fail($"{emailResult.Message}", 500);
            }

            return ServiceResponse<string>.Success(null, "Verification code send");
        }

        public async Task<ServiceResponse<string>> VerifyEmail(string email, string code)
        {
            //var record = await _dbContext.EmailVerifications
            //    .Where(c => c.Email == email && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
            //    .OrderByDescending(c => c.ExpiresAt)
            //    .FirstOrDefaultAsync();

            //if (record == null)
            //    return new ServiceResponse<string>
            //    {
            //        IsSuccess = false,
            //        ErrorMessage = "Invalid or expired code."
            //    };

            //if (record.CodeHash != OtpHelper.HashCode(code))
            //    return new ServiceResponse<string>
            //    {
            //        IsSuccess = false,
            //        ErrorMessage = "Invalid code."
            //    };

            //record.IsUsed = true;
            //await _dbContext.SaveChangesAsync();

            //return new ServiceResponse<string>
            //{
            //    IsSuccess = true,
            //    Data = "Email verified successfully!"
            //};
            throw new NotImplementedException();
        }
        public async Task<ServiceResponse<bool>> VerifyUsername(VerifyUsernameDTO usernameDTO)
        {
            //var exists = await _dbContext.Users
            //    .AnyAsync(u => u.UserName == usernameDTO.UserName);

            //if (exists)
            //{
            //    return new ServiceResponse<bool>
            //    {
            //        IsSuccess = false,
            //        Data = false,
            //        ErrorMessage = "Username is already in use."
            //    };
            //}

            //return new ServiceResponse<bool>
            //{
            //    IsSuccess = true,
            //    Data = true
            //};

            throw new NotImplementedException();
        }
    }
}
