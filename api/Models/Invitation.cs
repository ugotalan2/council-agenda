namespace CouncilAgendaApi.Models;

public class Invitation
{
    public Guid Id { get; set; }
    public string InvitedByClerkUserId { get; set; } = string.Empty;
    public string InvitedEmail { get; set; } = string.Empty;
    public string? ClerkInvitationId { get; set; }
    public string Status { get; set; } = "pending"; // pending, accepted, expired
    public DateTime CreatedAt { get; set; }
    public ICollection<InvitationOrganization> Organizations { get; set; } = new List<InvitationOrganization>();
}
