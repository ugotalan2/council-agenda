namespace CouncilAgendaApi.Models;

public class TopicBacklogItem
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? MinistryAreaId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Source { get; set; }
    public bool Used { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public MinistryArea? MinistryArea { get; set; }
}
