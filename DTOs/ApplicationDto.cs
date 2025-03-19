namespace EduMastersAPI.DTOs
{
    public class ApplicationDto
    {
        public int StudentId { get; set; }
        public string Degree { get; set; }
        public int UniversityId { get; set; }
        public string Language { get; set; }
        public int BranchId { get; set; }
        public int CreatedByUserId { get; set; }
        public string Status { get; set; } 
    }

    public class ApplicationStatusDto
    {
        public string Status { get; set; } 
    }
}
