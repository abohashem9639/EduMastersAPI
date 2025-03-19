using EduMastersAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class TeamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeamsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddTeam([FromForm] string name, IFormFile? logo)
    {
        try
        {
            var team = new Team { Name = name };

            if (logo != null)
            {
                string logoPath = SaveFile(logo, team.Name);
                team.Logo = logoPath;
            }

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return Ok(team);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error creating team: " + ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTeams()
    {
        var teams = await _context.Teams.Include(t => t.TeamMembers)
                                        .ThenInclude(tm => tm.User) 
                                        .ToListAsync();
        return Ok(teams);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeamById(int id)
    {
        var team = await _context.Teams
    .Include(t => t.TeamMembers)
    .ThenInclude(tm => tm.User) 
    .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
            return NotFound("Team not found");

        return Ok(team);
    }

    private string SaveFile(IFormFile file, string teamName)
    {
        string folderPath = Path.Combine("wwwroot", "uploads", teamName);
        Directory.CreateDirectory(folderPath);

        string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        string filePath = Path.Combine(folderPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return filePath.Replace("wwwroot", "");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTeam(int id, [FromForm] string name, IFormFile? logo)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound("Team not found");
        }

        team.Name = name;
        if (logo != null)
        {
            string logoPath = SaveFile(logo, team.Name);
            team.Logo = logoPath;
        }

        await _context.SaveChangesAsync();
        return Ok(team);
    }


    [HttpDelete("{teamId}/remove-member/{userId}")]
    public async Task<IActionResult> RemoveMemberFromTeam(int teamId, int userId)
    {
        var teamMember = await _context.TeamMembers
                                        .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
        if (teamMember == null)
        {
            return NotFound("Team member not found");
        }

        _context.TeamMembers.Remove(teamMember);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableAgents(int teamId)
    {
        var availableAgents = await _context.Users
            .Where(u => u.UserType == "Agent" &&
                        !_context.TeamMembers.Any(tm => tm.UserId == u.Id && tm.TeamId == teamId))
            .ToListAsync();

        return Ok(availableAgents);
    }






    [HttpPost("{teamId}/add-members")]
    public async Task<IActionResult> AddMembersToTeam(int teamId, [FromBody] AddMembersRequest request)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null) return NotFound("Team not found");

        foreach (var userId in request.UserIds)
        {
            if (!_context.TeamMembers.Any(tm => tm.TeamId == teamId && tm.UserId == userId))
            {
                var teamMember = new TeamMember { TeamId = teamId, UserId = userId };
                _context.TeamMembers.Add(teamMember);
            }
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    public class AddMembersRequest
    {
        public List<int> UserIds { get; set; }
    }



}
