namespace CouncilAgendaApi.DTOs;

public record MeetingResponse(
    Guid Id,
    DateTime MeetingDate,
    bool AgendaGenerated,
    bool AgendaPublished,
    string? GoogleDocUrl,
    DateTime CreatedAt
);

public record MeetingDetailResponse(
    Guid Id,
    DateTime MeetingDate,
    bool AgendaGenerated,
    bool AgendaPublished,
    string? GoogleDocUrl,
    DateTime CreatedAt,
    List<AgendaItemResponse> AgendaItems,
    List<AssignmentResponse> Assignments
);

public record AgendaItemResponse(
    Guid Id,
    string ItemType,
    int DisplayOrder,
    string? Notes,
    Guid? HandbookSectionId,
    Guid? TopicBacklogId
);

public record AssignmentResponse(
    Guid Id,
    string Description,
    DateTime FollowUpDate,
    string Status,
    string OwnerName
);

public record CreateMeetingRequest(
    DateTime MeetingDate
);

public record UpdateMeetingRequest(DateTime MeetingDate);
