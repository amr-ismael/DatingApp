using System;

namespace DatingApp.API.Models
{
    public class Match
    {
        public Guid Id { get; set; }
        public Guid LowerUserId { get; set; }
        public User LowerUser { get; set; }
        public Guid HigherUserId { get; set; }
        public User HigherUser { get; set; }
        public DateTime MatchedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UnmatchedAt { get; set; }
    }
}
