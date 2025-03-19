namespace EduMastersAPI.Models
{
    public class Announcement
    {
        public int id { get; set; }  // نغيرها إلى int بدلاً من string
        public int university_id { get; set; }
        public string announcement_text { get; set; }
        public DateTime created_at { get; set; }
        public int created_by { get; set; }

        public University? University { get; set; }
        public User? CreatedByUser { get; set; }
    }

}
