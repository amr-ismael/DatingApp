using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base (options){}
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Gender).HasConversion<string>();
                entity.Property(u => u.InterestedIn).HasConversion<string>();
                entity.Property(u => u.Location).HasColumnType("geography");
            });

            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasIndex(m => new { m.LowerUserId, m.HigherUserId }).IsUnique();
                entity.HasIndex(m => m.LowerUserId);
                entity.HasIndex(m => m.HigherUserId);
                entity.ToTable(t => t.HasCheckConstraint("CK_Match_OrderedIds", "[LowerUserId] < [HigherUserId]"));

                entity.HasOne(m => m.LowerUser)
                    .WithMany()
                    .HasForeignKey(m => m.LowerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.HigherUser)
                    .WithMany()
                    .HasForeignKey(m => m.HigherUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasIndex(l => new { l.LikerId, l.LikeeId }).IsUnique();
                entity.ToTable(t => t.HasCheckConstraint("CK_Like_NoSelfLike", "[LikerId] <> [LikeeId]"));

                entity.HasOne(l => l.Liker)
                    .WithMany()
                    .HasForeignKey(l => l.LikerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Likee)
                    .WithMany()
                    .HasForeignKey(l => l.LikeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}  