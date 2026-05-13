using System.Text;
using System.Text.Json;
using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Models;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Services;

public class InvitationService : IInvitationService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public InvitationService(AppDbContext db, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<InvitationResponse> InvitePerson(string invitedByClerkUserId, InvitePersonRequest request)
    {
        // Verify the inviter has admin access to all specified orgs
        foreach (var orgAssignment in request.Organizations)
        {
            var hasAccess = await _db.UserOrganizations
                .AnyAsync(uo => uo.OrganizationId == orgAssignment.OrganizationId
                    && uo.ClerkUserId == invitedByClerkUserId
                    && uo.Role == "admin");

            if (!hasAccess)
                throw new UnauthorizedAccessException($"You don't have admin access to one or more organizations.");
        }

        // Check if person already has a Clerk account and is already in these orgs
        var existingInvitation = await _db.Invitations
            .Include(i => i.Organizations)
            .FirstOrDefaultAsync(i => i.InvitedEmail == request.Email && i.Status == "pending");

        if (existingInvitation != null)
            throw new ArgumentException("A pending invitation already exists for this email.");

        string? clerkInvitationId = null;

		try
		{
			var payload = JsonSerializer.Serialize(new
			{
				email_address = request.Email,
				redirect_url = _config["App:FrontendUrl"] + "/accept-invite"
			});

			var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.clerk.com/v1/invitations")
			{
				Content = new StringContent(payload, Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
				"Bearer", _config["Clerk:SecretKey"]);

			var client = _httpClientFactory.CreateClient();
			var response = await client.SendAsync(httpRequest);
			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				var doc = JsonDocument.Parse(json);
				clerkInvitationId = doc.RootElement.GetProperty("id").GetString();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Clerk invitation failed: {ex.Message}");
		}

        var invitation = new Invitation
        {
            InvitedByClerkUserId = invitedByClerkUserId,
            InvitedEmail = request.Email,
            ClerkInvitationId = clerkInvitationId,
            Status = "pending"
        };

        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync();

        foreach (var orgAssignment in request.Organizations)
        {
            _db.InvitationOrganizations.Add(new InvitationOrganization
            {
                InvitationId = invitation.Id,
                OrganizationId = orgAssignment.OrganizationId,
                Role = orgAssignment.Role
            });
        }

        await _db.SaveChangesAsync();

        return new InvitationResponse(
            invitation.Id,
            invitation.InvitedEmail,
            invitation.Status,
            invitation.CreatedAt,
            request.Organizations
        );
    }

    public async Task<List<InvitationResponse>> GetPendingInvitations(string clerkUserId)
    {
        return await _db.Invitations
            .Include(i => i.Organizations)
            .Where(i => i.InvitedByClerkUserId == clerkUserId && i.Status == "pending")
            .Select(i => new InvitationResponse(
                i.Id,
                i.InvitedEmail,
                i.Status,
                i.CreatedAt,
                i.Organizations.Select(o => new OrgRoleAssignment(o.OrganizationId, o.Role)).ToList()
            ))
            .ToListAsync();
    }

    public async Task<List<PeopleResponse>> GetMyPeople(string clerkUserId)
    {
        // Get all orgs where the current user is admin
        var myAdminOrgIds = await _db.UserOrganizations
            .Where(uo => uo.ClerkUserId == clerkUserId && uo.Role == "admin")
            .Select(uo => uo.OrganizationId)
            .ToListAsync();

        // Get all users in those orgs (excluding self)
        var peopleInMyOrgs = await _db.UserOrganizations
            .Include(uo => uo.Organization)
            .Where(uo => myAdminOrgIds.Contains(uo.OrganizationId)
                && uo.ClerkUserId != clerkUserId)
            .GroupBy(uo => uo.ClerkUserId)
            .Select(g => new PeopleResponse(
                g.Key,
                "",
                null,
                g.Select(uo => new OrgAccessSummary(
                    uo.OrganizationId,
                    uo.Organization.Name,
                    uo.Role
                )).ToList()
            ))
            .ToListAsync();

        return peopleInMyOrgs;
    }

    public async Task UpdatePersonAccess(string clerkUserId, string targetClerkUserId, List<OrgRoleAssignment> assignments)
    {
        foreach (var assignment in assignments)
        {
            var hasAdminAccess = await _db.UserOrganizations
                .AnyAsync(uo => uo.OrganizationId == assignment.OrganizationId
                    && uo.ClerkUserId == clerkUserId
                    && uo.Role == "admin");

            if (!hasAdminAccess)
                throw new UnauthorizedAccessException("You don't have admin access to one or more organizations.");

            var existing = await _db.UserOrganizations
                .FirstOrDefaultAsync(uo => uo.OrganizationId == assignment.OrganizationId
                    && uo.ClerkUserId == targetClerkUserId);

            if (existing != null)
            {
                existing.Role = assignment.Role;
            }
            else
            {
                _db.UserOrganizations.Add(new UserOrganization
                {
                    ClerkUserId = targetClerkUserId,
                    OrganizationId = assignment.OrganizationId,
                    Role = assignment.Role
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task RemovePersonFromOrg(string clerkUserId, string targetClerkUserId, Guid orgId)
    {
        var hasAdminAccess = await _db.UserOrganizations
            .AnyAsync(uo => uo.OrganizationId == orgId
                && uo.ClerkUserId == clerkUserId
                && uo.Role == "admin");

        if (!hasAdminAccess)
            throw new UnauthorizedAccessException("You don't have admin access to this organization.");

        var userOrg = await _db.UserOrganizations
            .FirstOrDefaultAsync(uo => uo.OrganizationId == orgId
                && uo.ClerkUserId == targetClerkUserId);

        if (userOrg != null)
        {
            _db.UserOrganizations.Remove(userOrg);
            await _db.SaveChangesAsync();
        }
    }
}