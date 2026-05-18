namespace CouncilAgendaApi.Models;

public class MemberPosition
{
    public Guid Id { get; set; }
    public Guid PositionId { get; set; }
    public Guid MemberId { get; set; }
    public DateTime EffectiveDate { get; set; }

    // Navigation
    public OrgPosition Position { get; set; } = null!;
    public Member Member { get; set; } = null!;
}