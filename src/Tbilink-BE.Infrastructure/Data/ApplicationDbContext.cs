using Microsoft.EntityFrameworkCore;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserPhoto> UserPhotos { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Post>()
            //    .HasMany(p => p.LikedByUsers)     
            //    .WithMany(u => u.LikedPosts)      
            //    .UsingEntity(j => j.ToTable("UserPostLikes")); 

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.MessageSent)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Recipient)
                .WithMany(u => u.MessageReceived)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostLike>(entity =>
            {
                entity.HasKey(pl => pl.Id);

                entity.HasIndex(pl => new { pl.PostId, pl.UserId })
                      .IsUnique();

                entity.HasOne(pl => pl.Post)
                      .WithMany(p => p.Likes)
                      .HasForeignKey(pl => pl.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pl => pl.User)
                      .WithMany(u => u.PostLikes)
                      .HasForeignKey(pl => pl.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CommentLike>(entity =>
            {
                entity.HasKey(cl => cl.Id);

                entity.HasIndex(cl => new { cl.CommentId, cl.UserId })
                      .IsUnique();

                entity.HasOne(cl => cl.Comment)
                      .WithMany(c => c.Likes)
                      .HasForeignKey(cl => cl.CommentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cl => cl.User)
                      .WithMany(u => u.CommentLikes)
                      .HasForeignKey(cl => cl.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserFollow>(entity =>
            {
                entity.HasKey(uf => uf.Id);

                // Prevent duplicate follows
                entity.HasIndex(uf => new { uf.FollowerId, uf.FollowedId })
                      .IsUnique();

                // Follower relationship
                entity.HasOne(uf => uf.Follower)
                      .WithMany(u => u.Following)
                      .HasForeignKey(uf => uf.FollowerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Followed relationship
                entity.HasOne(uf => uf.Followed)
                      .WithMany(u => u.Followers)
                      .HasForeignKey(uf => uf.FollowedId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.HasOne(n => n.Recipient)
                      .WithMany()
                      .HasForeignKey(n => n.RecipientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Actor)
                      .WithMany()
                      .HasForeignKey(n => n.ActorId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(n => n.Type)
                      .HasConversion<int>(); 
            });
        }

    }
}
