namespace CouncilAgendaApi.Models;

public class OrgPosition
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OrgTypeScope { get; set; } = string.Empty;
    public bool IsStanding { get; set; } = true;
    public bool IsGuestDefault { get; set; } = false;
    public bool IsRotationEligible { get; set; } = true;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<MemberPosition> MemberPositions { get; set; } = new List<MemberPosition>();
    public ICollection<AgendaAttendee> AgendaAttendees { get; set; } = new List<AgendaAttendee>();
    public ICollection<RotationLog> RotationLogs { get; set; } = new List<RotationLog>();
}