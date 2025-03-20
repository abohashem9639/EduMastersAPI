using EduMastersAPI.Data;
using EduMastersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System;

[Route("[controller]")]
[ApiController]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddStudent([FromForm] Student student)
    {
        try
        {
            // تحقق من أن رقم الجواز غير مكرر
            if (await _context.Students.AnyAsync(s => s.PassportNumber == student.PassportNumber))
            {
                return BadRequest("Passport number is already in use.");
            }

            // تحقق من أن البريد الإلكتروني غير مكرر
            if (await _context.Students.AnyAsync(s => s.Email == student.Email))
            {
                return BadRequest("Email is already in use.");
            }

            int createdByUserId = Convert.ToInt32(Request.Form["CreatedByUserId"]);
            student.CreatedByUserId = createdByUserId;

            if (string.IsNullOrEmpty(student.ProfileImageUrl))
            {
                student.ProfileImageUrl = "/uploads/default-profile-image.png"; // تعيين مسار صورة افتراضية
            }

            if (student.SalesResponsibleId == null || !await _context.Users.AnyAsync(u => u.Id == student.SalesResponsibleId))
            {
                return BadRequest("Sales Responsible is invalid or not found.");
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok(student);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error details: " + ex.Message);
            return StatusCode(500, "Error saving student: " + ex.Message);
        }
    }





    [HttpPost("{studentId}/upload-files")]
    public async Task<IActionResult> UploadFiles(int studentId, [FromForm] IFormFile[] files, [FromForm] string[] fileTypes)
    {
        if (files == null || files.Length == 0)
        {
            return BadRequest("No files uploaded.");
        }

        var student = await _context.Students.FindAsync(studentId);
        if (student == null)
        {
            return NotFound("Student not found.");
        }

        List<StudentFile> uploadedFiles = new List<StudentFile>();

        string studentFolder = Path.Combine("wwwroot", "uploads", studentId.ToString());
        Directory.CreateDirectory(studentFolder); // تأكد من وجود المجلد

        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var fileType = fileTypes.Length > i ? fileTypes[i] : "Unknown";

            string fileExtension = Path.GetExtension(file.FileName);
            string sanitizedFirstName = student.FirstName.Replace(" ", "_");
            string sanitizedLastName = student.LastName.Replace(" ", "_");

            // إعادة تسمية الملف حسب التنسيق المطلوب
            string newFileName = $"{sanitizedFirstName}_{sanitizedLastName}_{fileType}_{studentId}{fileExtension}";
            string filePath = Path.Combine(studentFolder, newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var studentFile = new StudentFile
            {
                StudentId = studentId,
                FileType = fileType, // ✅ تخزين نوع الملف في قاعدة البيانات
                FilePath = filePath.Replace("wwwroot", ""), // حفظ المسار بدون "wwwroot"
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentFiles.Add(studentFile);
            uploadedFiles.Add(studentFile);
        }

        await _context.SaveChangesAsync();
        return Ok(uploadedFiles);
    }






    // إحضار جميع الطلاب
    [HttpGet]
    public async Task<IActionResult> GetStudents()
    {
        try
        {
            var students = await _context.Students.ToListAsync();
            return Ok(students);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error fetching students: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudentById(int id)
    {
        var student = await _context.Students
            .Include(s => s.Applications)
            .ThenInclude(a => a.University)
            .ThenInclude(a => a.Branches)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound("Student not found");

        // استرجاع صورة الطالب إذا كانت موجودة
        var studentFile = await _context.StudentFiles
            .Where(sf => sf.StudentId == id && sf.FileType == "profileImage")
            .FirstOrDefaultAsync();

        if (studentFile != null && studentFile.FilePath != null)
        {
            student.ProfileImageUrl = studentFile.FilePath;
        }

        // استرجاع بيانات المسؤول عن الطالب
        var salesResponsible = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == student.SalesResponsibleId);

        if (salesResponsible != null)
        {
            student.SalesResponsibleName = $"{salesResponsible.FirstName} {salesResponsible.LastName}";
        }

        return Ok(student);
    }




    [HttpGet("get-sales-responsibles")]
    public async Task<IActionResult> GetSalesResponsibles([FromQuery] string salesResponsibleType)
    {
        var responsibles = salesResponsibleType switch
        {
            "Sales Person" => await _context.Users.Where(u => u.UserType != "Agent" && u.UserType != "SubAgent").ToListAsync(),
            "Agent Person" => await _context.Users.Where(u => u.UserType == "Agent").ToListAsync(),
            "subAgent Person" => await _context.Users.Where(u => u.UserType == "SubAgent").ToListAsync(),
            _ => new List<User>()
        };
        return Ok(responsibles);
    }




    [HttpGet("{studentId}/files")]
    public async Task<IActionResult> GetStudentFiles(int studentId)
    {
        var studentFiles = await _context.StudentFiles
                                          .Where(f => f.StudentId == studentId)
                                          .ToListAsync();

        if (studentFiles == null || studentFiles.Count == 0)
        {
            return NotFound("No files found for this student.");
        }

        return Ok(studentFiles);
    }


}
