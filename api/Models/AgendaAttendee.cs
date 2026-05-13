namespace CouncilAgendaApi.Models;

public class AgendaAttendee
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid MemberId { get; set; }
    public bool Attending { get; set; } = true;
    public Meeting Meeting { get; set; } = null!;
    public Member Member { get; set; } = null!;
}