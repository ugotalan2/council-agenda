using CouncilAgendaApi.Data;
using CouncilAgendaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationsController : BaseController
{
    public OrganizationsController(AppDbContext db) : base(db) {}

    [HttpGet]
    public async Task<IActionResult> GetMyOrganizations()
    {
        var clerkUserId = GetClerkUserId();
        var orgs = await _db.UserOrganizations
            .Where(uo => uo.ClerkUserId == clerkUserId)
            .Include(uo => uo.Organization)
            .Select(uo => new {
                uo.Organization.Id,
                uo.Organization.Name,
                uo.Organization.OrganizationType,
                uo.Organization.ConductingRotates,
                uo.Role
            })
            .ToListAsync();

        return Ok(orgs);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrgRequest request)
    {
        var clerkUserId = GetClerkUserId();

        var org = new Organization
        {
            Name = request.Name,
            OrganizationType = request.OrganizationType,
            ConductingRotates = request.ConductingRotates
        };

        _db.Organizations.Add(org);

        var userOrg = new UserOrganization
        {
            ClerkUserId = clerkUserId,
            OrganizationId = org.Id,
            Role = "admin"
        };

        _db.UserOrganizations.Add(userOrg);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyOrganizations), new { id = org.Id }, org);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrganization(Guid id)
    {
        var clerkUserId = GetClerkUserId();

        var userOrg = await _db.UserOrganizations
            .FirstOrDefaultAsync(uo => uo.OrganizationId == id && uo.ClerkUserId == clerkUserId && uo.Role == "admin");

        if (userOrg == null) return Forbid();

        var org = await _db.Organizations.FindAsync(id);
        if (org == null) return NotFound();

        _db.Organizations.Remove(org);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateOrgRequest(string Name, string OrganizationType, bool ConductingRotates);