namespace CouncilAgendaApi.Models;

public class ResponsibilityCheckin
{
    public Guid Id { get; set; }
    public Guid ResponsibilityId { get; set; }
    public string CheckedByClerkUserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // clear, needs_followup, snoozed
    public string? Notes { get; set; }
    public DateTime CheckedAt { get; set; }
    public RecurringResponsibility Responsibility { get; set; } = null!;
}