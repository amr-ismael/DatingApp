using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public interface ILikeRepository
    {
        Task<(IEnumerable<Like> Likes, string NextCursor)> GetLikes(Guid likerId, string cursor, int pageSize);
        Task<Like> GetLike(Guid likerId, Guid likeeId);
        Task<Like> CreateLike(Guid likerId, Guid likeeId);
    }

    public class LikeRepository : ILikeRepository
    {
        private readonly DataContext _context;

        public LikeRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Like> Likes, string NextCursor)> GetLikes(Guid likerId, string cursor, int pageSize)
        {
            IQueryable<Like> query = _context.Likes
                .Include(l => l.Likee).ThenInclude(u => u.Photos)
                .Where(l => l.LikerId == likerId);

            if (!string.IsNullOrEmpty(cursor))
            {
                var (createdAtTicks, id) = DecodeCursor(cursor);
                var createdAt = new DateTime(createdAtTicks);
                query = query.Where(l => l.CreatedAt < createdAt || (l.CreatedAt == createdAt && l.Id < id));
            }

            var likes = await query
                .OrderByDescending(l => l.CreatedAt)
                .ThenByDescending(l => l.Id)
                .Take(pageSize)
                .ToListAsync();

            string nextCursor = null;
            if (likes.Count == pageSize)
            {
                var last = likes[^1];
                nextCursor = EncodeCursor(last.CreatedAt.Ticks, last.Id);
            }

            return (likes, nextCursor);
        }

        public async Task<Like> GetLike(Guid likerId, Guid likeeId)
        {
            return await _context.Likes
                .FirstOrDefaultAsync(l => l.LikerId == likerId && l.LikeeId == likeeId);
        }

        public async Task<Like> CreateLike(Guid likerId, Guid likeeId)
        {
            var like = new Like
            {
                LikerId = likerId,
                LikeeId = likeeId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return like;
        }

        private static string EncodeCursor(long createdAtTicks, Guid id)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{createdAtTicks}_{id}"));
        }

        private static (long CreatedAtTicks, Guid Id) DecodeCursor(string cursor)
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = decoded.Split('_');
            return (long.Parse(parts[0]), Guid.Parse(parts[1]));
        }
    }
}
