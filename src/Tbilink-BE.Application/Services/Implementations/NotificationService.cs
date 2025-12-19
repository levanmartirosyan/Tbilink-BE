using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Domain.Constants;
using Tbilink_BE.Domain.Entities;

namespace Tbilink_BE.Application.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task SendPostLikeNotificationAsync(int recipientId, int actorId, int postId)
        {
            if (recipientId == actorId) return;

            var notification = new Notification
            {
                RecipientId = recipientId,
                ActorId = actorId,
                PostId = postId,
                Type = NotificationType.LikePost,
                Message = "liked your post.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _notificationRepository.AddNotificationAsync(notification);
        }

        public async Task SendCommentLikeNotificationAsync(int recipientId, int actorId, int postId, int commentId)
        {
            if (recipientId == actorId) return;

            var notification = new Notification
            {
                RecipientId = recipientId,
                ActorId = actorId,
                PostId = postId,
                CommentId = commentId,
                Type = NotificationType.LikeComment,
                Message = "liked your comment.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _notificationRepository.AddNotificationAsync(notification);
        }

        public async Task SendNewCommentNotificationAsync(int recipientId, int actorId, int postId, int commentId)
        {
            if (recipientId == actorId) return;

            var notification = new Notification
            {
                RecipientId = recipientId,
                ActorId = actorId,
                PostId = postId,
                CommentId = commentId,
                Type = NotificationType.NewComment,
                Message = "commented on your post.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _notificationRepository.AddNotificationAsync(notification);
        }

        public async Task SendNewPostToFollowersAsync(int actorId, int postId, IEnumerable<int> followerIds)
        {
            foreach (var followerId in followerIds)
            {
                if (followerId == actorId) continue;
                var notification = new Notification
                {
                    RecipientId = followerId,
                    ActorId = actorId,
                    PostId = postId,
                    Type = NotificationType.NewPost,
                    Message = "created a new post.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                await _notificationRepository.AddNotificationAsync(notification);
            }
        }
    }
}
