public class CreateEmployeeDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string UserType { get; set; }
    public string PhoneNumber { get; set; }
    public int? LinkedUserId { get; set; } // إذا كان هناك مدير للموظف
}
