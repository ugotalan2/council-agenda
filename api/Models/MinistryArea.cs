namespace CouncilAgendaApi.Models;

public class MinistryArea
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? LastFocused { get; set; }
    public ICollection<HandbookSection> HandbookSections { get; set; } = new List<HandbookSection>();
    public ICollection<TopicBacklogItem> TopicBacklogItems { get; set; } = new List<TopicBacklogItem>();
}
