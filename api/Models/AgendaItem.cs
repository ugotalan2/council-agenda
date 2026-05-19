namespace CouncilAgendaApi.Models;

public class AgendaItem
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid? PositionId { get; set; }
    public OrgPosition? Position { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public string? Notes { get; set; }
    public Guid? HandbookSectionId { get; set; }
    public Guid? TopicBacklogId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    public HandbookSection? HandbookSection { get; set; }
    public TopicBacklogItem? TopicBacklogItem { get; set; }
}
