namespace CouncilAgendaApi.DTOs;

public record OrganizationResponse(
    Guid Id,
    string Name,
    string OrganizationType,
    bool ConductingRotates,
    string Role
);

public record CreateOrgRequest(
    string Name,
    string OrganizationType,
    bool ConductingRotates
);