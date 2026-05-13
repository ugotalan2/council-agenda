namespace CouncilAgendaApi.Models;

public class RecurringResponsibility
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? LcrUrl { get; set; }
    public int MinWeeks { get; set; } = 2;
    public int MaxWeeks { get; set; } = 4;
    public DateTime? LastCheckedDate { get; set; }
    public string? LastCheckedByClerkUserId { get; set; }
    public string OrgTypeScope { get; set; } = string.Empty; // bishopric, ward_council, etc.
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Organization Organization { get; set; } = null!;
    public ICollection<ResponsibilityCheckin> Checkins { get; set; } = new List<ResponsibilityCheckin>();
}