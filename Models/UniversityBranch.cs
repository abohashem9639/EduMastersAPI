using EduMastersAPI.Models;

public class UniversityBranch
{
    public int Id { get; set; }
    public int UniversityId { get; set; }
    public string BranchName { get; set; }
    public int DurationYears { get; set; }
    public decimal AnnualFee { get; set; }
    public string Levels { get; set; } // سيتم تخزين مستوى واحد في كل سجل
    public string Languages { get; set; } // سيتم تخزين لغة واحدة في كل سجل
    public decimal Price { get; set; }
    public decimal DiscountPrice { get; set; }
    public decimal CashPrice { get; set; }
    public string Currency { get; set; } // USD, EUR, TRY
    public decimal OurCommission { get; set; }
    public string Status { get; set; } // opening, closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // اجعل العلاقة اختيارية
    public University? University { get; set; }
}
