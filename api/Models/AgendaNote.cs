namespace CouncilAgendaApi.Models;

public class AgendaNote
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid? AgendaItemId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string CreatedByClerkUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Meeting Meeting { get; set; } = null!;
    public AgendaItem? AgendaItem { get; set; }
}
