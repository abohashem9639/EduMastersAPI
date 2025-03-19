using EduMastersAPI.Data;
using EduMastersAPI.DTOs;
using EduMastersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using System.Net;

namespace EduMastersAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return BadRequest("Invalid email or password.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("d2lc1x/cD9jtFkECV5A7TseQ+8wi5FncfMlFdUUe7ks=");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.UserType)
        }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            // إعادة كل بيانات المستخدم
            return Ok(new
            {
                Token = jwt,
                User = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.UserType,
                    user.LinkedUserId,
                    user.ProfileImageUrl,
                    user.CreatedAt
                }
            });
        }


        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromForm] CreateUserDto dto, IFormFile profileImage)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("Email and Password are required.");

            // ⚠️ إزالة تحقق "Admin" لأنك لا تريد المصادقة
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return BadRequest("A user with this email already exists.");

            string profileImagePath = null;
            if (profileImage != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "uploads", dto.Email);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                profileImagePath = $"/uploads/{dto.Email}/{fileName}";
            }

            var newUser = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                UserType = dto.UserType,
                LinkedUserId = dto.LinkedUserId, // ⚠️ لا يتم التحقق من الجلسة، يمكن تمريره مباشرة
                ProfileImageUrl = profileImagePath
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "User created successfully",
                User = new
                {
                    newUser.Id,
                    newUser.Email,
                    newUser.UserType,
                    newUser.LinkedUserId,
                    newUser.ProfileImageUrl
                }
            });
        }


        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("salahelden.abohashem@gmail.com", "ctorvgekohuhdgkf"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("salahelden.abohashem@gmail.com", "EduMasters"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email: {ex.Message}");
                return false;
            }
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return BadRequest("User not found.");

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.Verified = true; 

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password has been set successfully!" });
        }


        [HttpPost("create-agent")]
        public async Task<IActionResult> CreateAgent([FromForm] CreateEmployeeDto dto, IFormFile profileImage, [FromForm] List<int> selectedTeamIds)
        {
            Console.WriteLine($"📥 Received: {dto.FirstName} - {dto.LastName} - {dto.Email} - {dto.UserType} - {dto.PhoneNumber}");

            if (string.IsNullOrEmpty(dto.Email))
                return BadRequest("Email is required.");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return BadRequest("A user with this email already exists.");

            string profileImagePath = null;
            if (profileImage == null)
            {
                return BadRequest("Profile image is required.");
            }

            if (profileImage != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "uploads", dto.Email);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                profileImagePath = $"/uploads/{dto.Email}/{fileName}";
            }

            var newAgent = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserType = "Agent",
                LinkedUserId = dto.LinkedUserId,
                ProfileImageUrl = profileImagePath,
                PhoneNumber = dto.PhoneNumber, // تعيين رقم الهاتف
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()) // تعيين كلمة مرور مؤقتة
            };

            _context.Users.Add(newAgent);
            await _context.SaveChangesAsync();

            // ربط الوكيل بالفرق المحددة
            foreach (var teamId in selectedTeamIds)
            {
                var teamMember = new TeamMember
                {
                    TeamId = teamId,
                    UserId = newAgent.Id
                };
                _context.TeamMembers.Add(teamMember);
            }

            await _context.SaveChangesAsync();

            // 🔹 إنشاء رابط إعادة تعيين كلمة المرور
            string resetLink = $"http://localhost:3000/reset-password?email={Uri.EscapeDataString(dto.Email)}";
            string emailBody = $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <h2>Hello {newAgent.FirstName},</h2>
            <p>You have been added as an agent to <strong>EduMasters</strong>.</p>
            <p>Click the button below to create your password:</p>
            <a href='{resetLink}' style='display: inline-block; padding: 10px 15px; background-color: #007bff; color: white; text-decoration: none; font-size: 16px; border-radius: 5px;'>Create Password</a>
            <p>If you didn't request this, please ignore this email.</p>
            <p>Best regards,<br>EduMasters Team</p>
        </body>
        </html>";

            // 🔹 إرسال البريد الإلكتروني
            bool emailSent = await SendEmailAsync(newAgent.Email, "Create Your Password", emailBody);

            if (!emailSent)
            {
                return StatusCode(500, new { Message = "Agent created, but failed to send email." });
            }

            return Ok(new
            {
                Message = "Agent created successfully",
                User = new
                {
                    newAgent.Id,
                    newAgent.Email,
                    newAgent.UserType,
                    newAgent.LinkedUserId,
                    newAgent.ProfileImageUrl,
                    newAgent.PhoneNumber // إضافة رقم الهاتف
                }
            });
        }

        [HttpPost("resend-reset-link")]
        public async Task<IActionResult> ResendResetLink([FromBody] ResendResetLinkDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.Verified)
                return BadRequest("User not found or already verified.");

            // إنشاء رابط إعادة تعيين كلمة المرور
            string resetLink = $"http://localhost:3000/reset-password?email={Uri.EscapeDataString(dto.Email)}";
            string emailBody = $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <h2>Hello {user.FirstName},</h2>
            <p>Click the button below to create your password:</p>
            <a href='{resetLink}' style='display: inline-block; padding: 10px 15px; background-color: #007bff; color: white; text-decoration: none; font-size: 16px; border-radius: 5px;'>Create Password</a>
            <p>If you didn't request this, please ignore this email.</p>
            <p>Best regards,<br>EduMasters Team</p>
        </body>
        </html>";

            bool emailSent = await SendEmailAsync(user.Email, "Create Your Password", emailBody);

            if (!emailSent)
                return StatusCode(500, new { Message = "Failed to send reset email." });

            return Ok(new { Message = "Reset link sent successfully!" });
        }



    }
}
