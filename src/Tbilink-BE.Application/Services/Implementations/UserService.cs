using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using Tbilink_BE.Application.Common;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;
using Tbilink_BE.Services.Helpers;

namespace Tbilink_BE.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;

        public UserService(IUserRepository userRepository, IPostRepository postRepository)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
        }

        #region User Methods
        public async Task<ServiceResponse<string>> RemoveUser(int currentUserId, int userId)
        {
            try
            {
                if (userId < 0)
                {
                    return ServiceResponse<string>.Fail(null, "Invalid user ID.", 400);
                }

                var userToDelete = await _userRepository.GetUserById(userId);
                if (userToDelete == null)
                {
                    return ServiceResponse<string>.Fail(null, "User not found.", 404);
                }

                var currentUser = await _userRepository.GetUserById(currentUserId);
                if (currentUser == null)
                {
                    return ServiceResponse<string>.Fail(null, "Current user not found.", 401);
                }

                if (currentUser.Id != userId && !IsAdminOrOwner(currentUser))
                {
                    return ServiceResponse<string>.Fail(null, "You are not allowed to delete this user.", 403);
                }

                _userRepository.RemoveUser(userToDelete);

                if (!await _userRepository.SaveChangesAsync())
                {
                    return ServiceResponse<string>.Fail(null, "Failed to delete user.", 500);
                }

                return ServiceResponse<string>.Success(null, "User deleted successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail(null, "An error occurred while deleting the user.", 500);
            }
        }

        public async Task<ServiceResponse<string>> ChangePasswordAsync(int userId, ChangePasswordDTO dto)
        {
            if (dto == null)
                return ServiceResponse<string>.Fail(null, "Invalid request.", 400);

            if (string.IsNullOrWhiteSpace(dto.OldPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                string.IsNullOrWhiteSpace(dto.RepeatNewPassword))
            {
                return ServiceResponse<string>.Fail(null, "All password fields are required.", 400);
            }

            if (!ValidationHelper.IsValidPassword(dto.NewPassword))
            {
                return ServiceResponse<string>.Fail(null, "Password must be at least 8 characters long and contain at least one uppercase letter, one number, and one special character.", 400);
            }

            if (dto.NewPassword != dto.RepeatNewPassword)
            {
                return ServiceResponse<string>.Fail(null, "New passwords do not match.", 400);
            }

            var user = await _userRepository.GetUserById(userId);
            if (user == null)
                return ServiceResponse<string>.Fail(null, "User not found.", 404);

            if (!OtpHelper.VerifyPasswordHash(dto.OldPassword, user.PasswordHash, user.PasswordSalt))
            {
                return ServiceResponse<string>.Fail(null, "Old password is incorrect.", 400);
            }

            OtpHelper.CreatePasswordHash(dto.NewPassword, out byte[] newHash, out byte[] newSalt);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;

            _userRepository.UpdateUser(user);

            if (!await _userRepository.SaveChangesAsync())
                return ServiceResponse<string>.Fail(null, "Failed to change password.", 500);

            return ServiceResponse<string>.Success(null, "Password changed successfully.");
        }

        #endregion

        #region User Info Methods
        public async Task<ServiceResponse<User>> GetUserInfoByEmail(string email)
        {

            if (string.IsNullOrEmpty(email))
            {
                return ServiceResponse<User>.Fail(null, "Missing email.", 401);
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

                if (!string.IsNullOrWhiteSpace(updateUserDto.Role) && IsAdminOrOwner(currentUser))
                {
                    targetUser.Role = updateUserDto.Role.Trim();
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

#endregion

        #region User Follow Methods
        public async Task<ServiceResponse<string>> ToggleFollowUserAsync(int currentUserId, int targetUserId)
        {
            try
            {
                if (currentUserId == targetUserId)
                {
                    return ServiceResponse<string>.Fail(null, "You cannot follow yourself.", 400);
                }

                var currentUser = await _userRepository.GetUserById(currentUserId);
                var targetUser = await _userRepository.GetUserById(targetUserId);

                if (currentUser == null || targetUser == null)
                {
                    return ServiceResponse<string>.Fail(null, "User not found.", 404);
                }

                var existingFollow = await _userRepository.GetFollowAsync(currentUserId, targetUserId);

                if (existingFollow != null)
                {
                    // Unfollow
                    await _userRepository.RemoveFollowAsync(existingFollow);
                    currentUser.FollowingCount = Math.Max(0, currentUser.FollowingCount - 1);
                    targetUser.FollowersCount = Math.Max(0, targetUser.FollowersCount - 1);
                }
                else
                {
                    // Follow
                    var userFollow = new UserFollow
                    {
                        FollowerId = currentUserId,
                        FollowedId = targetUserId,
                        FollowedAt = DateTime.UtcNow
                    };

                    await _userRepository.AddFollowAsync(userFollow);
                    currentUser.FollowingCount += 1;
                    targetUser.FollowersCount += 1;

                    // Send notification to target user
                    // await _notificationService.SendFollowNotificationAsync(targetUserId, currentUserId);
                }

                _userRepository.UpdateUser(currentUser);
                _userRepository.UpdateUser(targetUser);

                if (await _userRepository.SaveChangesAsync())
                {
                    var action = existingFollow != null ? "unfollowed" : "followed";
                    return ServiceResponse<string>.Success(null, $"User {action} successfully.");
                }

                return ServiceResponse<string>.Fail(null, "Failed to update follow status.", 500);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail(null, "An error occurred while updating follow status.", 500);
            }
        }

        public async Task<ServiceResponse<List<UserFollowDTO>>> GetFollowersAsync(int userId, int? currentUserId = null)
        {
            try
            {
                var followers = await _userRepository.GetUserFollowersAsync(userId);

                var followerDtos = new List<UserFollowDTO>();

                foreach (var follow in followers)
                {
                    var isFollowingBack = currentUserId.HasValue ?
                        await _userRepository.IsFollowingAsync(currentUserId.Value, follow.FollowerId) : false;

                    followerDtos.Add(new UserFollowDTO
                    {
                        Id = follow.Follower.Id,
                        FirstName = follow.Follower.FirstName,
                        LastName = follow.Follower.LastName,
                        UserName = follow.Follower.UserName,
                        ProfilePhotoUrl = follow.Follower.ProfilePhotoUrl,
                        LastActive = follow.Follower.LastActive,
                        FollowedAt = follow.FollowedAt,
                        IsFollowingBack = isFollowingBack
                    });
                }

                return ServiceResponse<List<UserFollowDTO>>.Success(followerDtos, "Followers retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<UserFollowDTO>>.Fail(null, "An error occurred while retrieving followers.", 500);
            }
        }

        public async Task<ServiceResponse<List<UserFollowDTO>>> GetFollowingAsync(int userId, int? currentUserId = null)
        {
            try
            {
                var following = await _userRepository.GetUserFollowingAsync(userId);

                var followingDtos = new List<UserFollowDTO>();

                foreach (var follow in following)
                {
                    var isFollowingBack = currentUserId.HasValue ?
                        await _userRepository.IsFollowingAsync(follow.FollowedId, currentUserId.Value) : false;

                    followingDtos.Add(new UserFollowDTO
                    {
                        Id = follow.Followed.Id,
                        FirstName = follow.Followed.FirstName,
                        LastName = follow.Followed.LastName,
                        UserName = follow.Followed.UserName,
                        ProfilePhotoUrl = follow.Followed.ProfilePhotoUrl,
                        LastActive = follow.Followed.LastActive,
                        FollowedAt = follow.FollowedAt,
                        IsFollowingBack = isFollowingBack
                    });
                }

                return ServiceResponse<List<UserFollowDTO>>.Success(followingDtos, "Following list retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<UserFollowDTO>>.Fail(null, "An error occurred while retrieving following list.", 500);
            }
        }

        public async Task<ServiceResponse<List<UserFollowDTO>>> GetMutualFollowsAsync(int userId)
        {
            try
            {
                var mutualFollows = await _userRepository.GetMutualFollowsAsync(userId);

                var mutualFollowDtos = mutualFollows.Select(follow => new UserFollowDTO
                {
                    Id = follow.Followed.Id,
                    FirstName = follow.Followed.FirstName,
                    LastName = follow.Followed.LastName,
                    UserName = follow.Followed.UserName,
                    ProfilePhotoUrl = follow.Followed.ProfilePhotoUrl,
                    LastActive = follow.Followed.LastActive,
                    FollowedAt = follow.FollowedAt,
                    IsFollowingBack = true 
                }).ToList();

                return ServiceResponse<List<UserFollowDTO>>.Success(mutualFollowDtos, "Mutual follows retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<UserFollowDTO>>.Fail(null, "An error occurred while retrieving mutual follows.", 500);
            }
        }

        public async Task<ServiceResponse<FollowStatsDTO>> GetFollowStatsAsync(int userId, int? currentUserId = null)
        {
            try
            {
                var user = await _userRepository.GetUserById(userId);
                if (user == null)
                {
                    return ServiceResponse<FollowStatsDTO>.Fail(null, "User not found.", 404);
                }

                var isFollowing = currentUserId.HasValue ?
                    await _userRepository.IsFollowingAsync(currentUserId.Value, userId) : false;

                var isFollowedBy = currentUserId.HasValue ?
                    await _userRepository.IsFollowingAsync(userId, currentUserId.Value) : false;

                var stats = new FollowStatsDTO
                {
                    FollowersCount = user.FollowersCount,
                    FollowingCount = user.FollowingCount,
                    IsFollowing = isFollowing,
                    IsFollowedBy = isFollowedBy
                };

                return ServiceResponse<FollowStatsDTO>.Success(stats, "Follow stats retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<FollowStatsDTO>.Fail(null, "An error occurred while retrieving follow stats.", 500);
            }
        }
        #endregion

        #region Search Methods
        public async Task<ServiceResponse<SearchResultDTO>> SearchAsync(SearchRequestDTO searchRequest, int? currentUserId = null)
        {
            try
            {
                if (searchRequest == null)
                {
                    return ServiceResponse<SearchResultDTO>.Fail(null, "Search request cannot be null.", 400);
                }

                if (!IsValidCategory(searchRequest.Category))
                {
                    return ServiceResponse<SearchResultDTO>.Fail(null, "Invalid category. Must be 'all', 'posts', or 'people'.", 400);
                }

                if (searchRequest.Page < 1)
                {
                    searchRequest.Page = 1;
                }

                if (searchRequest.PageSize < 1 || searchRequest.PageSize > 50)
                {
                    searchRequest.PageSize = 10;
                }

                var result = new SearchResultDTO();

                switch (searchRequest.Category.ToLower())
                {
                    case "all":
                        result = await SearchAllAsync(searchRequest, currentUserId);
                        break;
                    case "people":
                        result = await SearchPeopleAsync(searchRequest, currentUserId);
                        break;
                    case "posts":
                        result = await SearchPostsAsync(searchRequest, currentUserId);
                        break;
                }

                return ServiceResponse<SearchResultDTO>.Success(result, "Search completed successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<SearchResultDTO>.Fail(null, "An error occurred during search.", 500);
            }
        }

        private async Task<SearchResultDTO> SearchAllAsync(SearchRequestDTO searchRequest, int? currentUserId)
        {
            var users = await _userRepository.SearchUsersAsync(searchRequest.Keyword, 1, 10);
            var posts = await _postRepository.SearchPostsAsync(searchRequest.Keyword, 1, 10, currentUserId);

            var userDtos = new List<UserSearchResultDTO>();
            foreach (var user in users)
            {
                var isFollowed = currentUserId.HasValue ?
                    await _userRepository.IsFollowingAsync(currentUserId.Value, user.Id) : false;

                userDtos.Add(new UserSearchResultDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    FollowersCount = user.FollowersCount,
                    IsFollowedByCurrentUser = isFollowed,
                });
            }

            var postDtos = posts.Select(p => new PostSearchResultDTO
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount,
                Username = p.User.UserName,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Avatar = p.User.ProfilePhotoUrl,
                UserId = p.User.Id,
                IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value)
            }).ToList();

            var totalUsers = await _userRepository.GetSearchUsersCountAsync(searchRequest.Keyword);
            var totalPosts = await _postRepository.GetSearchPostsCountAsync(searchRequest.Keyword);

            return new SearchResultDTO
            {
                Users = userDtos,
                Posts = postDtos,
                Pagination = new SearchPaginationDTO
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalUsers = totalUsers,
                    TotalPosts = totalPosts,
                    HasNextPage = totalUsers > 10 || totalPosts > 10,
                    HasPreviousPage = false,
                    Category = searchRequest.Category
                }
            };
        }

        private async Task<SearchResultDTO> SearchPeopleAsync(SearchRequestDTO searchRequest, int? currentUserId)
        {
            var users = await _userRepository.SearchUsersAsync(searchRequest.Keyword, searchRequest.Page, searchRequest.PageSize);
            var totalUsers = await _userRepository.GetSearchUsersCountAsync(searchRequest.Keyword);

            var userDtos = new List<UserSearchResultDTO>();
            foreach (var user in users)
            {
                var isFollowed = currentUserId.HasValue ?
                    await _userRepository.IsFollowingAsync(currentUserId.Value, user.Id) : false;

                userDtos.Add(new UserSearchResultDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    FollowersCount = user.FollowersCount,
                    IsFollowedByCurrentUser = isFollowed,
                });
            }

            return new SearchResultDTO
            {
                Users = userDtos,
                Posts = new List<PostSearchResultDTO>(), 
                Pagination = new SearchPaginationDTO
                {
                    CurrentPage = searchRequest.Page,
                    PageSize = searchRequest.PageSize,
                    TotalUsers = totalUsers,
                    TotalPosts = 0,
                    HasNextPage = (searchRequest.Page * searchRequest.PageSize) < totalUsers,
                    HasPreviousPage = searchRequest.Page > 1,
                    Category = searchRequest.Category
                }
            };
        }

        private async Task<SearchResultDTO> SearchPostsAsync(SearchRequestDTO searchRequest, int? currentUserId)
        {
            var posts = await _postRepository.SearchPostsAsync(searchRequest.Keyword, searchRequest.Page, searchRequest.PageSize, currentUserId);
            var totalPosts = await _postRepository.GetSearchPostsCountAsync(searchRequest.Keyword);

            var postDtos = posts.Select(p => new PostSearchResultDTO
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount,
                Username = p.User.UserName,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Avatar = p.User.ProfilePhotoUrl,
                UserId = p.User.Id,
                IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value)
            }).ToList();

            return new SearchResultDTO
            {
                Users = new List<UserSearchResultDTO>(), 
                Posts = postDtos,
                Pagination = new SearchPaginationDTO
                {
                    CurrentPage = searchRequest.Page,
                    PageSize = searchRequest.PageSize,
                    TotalUsers = 0,
                    TotalPosts = totalPosts,
                    HasNextPage = (searchRequest.Page * searchRequest.PageSize) < totalPosts,
                    HasPreviousPage = searchRequest.Page > 1,
                    Category = searchRequest.Category
                }
            };
        }

        private bool IsValidCategory(string category)
        {
            var validCategories = new[] { "all", "posts", "people" };
            return validCategories.Contains(category.ToLower());
        }
        #endregion
    }
}
 