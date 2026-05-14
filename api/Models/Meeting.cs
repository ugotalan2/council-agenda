namespace CouncilAgendaApi.Models;

public class Meeting
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime MeetingDate { get; set; }
    public string Status { get; set; } = "draft"; // draft, published, past
    public bool AgendaGenerated { get; set; }
    public bool AgendaPublished { get; set; }
    public string? GoogleDocUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public Organization Organization { get; set; } = null!;
    public ICollection<AgendaItem> AgendaItems { get; set; } = new List<AgendaItem>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<AgendaAttendee> Attendees { get; set; } = new List<AgendaAttendee>();
    public ICollection<AgendaNote> Notes { get; set; } = new List<AgendaNote>();
}
