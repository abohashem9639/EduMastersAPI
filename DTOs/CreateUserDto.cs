namespace EduMastersAPI.DTOs
{
    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public string PhoneNumber { get; set; }
        public string UserType { get; set; } // Admin, Employee, Agent, SubAgent
        public int? LinkedUserId { get; set; } // For linking SubAgent/Agent accounts
        public string? ProfileImageUrl { get; set; }
    }
}
