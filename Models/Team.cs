public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Logo { get; set; }
    public List<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
}
