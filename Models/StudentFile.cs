namespace EduMastersAPI.Models
{
    public class StudentFile
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FileType { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
