using CouncilAgendaApi.DTOs;

namespace CouncilAgendaApi.Services.Interfaces;

public interface IInvitationService
{
    Task<InvitationResponse> InvitePerson(string invitedByClerkUserId, InvitePersonRequest request);
    Task<List<InvitationResponse>> GetPendingInvitations(string clerkUserId);
    Task<List<PeopleResponse>> GetMyPeople(string clerkUserId);
    Task UpdatePersonAccess(string clerkUserId, string targetClerkUserId, List<OrgRoleAssignment> assignments);
    Task RemovePersonFromOrg(string clerkUserId, string targetClerkUserId, Guid orgId);
    Task<bool> CancelInvitation(string clerkUserId, Guid invitationId);
    Task<bool> SyncInvitation(string clerkUserId, Guid invitationId);
}
