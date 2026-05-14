namespace CouncilAgendaApi.DTOs;

public record MemberResponse(
    Guid Id,
    string Name,
    string Calling,
    bool Active,
    string ClerkUserId
);

public record MemberRequest(
    string Name,
    string Calling,
    string? ClerkUserId
);
