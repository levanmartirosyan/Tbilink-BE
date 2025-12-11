using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ServiceResponse<User>> GetUserInfoByEmail(ClaimsPrincipal userPrincipal)
        {
            var email = userPrincipal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                        ?? userPrincipal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return ServiceResponse<User>.Fail(null, "Missing email claim in token.", 401);
            }
                
            var user = await _userRepository.GetUserByEmail(email);

            if (user == null)
            {
                return ServiceResponse<User>.Fail(null, "User not found.", 404);
            }

            return ServiceResponse<User>.Success(user, "User data retrieved successfully.");
        }

        public async Task<ServiceResponse<User>> GetUserInfoById(int userId)
        {
            var user = await _userRepository.GetUserById(userId);

            if (user == null)
            {
                return ServiceResponse<User>.Fail(null, "User not found.", 404);
            }

            return ServiceResponse<User>.Success(user, "User data retrieved successfully.");
        }

        public async Task<ServiceResponse<User>> GetUserInfoByUsername(string username)
        {
            var user = await _userRepository.GetUserByUsername(username);

            if (user == null)
            {
                return ServiceResponse<User>.Fail(null, "User not found.", 404);
            }

            return ServiceResponse<User>.Success(user, "User data retrieved successfully.");
        }

        public async Task<ServiceResponse<string>> UpdateUserAsync(int currentUserId, int targetUserId, UpdateUserDTO updateUserDto)
        {
            try
            {
                if (updateUserDto == null)
                {
                    return ServiceResponse<string>.Fail(null, "Update data cannot be null.", 400);
                }

                var currentUser = await _userRepository.GetUserById(currentUserId);
                if (currentUser == null)
                {
                    return ServiceResponse<string>.Fail(null, "Current user not found.", 401);
                }

                var targetUser = await _userRepository.GetUserById(targetUserId);
                if (targetUser == null)
                {
                    return ServiceResponse<string>.Fail(null, "Target user not found.", 404);
                }

                if (!CanUserEditProfile(currentUser, targetUser))
                {
                    return ServiceResponse<string>.Fail(null, "You are not authorized to edit this user's profile.", 403);
                }

                if (!string.IsNullOrWhiteSpace(updateUserDto.Email) && updateUserDto.Email != targetUser.Email)
                {
                    var emailExists = await _userRepository.GetUserByEmail(updateUserDto.Email);
                    if (emailExists != null && emailExists.Id != targetUserId)
                    {
                        return ServiceResponse<string>.Fail(null, "Email is already in use.", 409);
                    }
                }

                if (!string.IsNullOrWhiteSpace(updateUserDto.UserName) && updateUserDto.UserName != targetUser.UserName)
                {
                    var usernameExists = await _userRepository.GetUserByUsername(updateUserDto.UserName);
                    if (usernameExists != null && usernameExists.Id != targetUserId)
                    {
                        return ServiceResponse<string>.Fail(null, "Username is already taken.", 409);
                    }
                }

                if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
                    targetUser.FirstName = updateUserDto.FirstName.Trim();

                if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
                    targetUser.LastName = updateUserDto.LastName.Trim();

                if (!string.IsNullOrWhiteSpace(updateUserDto.UserName))
                    targetUser.UserName = updateUserDto.UserName.Trim();

                if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
                    targetUser.Email = updateUserDto.Email.Trim().ToLower();

                if (updateUserDto.Country != null)
                    targetUser.Country = string.IsNullOrWhiteSpace(updateUserDto.Country) ? null : updateUserDto.Country.Trim();

                if (updateUserDto.City != null)
                    targetUser.City = string.IsNullOrWhiteSpace(updateUserDto.City) ? null : updateUserDto.City.Trim();

                if (updateUserDto.PhoneNumber != null)
                    targetUser.PhoneNumber = string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber) ? null : updateUserDto.PhoneNumber.Trim();

                if (updateUserDto.ProfilePhotoUrl != null)
                    targetUser.ProfilePhotoUrl = string.IsNullOrWhiteSpace(updateUserDto.ProfilePhotoUrl) ? null : updateUserDto.ProfilePhotoUrl.Trim();

                if (updateUserDto.CoverPhotoUrl != null)
                    targetUser.CoverPhotoUrl = string.IsNullOrWhiteSpace(updateUserDto.CoverPhotoUrl) ? null : updateUserDto.CoverPhotoUrl.Trim();

                if (updateUserDto.Description != null)
                    targetUser.Description = string.IsNullOrWhiteSpace(updateUserDto.Description) ? null : updateUserDto.Description.Trim();

                if (currentUser.Id == targetUser.Id || IsAdminOrOwner(currentUser))
                {
                    if (updateUserDto.IsPublicProfile.HasValue)
                        targetUser.IsPublicProfile = updateUserDto.IsPublicProfile.Value;

                    if (updateUserDto.ShowEmail.HasValue)
                        targetUser.ShowEmail = updateUserDto.ShowEmail.Value;

                    if (updateUserDto.ShowPhone.HasValue)
                        targetUser.ShowPhone = updateUserDto.ShowPhone.Value;

                    if (updateUserDto.AllowTagging.HasValue)
                        targetUser.AllowTagging = updateUserDto.AllowTagging.Value;

                    if (updateUserDto.EmailNotifications.HasValue)
                        targetUser.EmailNotifications = updateUserDto.EmailNotifications.Value;

                    if (updateUserDto.PushNotifications.HasValue)
                        targetUser.PushNotifications = updateUserDto.PushNotifications.Value;

                    if (updateUserDto.SmsNotifications.HasValue)
                        targetUser.SmsNotifications = updateUserDto.SmsNotifications.Value;

                    if (updateUserDto.MarketingEmails.HasValue)
                        targetUser.MarketingEmails = updateUserDto.MarketingEmails.Value;

                    if (!string.IsNullOrWhiteSpace(updateUserDto.Language))
                        targetUser.Language = updateUserDto.Language.Trim();

                    if (!string.IsNullOrWhiteSpace(updateUserDto.TimeZone))
                        targetUser.TimeZone = updateUserDto.TimeZone.Trim();
                }

                _userRepository.UpdateUser(targetUser);

                if (!await _userRepository.SaveChangesAsync())
                {
                    return ServiceResponse<string>.Fail(null, "Failed to update user.", 500);
                }

                return ServiceResponse<string>.Success(null, "User updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail(null, "An error occurred while updating user.", 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateUserAsync(int userId, UpdateUserDTO updateUserDto)
        {
            return await UpdateUserAsync(userId, userId, updateUserDto);
        }

        private bool CanUserEditProfile(User currentUser, User targetUser)
        {
            if (currentUser.Id == targetUser.Id)
            {
                return true;
            }

            if (IsAdminOrOwner(currentUser))
            {
                return true;
            }

            return false;
        }

        private bool IsAdminOrOwner(User user)
        {
            return user.Role == "Admin" || user.Role == "Owner";
        }
    }
}
 