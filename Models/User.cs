using System;

namespace EduMastersAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } 
        public string UserType { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? LinkedUserId { get; set; } 
        public string? ProfileImageUrl { get; set; }
        public bool Verified { get; set; } = false; 
        public string PhoneNumber { get; set; } 

    }

}
