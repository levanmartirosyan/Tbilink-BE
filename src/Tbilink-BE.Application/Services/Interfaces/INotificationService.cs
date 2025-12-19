namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendPostLikeNotificationAsync(int recipientId, int actorId, int postId);
        Task SendCommentLikeNotificationAsync(int recipientId, int actorId, int postId, int commentId);
        Task SendNewCommentNotificationAsync(int recipientId, int actorId, int postId, int commentId);
        Task SendNewPostToFollowersAsync(int actorId, int postId, IEnumerable<int> followerIds);
    }
}
