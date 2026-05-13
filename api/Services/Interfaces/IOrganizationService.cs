using CouncilAgendaApi.DTOs;

namespace CouncilAgendaApi.Services.Interfaces;

public interface IOrganizationService
{
    Task<List<OrganizationResponse>> GetUserOrganizations(string clerkUserId);
    Task<OrganizationResponse> CreateOrganization(string clerkUserId, CreateOrgRequest request);
    Task<bool> DeleteOrganization(string clerkUserId, Guid orgId);
}