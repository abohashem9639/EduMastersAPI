public class UniversityBranchDto
{
    public int UniversityId { get; set; }
    public string BranchName { get; set; }
    public int DurationYears { get; set; }
    public decimal AnnualFee { get; set; }
    public List<string> Levels { get; set; } = new List<string>(); // قائمة بالمستويات
    public List<string> Languages { get; set; } = new List<string>(); // قائمة باللغات
    public decimal Price { get; set; }
    public decimal DiscountPrice { get; set; }
    public decimal CashPrice { get; set; }
    public string Currency { get; set; } // USD, EUR, TRY
    public decimal OurCommission { get; set; }
    public string Status { get; set; } // opening, closed
}
