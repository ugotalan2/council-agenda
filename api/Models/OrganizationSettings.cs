namespace CouncilAgendaApi.Models;

public class OrganizationSettings
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string MeetingDay { get; set; } = string.Empty; // Monday, Sunday, etc.
    public TimeOnly MeetingTime { get; set; }
    public string Frequency { get; set; } = "weekly"; // weekly, biweekly, monthly
    public int? WeekOfMonth { get; set; } // 1-4 for monthly
    public int MeetingDurationMinutes { get; set; } = 60;
    public Organization Organization { get; set; } = null!;
}