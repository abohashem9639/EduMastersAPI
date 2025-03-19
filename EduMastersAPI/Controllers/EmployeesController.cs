using EduMastersAPI.Data;
using EduMastersAPI.DTOs;
using EduMastersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _context.Users
            .Where(u => u.UserType != "SubAgent" && u.UserType != "Agent")
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.UserType,
                u.ProfileImageUrl,
                u.Verified,
                u.LinkedUserId,
                u.PhoneNumber
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpPost("create-employee")]
    public async Task<IActionResult> CreateEmployee([FromForm] CreateEmployeeDto dto, IFormFile profileImage)
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

        var newUser = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserType = dto.UserType,
            LinkedUserId = dto.LinkedUserId,
            ProfileImageUrl = profileImagePath,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString())
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        string resetLink = $"http://localhost:3000/reset-password?email={Uri.EscapeDataString(dto.Email)}";
        string emailBody = $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <h2>Hello {newUser.FirstName},</h2>
            <p>You have been added as an employee to <strong>EduMasters</strong>.</p>
            <p>Click the button below to create your password:</p>
            <a href='{resetLink}' style='display: inline-block; padding: 10px 15px; background-color: #007bff; color: white; text-decoration: none; font-size: 16px; border-radius: 5px;'>Create Password</a>
            <p>If you didn't request this, please ignore this email.</p>
            <p>Best regards,<br>EduMasters Team</p>
        </body>
        </html>";

        bool emailSent = await SendEmailAsync(newUser.Email, "Create Your Password", emailBody);

        if (!emailSent)
        {
            return StatusCode(500, new { Message = "User created, but failed to send email." });
        }

        return Ok(new
        {
            Message = "Employee created successfully, and an email has been sent!",
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
                Credentials = new NetworkCredential("prime01010108@gmail.com", "veorfwcumoejepca"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("prime01010108@gmail.com", "EduMasters"),
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromForm] CreateUserDto dto, IFormFile profileImage = null)
    {
        var employee = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (employee == null)
            return NotFound(new { Message = "Employee not found" });

        // تحديث الحقول التي تم إرسالها فقط
        employee.FirstName = dto.FirstName ?? employee.FirstName;
        employee.LastName = dto.LastName ?? employee.LastName;
        employee.Email = dto.Email ?? employee.Email;
        employee.PhoneNumber = dto.PhoneNumber ?? employee.PhoneNumber;
        employee.UserType = dto.UserType ?? employee.UserType;

        // تحديث صورة الملف إذا كانت موجودة
        if (profileImage != null)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
            var filePath = Path.Combine("wwwroot", "uploads", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }
            employee.ProfileImageUrl = $"/uploads/{fileName}";
        }

        // إذا لم يتم إرسال كلمة المرور، لا نقوم بتحديثها
        if (!string.IsNullOrEmpty(dto.Password))
        {
            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        _context.Users.Update(employee);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Employee updated successfully" });
    }




    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Users.FindAsync(id);
        if (employee == null)
            return NotFound(new { Message = "Employee not found" });

        _context.Users.Remove(employee);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Employee deleted successfully" });
    }

    //[HttpPut("{id}/toggle-status")]
    //public async Task<IActionResult> ToggleEmployeeStatus(int id)
    //{
    //    var employee = await _context.Users.FindAsync(id);
    //    if (employee == null)
    //        return NotFound(new { Message = "Employee not found" });

    //    employee.Status = !employee.Status;
    //    _context.Users.Update(employee);
    //    await _context.SaveChangesAsync();

    //    return Ok(new { Message = "Employee status toggled successfully" });
    //}
}
