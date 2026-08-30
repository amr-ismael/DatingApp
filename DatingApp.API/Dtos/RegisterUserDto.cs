using System;
using DatingApp.API.Shared;
using NetTopologySuite.Geometries;

namespace DatingApp.API.Dtos
{
    public class RegisterUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public Gender? Gender { get; set; }
        public Gender? InterestedIn { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Point Location { get; set; }
    }
}
