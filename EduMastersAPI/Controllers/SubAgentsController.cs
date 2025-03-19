using EduMastersAPI.Data;
using EduMastersAPI.DTOs;
using EduMastersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class SubAgentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SubAgentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // جلب الـ sub-agent بناءً على الـ id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubAgent(int id)
    {
        var subAgent = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.UserType,
                u.ProfileImageUrl,
                u.Verified,
                u.PhoneNumber,
                LinkedAgent = u.LinkedUserId != null ? _context.Users.FirstOrDefault(x => x.Id == u.LinkedUserId) : null // استرجاع الوكيل المرتبط
            })
            .FirstOrDefaultAsync();

        if (subAgent == null)
            return NotFound(new { Message = "Sub-Agent not found" });

        return Ok(subAgent);
    }

    [HttpGet]
    public async Task<IActionResult> GetSubAgents()
    {
        try
        {
            var subAgents = await _context.Users
                .Where(u => u.UserType == "SubAgent")
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.UserType,
                    u.ProfileImageUrl,
                    u.Verified,
                    u.PhoneNumber,
                    LinkedAgent = u.LinkedUserId != null ? _context.Users
                    .Where(x => x.Id == u.LinkedUserId)
                    .Select(x => x.FirstName + " " + x.LastName) // جمع اسم الوكيل
                    .FirstOrDefault() : null

                })
                .ToListAsync();

            return Ok(subAgents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Internal Server Error", Error = ex.Message });
        }
    }



    // إضافة Sub-Agent جديد
    [HttpPost("create-sub-agent")]
    public async Task<IActionResult> CreateSubAgent([FromForm] CreateUserDto dto, IFormFile profileImage)
    {
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
            UserType = "SubAgent",
            LinkedUserId = dto.LinkedUserId,
            PhoneNumber = dto.PhoneNumber,
            ProfileImageUrl = profileImagePath,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString())
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Sub-Agent created successfully", User = newUser });
    }

    [HttpPut("update-sub-agent/{id}")]
    public async Task<IActionResult> UpdateSubAgent(int id, [FromForm] CreateUserDto dto, IFormFile profileImage)
    {
        // جلب الـ sub-agent من قاعدة البيانات باستخدام الـ ID
        var subAgent = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.UserType == "SubAgent");

        // التحقق مما إذا كان الـ sub-agent موجود
        if (subAgent == null)
        {
            return NotFound(new { Message = "Sub-Agent not found" });
        }

        // تحديث البيانات مع تجاهل كلمة المرور
        subAgent.FirstName = dto.FirstName;
        subAgent.LastName = dto.LastName;
        subAgent.Email = dto.Email;
        subAgent.PhoneNumber = dto.PhoneNumber;
        subAgent.LinkedUserId = dto.LinkedUserId;

        // التحقق من وجود صورة جديدة وتحديثها
        if (profileImage != null)
        {
            // تحديد مجلد رفع الصورة
            var uploadsFolder = Path.Combine("wwwroot", "uploads", dto.Email);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // إنشاء اسم جديد للملف
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // رفع الصورة إلى المجلد
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            // تحديث رابط الصورة في قاعدة البيانات
            subAgent.ProfileImageUrl = $"/uploads/{dto.Email}/{fileName}";
        }

        // إذا كان هناك وكيل مرتبط، قم بتحديثه
        if (dto.LinkedUserId.HasValue)
        {
            subAgent.LinkedUserId = dto.LinkedUserId.Value;
        }

        // حفظ التحديثات في قاعدة البيانات
        try
        {
            _context.Users.Update(subAgent);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Sub-Agent updated successfully", User = subAgent });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Internal Server Error", Error = ex.Message });
        }
    }


}
