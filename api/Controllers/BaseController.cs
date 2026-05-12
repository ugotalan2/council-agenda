using CouncilAgendaApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Controllers;

public class BaseController : ControllerBase
{
    protected readonly AppDbContext _db;

    public BaseController(AppDbContext db)
    {
        _db = db;
    }

    protected string GetClerkUserId() =>
        User.FindFirst("sub")?.Value ?? string.Empty;

    protected async Task<bool> HasAccess(Guid orgId, string minRole = "viewer")
    {
        var clerkUserId = GetClerkUserId();
        var userOrg = await _db.UserOrganizations
            .FirstOrDefaultAsync(uo => uo.OrganizationId == orgId && uo.ClerkUserId == clerkUserId);

        if (userOrg == null) return false;

        return minRole switch
        {
            "admin" => userOrg.Role == "admin",
            "editor" => userOrg.Role is "admin" or "editor",
            _ => true
        };
    }

    protected async Task<string?> GetUserRole(Guid orgId)
    {
        var clerkUserId = GetClerkUserId();
        var userOrg = await _db.UserOrganizations
            .FirstOrDefaultAsync(uo => uo.OrganizationId == orgId && uo.ClerkUserId == clerkUserId);
        return userOrg?.Role;
    }
}