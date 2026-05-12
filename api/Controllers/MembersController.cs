using CouncilAgendaApi.Data;
using CouncilAgendaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[Route("api/organizations/{orgId}/members")]
[Authorize]
public class MembersController : BaseController
{
    public MembersController(AppDbContext db) : base(db) { }

    [HttpGet]
    public async Task<IActionResult> GetMembers(Guid orgId)
    {
        if (!await HasAccess(orgId)) return Forbid();

        var members = await _db.Members
            .Where(m => m.OrganizationId == orgId && m.Active)
            .OrderBy(m => m.Name)
            .Select(m => new {
                m.Id,
                m.Name,
                m.Calling,
                m.Active,
                m.ClerkUserId
            })
            .ToListAsync();

        return Ok(members);
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(Guid orgId, [FromBody] MemberRequest request)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();

        var member = new Member
        {
            OrganizationId = orgId,
            Name = request.Name,
            Calling = request.Calling,
            ClerkUserId = request.ClerkUserId ?? string.Empty,
            Active = true
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMembers), new { orgId }, member);
    }

    [HttpPut("{memberId}")]
    public async Task<IActionResult> UpdateMember(Guid orgId, Guid memberId, [FromBody] MemberRequest request)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();

        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && m.OrganizationId == orgId);

        if (member == null) return NotFound();

        member.Name = request.Name;
        member.Calling = request.Calling;
        if (request.ClerkUserId != null)
            member.ClerkUserId = request.ClerkUserId;

        await _db.SaveChangesAsync();
        return Ok(member);
    }

    [HttpDelete("{memberId}")]
    public async Task<IActionResult> DeactivateMember(Guid orgId, Guid memberId)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();

        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && m.OrganizationId == orgId);

        if (member == null) return NotFound();

        member.Active = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record MemberRequest(string Name, string Calling, string? ClerkUserId);