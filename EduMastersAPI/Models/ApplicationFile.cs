namespace EduMastersAPI.Models
{
    public class ApplicationFile
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public int CreatedByUserId { get; set; }
        public virtual User CreatedByUser { get; set; } // العلاقة مع User
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual Application Application { get; set; }
    }




}
