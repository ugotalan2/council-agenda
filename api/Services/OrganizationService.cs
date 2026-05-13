using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Models;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Services;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _db;

    public OrganizationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<OrganizationResponse>> GetUserOrganizations(string clerkUserId)
    {
        return await _db.UserOrganizations
            .Where(uo => uo.ClerkUserId == clerkUserId)
            .Include(uo => uo.Organization)
            .Select(uo => new OrganizationResponse(
                uo.Organization.Id,
                uo.Organization.Name,
                uo.Organization.OrgType,
                uo.Organization.ConductingRotates,
                uo.Role
            ))
            .ToListAsync();
    }

    public async Task<OrganizationResponse> CreateOrganization(string clerkUserId, CreateOrgRequest request)
    {
        var org = new Organization
        {
            Name = request.Name,
            OrgType = request.OrgType,
            ConductingRotates = request.ConductingRotates
        };

        _db.Organizations.Add(org);
        await _db.SaveChangesAsync();

        // Create default settings
        _db.OrganizationSettings.Add(new OrganizationSettings
        {
            OrganizationId = org.Id,
            MeetingDay = "Sunday",
            MeetingTime = new TimeOnly(11, 0),
            Frequency = "weekly",
            MeetingDurationMinutes = 60
        });

        // Add admin user
        _db.UserOrganizations.Add(new UserOrganization
        {
            ClerkUserId = clerkUserId,
            OrganizationId = org.Id,
            Role = "admin"
        });

        // Seed default responsibilities for this org type
        var defaults = DefaultResponsibilities.Defaults
            .Where(d => d.OrgTypeScope == request.OrgType)
            .ToList();

        foreach (var (title, lcrUrl, minWeeks, maxWeeks, orgTypeScope) in defaults)
        {
            _db.RecurringResponsibilities.Add(new RecurringResponsibility
            {
                OrganizationId = org.Id,
                Title = title,
                LcrUrl = lcrUrl,
                MinWeeks = minWeeks,
                MaxWeeks = maxWeeks,
                OrgTypeScope = orgTypeScope,
                Active = true
            });
        }

        await _db.SaveChangesAsync();

        return new OrganizationResponse(
            org.Id,
            org.Name,
            org.OrgType,
            org.ConductingRotates,
            "admin"
        );
    }

    public async Task<bool> DeleteOrganization(string clerkUserId, Guid orgId)
    {
        var userOrg = await _db.UserOrganizations
            .FirstOrDefaultAsync(uo => uo.OrganizationId == orgId
                && uo.ClerkUserId == clerkUserId
                && uo.Role == "admin");

        if (userOrg == null) return false;

        var org = await _db.Organizations.FindAsync(orgId);
        if (org == null) return false;

        _db.Organizations.Remove(org);
        await _db.SaveChangesAsync();
        return true;
    }
}