namespace CouncilAgendaApi.DTOs;

public record CreateAssignmentRequest(
    Guid MeetingId,
    Guid OwnerId,
    string Description,
    DateTime FollowUpDate
);

public record UpdateAssignmentStatusRequest(
    string Status,
    DateTime? NewFollowUpDate
);