using EduMastersAPI.Data;
using EduMastersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class UniversitiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UniversitiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Universities
    [HttpGet]
    public async Task<IActionResult> GetUniversities()
    {
        var universities = await _context.Universities.ToListAsync();
        return Ok(universities);
    }

    // POST: api/Universities
    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> AddUniversity([FromForm] string name, [FromForm] string country, [FromForm] string city, [FromForm] string universityType, IFormFile? logo)
    {
        Console.WriteLine($"Received: Name={name}, Country={country}, City={city}, UniversityType={universityType}");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var university = new University
        {
            Name = name,
            Country = country,
            City = city,
            UniversityType = universityType,
            CreatedAt = DateTime.UtcNow
        };

        if (logo != null)
        {
            var fileName = $"{Guid.NewGuid()}_{logo.FileName}";
            var filePath = Path.Combine("wwwroot/logos", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await logo.CopyToAsync(stream);
            }
            university.LogoUrl = $"/logos/{fileName}";
        }

        _context.Universities.Add(university);
        await _context.SaveChangesAsync();

        return Ok(university);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUniversity(int id, [FromForm] string name, [FromForm] string country, [FromForm] string city, [FromForm] string universityType, IFormFile? logo)
    {
        var existingUniversity = await _context.Universities.FindAsync(id);
        if (existingUniversity == null)
            return NotFound("University not found");

        existingUniversity.Name = name;
        existingUniversity.Country = country;
        existingUniversity.City = city;  // ✅ تحديث المدينة
        existingUniversity.UniversityType = universityType;  // ✅ تحديث نوع الجامعة

        if (logo != null)
        {
            var fileName = $"{Guid.NewGuid()}_{logo.FileName}";
            var filePath = Path.Combine("wwwroot/logos", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await logo.CopyToAsync(stream);
            }
            existingUniversity.LogoUrl = $"/logos/{fileName}";
        }

        await _context.SaveChangesAsync();
        return Ok(existingUniversity);
    }





    [HttpGet("{id}")]
    public async Task<IActionResult> GetUniversity(int id)
    {
        var university = await _context.Universities.FindAsync(id);
        if (university == null)
        {
            return NotFound();
        }
        return Ok(university);
    }

}
