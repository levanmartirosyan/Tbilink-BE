using Microsoft.EntityFrameworkCore;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _db;

        public PostRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Post>> GetAllPosts(int? currentUserId)
        {
            var query = _db.Posts
                .Include(p => p.User)
                .AsQueryable();

            if (currentUserId.HasValue)
            {
                query = query.Include(p => p.Likes.Where(l => l.UserId == currentUserId.Value));
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post?> GetPostById(int postId) {
            return await _db.Posts.FindAsync(postId);
        }

        public async Task CreatePost(CreatePostDTO createPostDTO)
        {
            _db.Posts.Add(new Post
            {
                UserId = createPostDTO.UserId,
                Content = createPostDTO.Content,
                ImageUrl = createPostDTO.ImageUrl,
                CreatedAt = DateTime.UtcNow
            });
        }

        public void UpdatePost(Post post)
        {
            _db.Posts.Update(post);
        }

        public void DeletePost(Post post)
        {
            _db.Posts.Remove(post);
        }

        public async Task<PostLike?> GetPostLikeAsync(int postId, int userId)
        {
            return await _db.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);
        }

        public async Task<List<PostLike>> GetPostLikesAsync(int postId)
        {
            return await _db.PostLikes
                .Include(pl => pl.User)
                .Where(pl => pl.PostId == postId)
                .OrderByDescending(pl => pl.LikedAt)
                .ToListAsync();
        }

        public async Task<int> GetPostLikeCountAsync(int postId)
        {
            return await _db.PostLikes
                .CountAsync(pl => pl.PostId == postId);
        }

        public async Task<bool> HasUserLikedPostAsync(int postId, int userId)
        {
            return await _db.PostLikes
                .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
        }

        public async Task AddPostLikeAsync(PostLike postLike)
        {
            await _db.PostLikes.AddAsync(postLike);
        }

        public async Task RemovePostLikeAsync(PostLike postLike)
        {
            _db.PostLikes.Remove(postLike);
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            return await _db.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<List<Comment>> GetPostCommentsAsync(int postId, int? currentUserId)
        {
            var query = _db.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId)
                .AsQueryable();

            if (currentUserId.HasValue)
            {
                query = query.Include(c => c.Likes.Where(l => l.UserId == currentUserId.Value));
            }

            return await query
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetPostCommentCountAsync(int postId)
        {
            return await _db.Comments
                .CountAsync(c => c.PostId == postId);
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _db.Comments.AddAsync(comment);
        }

        public void UpdateComment(Comment comment)
        {
            _db.Comments.Update(comment);
        }

        public void DeleteComment(Comment comment)
        {
            _db.Comments.Remove(comment);
        }

        public async Task<CommentLike?> GetCommentLikeAsync(int commentId, int userId)
        {
            return await _db.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
        }

        public async Task<List<CommentLike>> GetCommentLikesAsync(int commentId)
        {
            return await _db.CommentLikes
                .Include(cl => cl.User)
                .Where(cl => cl.CommentId == commentId)
                .OrderByDescending(cl => cl.LikedAt)
                .ToListAsync();
        }

        public async Task<int> GetCommentLikeCountAsync(int commentId)
        {
            return await _db.CommentLikes
                .CountAsync(cl => cl.CommentId == commentId);
        }

        public async Task<bool> HasUserLikedCommentAsync(int commentId, int userId)
        {
            return await _db.CommentLikes
                .AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
        }

        public async Task AddCommentLikeAsync(CommentLike commentLike)
        {
            await _db.CommentLikes.AddAsync(commentLike);
        }

        public async Task RemoveCommentLikeAsync(CommentLike commentLike)
        {
            _db.CommentLikes.Remove(commentLike);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
