namespace CouncilAgendaApi.Models;

public class Member
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string ClerkUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Calling { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public Organization Organization { get; set; } = null!;
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<RotationLog> RotationLogs { get; set; } = new List<RotationLog>();
}