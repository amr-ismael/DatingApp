using System;

namespace DatingApp.API.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int LowerUserId { get; set; }
        public User LowerUser { get; set; }
        public int HigherUserId { get; set; }
        public User HigherUser { get; set; }
        public DateTime MatchedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UnmatchedAt { get; set; }
    }
}
