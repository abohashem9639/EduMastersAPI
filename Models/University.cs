public class University
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string City { get; set; } // ✅ إضافة المدينة
    public string UniversityType { get; set; } // ✅ عامة أو خاصة
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LogoUrl { get; set; }

    public ICollection<UniversityBranch> Branches { get; set; } = new List<UniversityBranch>();
}
