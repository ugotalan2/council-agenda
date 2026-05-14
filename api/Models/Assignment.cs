namespace CouncilAgendaApi.Models;

public class Assignment
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid? AgendaItemId { get; set; }
    public Guid OwnerId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime FollowUpDate { get; set; }
    public string Status { get; set; } = "open";
    public DateTime CreatedAt { get; set; }
    public Meeting Meeting { get; set; } = null!;
    public AgendaItem? AgendaItem { get; set; }
    public Member Owner { get; set; } = null!;
}
