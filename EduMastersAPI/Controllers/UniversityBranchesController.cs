using EduMastersAPI.Data;
using EduMastersAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class UniversityBranchesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UniversityBranchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/UniversityBranches
    [HttpGet]
    public async Task<IActionResult> GetBranches([FromQuery] int? universityId)
    {
        var query = _context.UniversityBranches.AsQueryable();

        if (universityId.HasValue)
        {
            query = query.Where(b => b.UniversityId == universityId.Value);
        }

        var branches = await query.ToListAsync();
        return Ok(branches);
    }


    [HttpPost]
    public async Task<IActionResult> AddBranch([FromBody] UniversityBranchDto branchDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var universityExists = await _context.Universities.AnyAsync(u => u.Id == branchDto.UniversityId);
        if (!universityExists)
        {
            return BadRequest(new { message = "The specified University ID does not exist." });
        }

        if (branchDto.Levels == null || !branchDto.Levels.Any() || branchDto.Languages == null || !branchDto.Languages.Any())
        {
            return BadRequest(new { message = "Levels and Languages cannot be empty." });
        }

        var branchesToAdd = new List<UniversityBranch>();

        foreach (var level in branchDto.Levels)
        {
            foreach (var language in branchDto.Languages)
            {
                branchesToAdd.Add(new UniversityBranch
                {
                    UniversityId = branchDto.UniversityId,
                    BranchName = branchDto.BranchName,
                    DurationYears = branchDto.DurationYears,
                    AnnualFee = branchDto.AnnualFee,
                    Levels = level.Trim(),
                    Languages = language.Trim(),
                    Price = branchDto.Price,
                    DiscountPrice = branchDto.DiscountPrice,
                    CashPrice = branchDto.CashPrice,
                    Currency = branchDto.Currency,
                    OurCommission = branchDto.OurCommission,
                    Status = branchDto.Status,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.UniversityBranches.AddRangeAsync(branchesToAdd);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Branch added successfully.", addedBranches = branchesToAdd });
    }








    // PUT: api/UniversityBranches/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBranch(int id, [FromBody] UniversityBranch branch)
    {
        if (id != branch.Id)
            return BadRequest();

        _context.Entry(branch).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.UniversityBranches.Any(b => b.Id == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/UniversityBranches/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBranch(int id)
    {
        var branch = await _context.UniversityBranches.FindAsync(id);
        if (branch == null)
            return NotFound();

        _context.UniversityBranches.Remove(branch);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
