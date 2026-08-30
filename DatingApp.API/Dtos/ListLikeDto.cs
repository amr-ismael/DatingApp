using System;

namespace DatingApp.API.Dtos
{
    public class ListLikeDto
    {
        public Guid LikeId { get; set; }

        public DateTime CreatedAt { get; set; }

        public ListUserDto User { get; set; }
    }
}
