namespace EduMastersAPI.DTOs
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }  // تأكد من وجود البريد الإلكتروني هنا
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string ResetToken { get; set; }  // التوكين
    }

}
