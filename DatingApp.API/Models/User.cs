using System;
using System.Collections.Generic;
using DatingApp.API.Shared;
using NetTopologySuite.Geometries;

namespace DatingApp.API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public Gender Gender { get; set; }
        public Gender InterestedIn { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Bio { get; set; }
        public string[] Interests { get; set; }
        public Point Location { get; set; }
        public ICollection<Photo> Photos{ get; set; }
    }
    
}