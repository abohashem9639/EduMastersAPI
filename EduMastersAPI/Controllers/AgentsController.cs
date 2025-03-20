using EduMastersAPI.Data;
using EduMastersAPI.DTOs;
using EduMastersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.IO;
using System;

namespace EduMastersAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AgentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 جلب قائمة الوكلاء
        [HttpGet]
        public async Task<IActionResult> GetAgents()
        {
            try
            {
                var agents = await _context.Users
                    .Where(u => u.UserType == "Agent")
                    .Select(u => new
                    {
                        u.Id,
                        FullName = $"{u.FirstName} {u.LastName}",
                        u.Email,
                        u.UserType,
                        u.ProfileImageUrl,
                        u.Verified,
                        u.PhoneNumber
                    })
                    .ToListAsync();

                return Ok(agents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error", Error = ex.Message });
            }
        }

        // 🔹 جلب الوكيل بناءً على الـ id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAgent(int id) // استقبل الـ id من الرابط
        {
            try
            {
                var agent = await _context.Users
                    .Where(u => u.Id == id) // ابحث عن الوكيل باستخدام الـ id
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.UserType,
                        u.ProfileImageUrl,
                        u.Verified,
                        u.PhoneNumber
                    })
                    .FirstOrDefaultAsync(); // استخدم FirstOrDefault لأنه سيعيد وكيل واحد فقط

                if (agent == null)
                {
                    return NotFound(new { Message = "Agent not found" }); // في حال عدم العثور على الوكيل
                }

                return Ok(agent); // أعد بيانات الوكيل
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error", Error = ex.Message });
            }
        }



        [HttpPut("update-agent/{id}")]
        public async Task<IActionResult> UpdateAgent(int id, [FromForm] CreateEmployeeDto dto, IFormFile profileImage, [FromForm] List<int> selectedTeamIds)
        {
            var agent = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.UserType == "Agent");
            if (agent == null)
                return NotFound("Agent not found.");

            // تحديث البيانات
            agent.FirstName = dto.FirstName;
            agent.LastName = dto.LastName;
            agent.Email = dto.Email;
            agent.PhoneNumber = dto.PhoneNumber;  // إضافة تحديث رقم الهاتف

            // إذا تم تحميل صورة جديدة
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

                agent.ProfileImageUrl = $"/uploads/{dto.Email}/{fileName}";
            }

            // حذف الفرق القديمة
            var existingTeamMembers = _context.TeamMembers.Where(tm => tm.UserId == agent.Id);
            _context.TeamMembers.RemoveRange(existingTeamMembers);

            if (selectedTeamIds != null)
            {
                if (selectedTeamIds.Count != 0)
                {
                    foreach (var teamId in selectedTeamIds)
                    {
                        var teamMember = new TeamMember
                        {
                            TeamId = teamId,
                            UserId = agent.Id
                        };
                        _context.TeamMembers.Add(teamMember);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Agent updated successfully",
                Agent = new
                {
                    agent.Id,
                    agent.Email,
                    agent.UserType,
                    agent.ProfileImageUrl,
                    agent.PhoneNumber
                }
            });
        }

        [HttpGet("get-teams")]
        public async Task<IActionResult> GetTeams()
        {
            var teams = await _context.Teams.Select(t => new { t.Id, t.Name }).ToListAsync();
            return Ok(teams);
        }


    }
}
