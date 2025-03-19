using System;

namespace EduMastersAPI.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ResidenceCountry { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PassportNumber { get; set; }
        public decimal GPA { get; set; }
        public string GraduationSchool { get; set; }
        public string GraduationCountry { get; set; }
        public int SalesResponsibleId { get; set; }  // إضافة هذا العمود
        public string SalesResponsibleType { get; set; } // Sales Person, Agent Person, subAgent Person
        public string SalesResponsibleName { get; set; }
        public string SalesResponsibleProfileImageUrl { get; set; }

        // المستخدم الذي أضاف الطالب
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
