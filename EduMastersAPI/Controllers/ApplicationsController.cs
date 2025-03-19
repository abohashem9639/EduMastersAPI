using EduMastersAPI.Data;
using EduMastersAPI.Models;
using EduMastersAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApplicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Applications
    [HttpGet]
    public async Task<IActionResult> GetApplications([FromQuery] bool? isAdmin)
    {
        // تحقق من أن القيمة ليست فارغة
        var userId = 0;

        if (User.Identity.Name != null)
        {
            // إذا كانت القيمة موجودة، حاول تحويلها
            userId = int.Parse(User.Identity.Name);
        }

        IQueryable<Application> applications;

        if (isAdmin.HasValue && isAdmin.Value) // إذا كان المستخدم Admin
        {
            applications = _context.Applications
                                   .Include(a => a.Student)
                                   .Include(a => a.University)
                                   .Include(a => a.Branch);
        }
        else // إذا كان المستخدم عادي
        {
            applications = _context.Applications
                                   .Where(a => a.CreatedByUserId == userId)  // تصفية التطبيقات حسب المستخدم
                                   .Include(a => a.Student)
                                   .Include(a => a.University)
                                   .Include(a => a.Branch);
        }

        return Ok(await applications.ToListAsync());
    }





    // GET: api/Applications/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplicationById(int id)
    {
        var application = await _context.Applications
            .Include(a => a.Student)
            .Include(a => a.University)
            .Include(a => a.Branch)
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
        {
            return NotFound("Application not found");
        }

        return Ok(application);
    }

    // POST: api/Applications
    // POST: api/Applications
    [HttpPost]
    public async Task<IActionResult> AddApplication([FromBody] ApplicationDto applicationDto)
    {
        // تأكد من إرسال Status مع البيانات المرسلة
        if (string.IsNullOrEmpty(applicationDto.Status))
        {
            applicationDto.Status = "Ready to Apply"; // تحديد قيمة افتراضية إذا لم يتم إرسالها
        }

        var student = await _context.Students.FindAsync(applicationDto.StudentId);
        if (student == null)
        {
            return BadRequest("Student not found.");
        }

        var university = await _context.Universities.FindAsync(applicationDto.UniversityId);
        if (university == null)
        {
            return BadRequest("University not found.");
        }

        var branch = await _context.UniversityBranches.FindAsync(applicationDto.BranchId);
        if (branch == null)
        {
            return BadRequest("Branch not found.");
        }

        var existingApplication = await _context.Applications
            .Where(a => a.StudentId == applicationDto.StudentId &&
                        a.UniversityId == applicationDto.UniversityId &&
                        a.BranchId == applicationDto.BranchId) 
            .Where(a => a.Status != "المرحلة الثالثة")
            .FirstOrDefaultAsync();

        if (existingApplication != null)
        {
            existingApplication.Status = "المرحلة الأولى"; 
            await _context.SaveChangesAsync();
            return Ok(new { message = "Application updated successfully." });
        }

        var application = new Application
        {
            StudentId = applicationDto.StudentId,
            Degree = applicationDto.Degree,
            UniversityId = applicationDto.UniversityId,
            Language = applicationDto.Language,
            BranchId = applicationDto.BranchId,
            CreatedByUserId = applicationDto.CreatedByUserId,
            Status = applicationDto.Status 
        };

        try
        {
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Application added successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error adding application.", error = ex.Message });
        }
    }

    // PUT: api/Applications/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] ApplicationStatusDto dto)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
        {
            return NotFound("Application not found");
        }

        // قائمة الحالات بالتدرج الصحيح
        var statusOrder = new List<string>
    {
        "Ready to Apply",
        "Applied",
        "Missing Documents",
        "Missing Docs Uploaded",
        "Similar",
        "Rejected",
        "Conditional Letter Issued",
        "Deposit Paid",
        "Final Acceptance Letter Issued",
        "Registration Done"
    };

        int currentIndex = statusOrder.IndexOf(application.Status);
        int newIndex = statusOrder.IndexOf(dto.Status);

        //if (newIndex == -1)
        //{
        //    return BadRequest("Invalid status.");
        //}

        //if (newIndex <= currentIndex)
        //{
        //    return BadRequest("You cannot move to a previous or same status.");
        //}

        application.Status = dto.Status;
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Application status updated successfully." });
    }

    // Generate a random 6-digit PIN
    private string GeneratePin()
    {
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }





}

