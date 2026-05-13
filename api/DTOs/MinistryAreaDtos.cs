namespace CouncilAgendaApi.DTOs;

public record MinistryAreaResponse(
    Guid Id,
    string Name,
    DateTime? LastFocused
);

public record MinistryAreaRequest(
    string Name
);