namespace CouncilAgendaApi.Models;

public class RotationLog
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MemberId { get; set; }
    public string RotationType { get; set; } = string.Empty; // prayer, training, conducting
    public DateTime AssignedDate { get; set; }
    public Member Member { get; set; } = null!;
}