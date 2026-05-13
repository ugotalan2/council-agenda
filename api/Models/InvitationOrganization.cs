namespace CouncilAgendaApi.Models;

public class InvitationOrganization
{
    public Guid Id { get; set; }
    public Guid InvitationId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Role { get; set; } = "viewer";
    public Invitation Invitation { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}