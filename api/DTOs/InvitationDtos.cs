namespace CouncilAgendaApi.DTOs;

public record InvitePersonRequest(
    string Email,
    List<OrgRoleAssignment> Organizations
);

public record OrgRoleAssignment(
    Guid OrganizationId,
    string Role
);

public record InvitationResponse(
    Guid Id,
    string Email,
    string Status,
    DateTime CreatedAt,
    List<OrgRoleAssignment> Organizations
);

public record PeopleResponse(
    string ClerkUserId,
    string Email,
    string? Name,
    List<OrgAccessSummary> Organizations
);

public record OrgAccessSummary(
    Guid OrgId,
    string OrgName,
    string Role
);
