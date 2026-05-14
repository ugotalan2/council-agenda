using CouncilAgendaApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Authorization;

public class OrgAccessHandler : AuthorizationHandler<OrgAccessRequirement>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrgAccessHandler(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrgAccessRequirement requirement)
    {
        var clerkUserId = context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(clerkUserId))
        {
            context.Fail();
            return;
        }

        var routeValues = _httpContextAccessor.HttpContext?.Request.RouteValues;
        if (routeValues == null || !routeValues.TryGetValue("orgId", out var orgIdObj))
        {
            context.Fail();
            return;
        }

        if (!Guid.TryParse(orgIdObj?.ToString(), out var orgId))
        {
            context.Fail();
            return;
        }

        var userOrg = await _db.UserOrganizations
            .FirstOrDefaultAsync(uo => uo.OrganizationId == orgId && uo.ClerkUserId == clerkUserId);

        if (userOrg == null)
        {
            context.Fail();
            return;
        }

        var hasRole = requirement.MinRole switch
        {
            "admin" => userOrg.Role == "admin",
            "editor" => userOrg.Role is "admin" or "editor",
            _ => true
        };

        if (hasRole)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
