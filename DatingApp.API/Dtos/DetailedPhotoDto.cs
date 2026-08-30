using System;
using DatingApp.API.Models;

namespace DatingApp.API.Dtos
{
    public class DetailedPhotoDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        public Guid UserId { get; set; }
    }
}