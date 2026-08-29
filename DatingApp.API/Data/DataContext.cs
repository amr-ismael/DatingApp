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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
}  