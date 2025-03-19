using EduMastersAPI.Data;
using EduMastersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ApplicationFilesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ApplicationFilesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // POST: api/ApplicationFiles/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] int applicationId, [FromForm] string fileName, [FromForm] int CreatedByUserId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // Get the file extension
        string fileExtension = Path.GetExtension(file.FileName);

        // Create the directory if it doesn't exist
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "application_files", applicationId.ToString());
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Combine the filename with extension
        string fullFileName = $"{fileName}{fileExtension}"; // Add extension to file name

        string filePath = Path.Combine(uploadsFolder, fullFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        var applicationFile = new ApplicationFile
        {
            ApplicationId = applicationId,
            FileName = fullFileName, // Store file with extension
            FilePath = $"uploads/application_files/{applicationId}/{fullFileName}", // Save the full path with applicationId
            FileType = file.ContentType,
            CreatedByUserId = CreatedByUserId // Store userId from the request
        };

        _context.ApplicationFiles.Add(applicationFile);
        await _context.SaveChangesAsync();

        return Ok(new { message = "File uploaded successfully", filePath = applicationFile.FilePath });
    }

    // GET: api/ApplicationFiles/{applicationId}
    [HttpGet("{applicationId}")]
    public async Task<IActionResult> GetFilesForApplication(int applicationId)
    {
        var files = await _context.ApplicationFiles
            .Where(f => f.ApplicationId == applicationId)
            .Include(f => f.CreatedByUser) // تأكد من تحميل معلومات المستخدم
            .ToListAsync();

        var filesWithUser = files.Select(f => new
        {
            f.Id,
            f.FileName,
            f.FilePath,
            f.FileType,
            f.CreatedAt,
            UploadedBy = f.CreatedByUser != null ? f.CreatedByUser.FirstName + " " + f.CreatedByUser.LastName : "Unknown",
            ProfileImageUrl = f.CreatedByUser != null ? f.CreatedByUser.ProfileImageUrl : null // إضافة مسار الصورة
        }).ToList();

        return Ok(filesWithUser);
    }




}
