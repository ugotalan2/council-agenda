namespace CouncilAgendaApi.DTOs;

public record OrganizationResponse(
    Guid Id,
    string Name,
    string OrgType,
    bool ConductingRotates,
    string Role
);

public record CreateOrgRequest(
    string Name,
    string OrgType,
    bool ConductingRotates
);
