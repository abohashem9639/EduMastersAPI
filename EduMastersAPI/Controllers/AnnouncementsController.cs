using EduMastersAPI.Data;
using EduMastersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("[controller]")]
[ApiController]
public class AnnouncementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AnnouncementsController(ApplicationDbContext context)
    {
        _context = context;
    }

[HttpGet("all")]
public async Task<IActionResult> GetAllAnnouncements()
{
    var announcements = await _context.Announcements
        .Include(a => a.University)
        .Include(a => a.CreatedByUser)  
        .ToListAsync();

    var result = announcements.Select(a => new 
    {
        a.id,
        UniversityName = a.University.Name,  
        universityLogo = a.University.LogoUrl,  
        AnnouncementText = a.announcement_text,
        CreatedAt = a.created_at,
        CreatedBy = a.CreatedByUser.FirstName + " " + a.CreatedByUser.LastName,
        createdByUserImage = a.CreatedByUser.ProfileImageUrl
    }).ToList();

    return Ok(result);
}


    [HttpGet]
    public async Task<IActionResult> GetAnnouncementsByUniversity(int universityId)
    {
        var announcements = await _context.Announcements
            .Where(a => a.university_id == universityId)
            .ToListAsync();

        return Ok(announcements);
    }

    [HttpPost]
    public async Task<IActionResult> AddAnnouncement([FromBody] Announcement announcement)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid data.");
        }

        var university = await _context.Universities.FindAsync(announcement.university_id);
        var user = await _context.Users.FindAsync(announcement.created_by);

        if (university == null)
        {
            return BadRequest("Invalid university ID.");
        }

        if (user == null)
        {
            return BadRequest("Invalid user ID.");
        }

        // تعيين الكائنات المتعلقة بالجامعة والمستخدم
        announcement.University = university;
        announcement.CreatedByUser = user;

        // تعيين التاريخ
        announcement.created_at = DateTime.UtcNow;

        // إضافة الإعلان إلى قاعدة البيانات
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        return Ok(announcement);
    }



    // عرض تفاصيل الإعلان عند الضغط على "قراءة المزيد"
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnnouncementById(int id)
    {
        var announcement = await _context.Announcements
            .Where(a => a.id == id)
            .FirstOrDefaultAsync();

        if (announcement == null)
        {
            return NotFound("Announcement not found");
        }

        return Ok(announcement);
    }
}
