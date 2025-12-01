using Microsoft.AspNetCore.Hosting;
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
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IWebHostEnvironment _env;

        public AuthService(IAuthRepository authRepository, IUserRepository userRepository, IEmailService emailService, ITokenProvider tokenProvider, IWebHostEnvironment env)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _tokenProvider = tokenProvider;
            _env = env;
        }

        public async Task<ServiceResponse<LoginResultDto>> Login(LoginDTO loginDTO)
        {
            if (loginDTO == null ||
                string.IsNullOrWhiteSpace(loginDTO.Email) ||
                string.IsNullOrWhiteSpace(loginDTO.Password))
            {
                return ServiceResponse<LoginResultDto>.Fail(null, "Email and password are required.");
            }

            if (!ValidationHelper.IsValidPassword(loginDTO.Password))
            {
                return ServiceResponse<LoginResultDto>.Fail(null, "Password must be at least 8 characters long and contain at least one uppercase letter, one number, and one special character.");
            }

            var userExist = await _userRepository.GetUserByEmail(loginDTO.Email.Trim());

            if (userExist == null)
            {
                return ServiceResponse<LoginResultDto>.Fail(null, "Invalid Email or Password.", 404);
            }

            if (!OtpHelper.VerifyPasswordHash(loginDTO.Password, userExist.PasswordHash, userExist.PasswordSalt))
            {
                return ServiceResponse<LoginResultDto>.Fail(null, "Invalid Email or Password.");
            }

            var userNotVerified = new UserDTO
            {
                Email = userExist.Email,
                IsEmailVerified = userExist.IsEmailVerified,
            };

            var emailNotVerifiedResult = new LoginResultDto
            {
                Data = userNotVerified
            };

            if (!userExist.IsEmailVerified)
            {
                return ServiceResponse<LoginResultDto>.Fail(emailNotVerifiedResult, "User's email is not verified.", 403);
            }

            var userModel = new UserDTO
            {
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

            _authRepository.SaveRefreshToken(userExist, refreshToken, refreshTokenExpiresAt);

            if (!await _authRepository.SaveChangesAsync())
            {
                return ServiceResponse<LoginResultDto>.Fail(null, "Unable to save refresh token.", 500);
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
                return ServiceResponse<RegisterResultDTO>.Fail(null, "Registration details required.");
            }

            if (string.IsNullOrWhiteSpace(registerDTO.FirstName))
            {
                return ServiceResponse<RegisterResultDTO>.Fail(null, "First name is required (min length 1).");
            }

            if (string.IsNullOrWhiteSpace(registerDTO.LastName))
            {
                return ServiceResponse<RegisterResultDTO>.Fail(null, "Last name is required (min length 1).");
            }

            if (!ValidationHelper.IsValidEmail(registerDTO.Email))
            {
                return ServiceResponse<RegisterResultDTO>.Fail(null, "Wrong email format.");
            }

            if (!ValidationHelper.IsValidPassword(registerDTO.Password))
            {
                return ServiceResponse<RegisterResultDTO>.Fail(null, "Password must be at least 8 characters long and contain at least one uppercase letter, one number, and one special character.");
            }

            if (!ValidationHelper.PasswordCompare(registerDTO.Password, registerDTO.RepPassword))
            {
                return ServiceResponse<RegisterResultDTO>.Fail(null, "Passwords do not match.");
            }

            var userExists = await _authRepository.UserExistCheck(registerDTO.Email);

            if (userExists)
            {
                return ServiceResponse<RegisterResultDTO>.Fail(null, "Email is already in use.", 409);
            }

            var usernameAvailable = await _authRepository.UsernameIsAvailable(registerDTO.UserName.ToLower().Trim());

            if (usernameAvailable)
            {
                return ServiceResponse<RegisterResultDTO>.Fail(null, "Username is already in use.", 409);
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
                return ServiceResponse<RegisterResultDTO>.Fail(null, "Failed to create user.", 500);
            }

            var sendCodeBody = new SendVerificationCodeDTO
            {
                Email = registerDTO.Email,
                CodeType = CodeType.EmailVerification
            };

            var senderResponse = await SendtVerificationCode(sendCodeBody);

            if (senderResponse != null)
            {
                return ServiceResponse<RegisterResultDTO>.Fail(null, $"{senderResponse.Message}", senderResponse.StatusCode);
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

        public async Task<ServiceResponse<string>> SendtVerificationCode(SendVerificationCodeDTO sendVerificationCodeDTO)
        {

            if (string.IsNullOrWhiteSpace(sendVerificationCodeDTO.Email) || !ValidationHelper.IsValidEmail(sendVerificationCodeDTO.Email))
            {
                return ServiceResponse<string>.Fail(null, "Invalid email format.");
            }

            if (sendVerificationCodeDTO.CodeType != CodeType.EmailVerification && sendVerificationCodeDTO.CodeType != CodeType.PasswordRecovery)
            {
                return ServiceResponse<string>.Fail(null, "Invalid code type.");
            }

            var userExists = await _userRepository.GetUserByEmail(sendVerificationCodeDTO.Email.Trim());

            if (userExists == null)
            {
                return ServiceResponse<string>.Fail(null, "User not found.", 404);
            }

            if (userExists.IsEmailVerified && sendVerificationCodeDTO.CodeType == CodeType.EmailVerification)
            {
                return ServiceResponse<string>.Fail(null, "User email is already verified.", 409);
            }

            var existingRecord = await _authRepository.GetEmailVerificationRecords(userExists.Id);

            if (existingRecord != null)
            {
                _authRepository.RemoveEmailVerificationRecord(existingRecord);

                if (!await _authRepository.SaveChangesAsync())
                {
                    return ServiceResponse<string>.Fail(null, "Unable to remove record from email verification table.", 500);
                }
            }

            var code = OtpHelper.GenerateCode();
            var codeHash = OtpHelper.HashCode(code);

            var record = new EmailVerification
            {
                UserId = userExists.Id,
                CodeHash = codeHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                CodeType = sendVerificationCodeDTO.CodeType,
                IsVerified = false
            };

            await _authRepository.AddEmailVerificationRecord(record);

            if (!await _authRepository.SaveChangesAsync())
            {
                return ServiceResponse<string>.Fail(null, "Unable to add record to email verification table.", 500);
            }

            var templatePath = Path.Combine(_env.WebRootPath, "Resources", "Templates", "EmailVerification.html");
            var htmlTemplate = await File.ReadAllTextAsync(templatePath);
            var body = htmlTemplate.Replace("{{CODE}}", code);

            if (string.IsNullOrWhiteSpace(userExists.Email)) throw new InvalidOperationException("User email is null. Cannot send verification code.");

            var emailResult = await _emailService.SendEmail(userExists.Email, "Email verification.", body);

            if (!emailResult.IsSuccess)
            {
                return ServiceResponse<string>.Fail(null, $"{emailResult.Message}", 500);
            }

            return ServiceResponse<string>.Success(null, "Verification code sent.");
        }

        public async Task<ServiceResponse<EmailVerificationResultDTO>> VerifyEmail(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "Invalid code format.");
            }

            if (!ValidationHelper.IsValidEmail(email))
            {
                return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "Invalid email format.");
            }

            var userExist = await _userRepository.GetUserByEmail(email.Trim());

            if (userExist == null)
            {
                return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "User not found.", 404);
            }

            var record = await _authRepository.GetEmailVerificationRecords(userExist.Id);

            if (record == null)
            {
                return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "Email verification record not found.", 404);
            }

            if (record.IsVerified)
            {
                return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "Code already used.", 409);
            }

            if (record.ExpiresAt < DateTime.UtcNow)
            {
                return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "Code expired.", 410);
            }

            if (record.CodeHash != OtpHelper.HashCode(code))
            {
                return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "Invalid code.");
            }

            record.IsVerified = true;

            if (record.CodeType == CodeType.EmailVerification)
            {
                if (record.User ==null )
                {
                    return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "User not found.", 404);
                }

                var user = await _userRepository.GetUserByEmail(record.User.Email);

                if (user == null)
                {
                    return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "User not found.", 404);
                }

                user.IsEmailVerified = true;

                _userRepository.UpdateUser(user);
            }

            _authRepository.UpdateEmailVerificationRecord(record);

            if (!await _authRepository.SaveChangesAsync())
            {
                return ServiceResponse<EmailVerificationResultDTO>.Fail(null, "Failed to update email verification record.", 500);
            }

            return ServiceResponse<EmailVerificationResultDTO>.Success(null, "Email verified successfully.");
        }

        public async Task<ServiceResponse<CreateNewPasswordDTO>> CreateNewPassword(CreateNewPasswordDTO createNewPasswordDTO)
        {
            if (createNewPasswordDTO == null)
            {
                return ServiceResponse<CreateNewPasswordDTO>.Fail(null, "New password fields are required.");
            }

            if (!ValidationHelper.IsValidEmail(createNewPasswordDTO.Email))
            {
                return ServiceResponse<CreateNewPasswordDTO>.Fail(null, "Invalid email type.");
            }

            var userExist = await _userRepository.GetUserByEmail(createNewPasswordDTO.Email.Trim());

            if ( userExist == null )
            {
                return ServiceResponse<CreateNewPasswordDTO>.Fail(null, "Email not found.", 404);
            }

            var record = await _authRepository.GetEmailVerificationRecords(userExist.Id);

            if (record == null)
            {
                return ServiceResponse<CreateNewPasswordDTO>.Fail(null, "Email verification record not found.", 404);
            }

            var hashedVerificationCode = OtpHelper.HashCode(createNewPasswordDTO.code);

            if (hashedVerificationCode != record.CodeHash)
            {
                return ServiceResponse<CreateNewPasswordDTO>.Fail(null, "Unexpected error.", 403);
            }

            if (record.ExpiresAt < DateTime.UtcNow)
            {
                return ServiceResponse<CreateNewPasswordDTO>.Fail(null, "Invalid or expired code.", 400);
            }

            if (!ValidationHelper.IsValidPassword(createNewPasswordDTO.Password))
            {
                return ServiceResponse<CreateNewPasswordDTO>.Fail(null, "Password must be at least 8 characters long and contain at least one uppercase letter, one number, and one special character.");
            }

            if (!ValidationHelper.PasswordCompare(createNewPasswordDTO.Password, createNewPasswordDTO.RepPassword))
            {
                return ServiceResponse<CreateNewPasswordDTO>.Fail(null, "Passwords do not match.");
            }

            OtpHelper.CreatePasswordHash(createNewPasswordDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

            userExist.PasswordHash = passwordHash;
            userExist.PasswordSalt = passwordSalt;

            _userRepository.UpdateUser(userExist);

            _authRepository.RemoveEmailVerificationRecord(record);

            if (!await _userRepository.SaveChangesAsync())
            {
                return ServiceResponse<CreateNewPasswordDTO>.Fail(null, "Failed to save new password.", 500);
            }

            return ServiceResponse<CreateNewPasswordDTO>.Success(null, "Password has been reset successfully.");
        }
    }
}
