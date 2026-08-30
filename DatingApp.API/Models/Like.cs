using System;

namespace DatingApp.API.Models
{
    public class Like
    {
        public Guid Id { get; set; }
        public Guid LikerId { get; set; }
        public User Liker { get; set; }
        public Guid LikeeId { get; set; }
        public User Likee { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
