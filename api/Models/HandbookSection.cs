namespace CouncilAgendaApi.Models;

public class HandbookSection
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? MinistryAreaId { get; set; }
    public string Chapter { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? LastUsed { get; set; }
    public MinistryArea? MinistryArea { get; set; }
}