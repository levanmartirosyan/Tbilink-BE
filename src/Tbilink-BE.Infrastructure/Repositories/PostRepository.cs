using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
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

        public async Task<List<Post>> GetAllPosts()
        {
            return _db.Posts.ToList();
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

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
