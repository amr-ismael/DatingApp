using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public interface IMatchRepository
    {
        Task<(IEnumerable<Match> Matches, string NextCursor)> GetMatches(int userId, string cursor, int pageSize);
        Task<Match> GetMatch(int id);
        Task<Match> CreateMatch(int userAId, int userBId);
        Task<bool> SaveAll();
    }

    public class MatchRepository : IMatchRepository
    {
        private readonly DataContext _context;

        public MatchRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Match> Matches, string NextCursor)> GetMatches(int userId, string cursor, int pageSize)
        {
            var query = _context.Matches
                .Include(m => m.LowerUser).ThenInclude(u => u.Photos)
                .Include(m => m.HigherUser).ThenInclude(u => u.Photos)
                .Where(m => m.IsActive && (m.LowerUserId == userId || m.HigherUserId == userId));

            if (!string.IsNullOrEmpty(cursor))
            {
                var (matchedAtTicks, id) = DecodeCursor(cursor);
                var matchedAt = new DateTime(matchedAtTicks);
                query = query.Where(m => m.MatchedAt < matchedAt || (m.MatchedAt == matchedAt && m.Id < id));
            }

            var matches = await query
                .OrderByDescending(m => m.MatchedAt)
                .ThenByDescending(m => m.Id)
                .Take(pageSize)
                .ToListAsync();

            string nextCursor = null;
            if (matches.Count == pageSize)
            {
                var last = matches[^1];
                nextCursor = EncodeCursor(last.MatchedAt.Ticks, last.Id);
            }

            return (matches, nextCursor);
        }

        public async Task<Match> GetMatch(int id)
        {
            return await _context.Matches
                .Include(m => m.LowerUser)
                .Include(m => m.HigherUser)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Match> CreateMatch(int userAId, int userBId)
        {
            var lowerId = Math.Min(userAId, userBId);
            var higherId = Math.Max(userAId, userBId);

            var existing = await _context.Matches
                .FirstOrDefaultAsync(m => m.LowerUserId == lowerId && m.HigherUserId == higherId);
            if (existing != null)
            {
                return existing;
            }

            var match = new Match
            {
                LowerUserId = lowerId,
                HigherUserId = higherId,
                MatchedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
            return match;
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        private static string EncodeCursor(long matchedAtTicks, int id)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{matchedAtTicks}_{id}"));
        }

        private static (long MatchedAtTicks, int Id) DecodeCursor(string cursor)
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = decoded.Split('_');
            return (long.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}
