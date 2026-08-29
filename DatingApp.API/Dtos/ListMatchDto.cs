using System;

namespace DatingApp.API.Dtos
{
    public class ListMatchDto
    {
        public int MatchId { get; set; }

        public DateTime MatchedAt { get; set; }

        public ListUserDto User { get; set; }
    }
}
