namespace CouncilAgendaApi.Models;

public class UserOrganization
{
    public Guid Id { get; set; }
    public string ClerkUserId { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public string Role { get; set; } = string.Empty; // admin, editor, viewer
    public DateTime JoinedAt { get; set; }
    public Organization Organization { get; set; } = null!;
}
