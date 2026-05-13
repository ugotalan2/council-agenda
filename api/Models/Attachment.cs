namespace CouncilAgendaApi.Models;

public class Attachment
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? AgendaItemId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string AttachmentType { get; set; } = "link"; // link, google_drive, lcr_report, handbook
    public DateTime CreatedAt { get; set; }
    public Organization Organization { get; set; } = null!;
    public AgendaItem? AgendaItem { get; set; }
}