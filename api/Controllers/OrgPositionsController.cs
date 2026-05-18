using Asp.Versioning;
using CouncilAgendaApi.Constants;
using CouncilAgendaApi.Data;
using CouncilAgendaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v1/organizations/{orgId}/positions")]
[Authorize]
public class OrgPositionsController : BaseController
{
    public OrgPositionsController(AppDbContext db) : base(db) { }

    // GET all positions for an org (standing + guest defaults)
    [HttpGet]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetPositions(Guid orgId)
    {
        var positions = await _db.OrgPositions
            .Where(p => p.OrganizationId == orgId && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.IsStanding,
                p.IsGuestDefault,
                p.IsRotationEligible,
                p.DisplayOrder,
                MappedMember = p.MemberPositions
                    .OrderByDescending(mp => mp.EffectiveDate)
                    .Select(mp => new { mp.Member.Id, mp.Member.Name })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(positions);
    }

    // POST add a custom position (guest invite)
    [HttpPost]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> AddPosition(Guid orgId, [FromBody] AddPositionRequest request)
    {
        // Check for existing position with same title to avoid duplicates
        var existing = await _db.OrgPositions
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId
                && p.Title.ToLower() == request.Title.ToLower()
                && p.IsActive);

        if (existing != null)
            return Ok(existing); // Return existing instead of creating duplicate

        var maxOrder = await _db.OrgPositions
            .Where(p => p.OrganizationId == orgId)
            .MaxAsync(p => (int?)p.DisplayOrder) ?? 0;

        var position = new OrgPosition
        {
            OrganizationId = orgId,
            Title = request.Title,
            IsStanding = false,
            IsGuestDefault = false,
            IsRotationEligible = request.IsRotationEligible,
            DisplayOrder = maxOrder + 1,
            OrgTypeScope = request.OrgType ?? ""
        };

        _db.OrgPositions.Add(position);
        await _db.SaveChangesAsync();
        return Ok(position);
    }

    // POST map a member to a position
    [HttpPost("{positionId}/map-member")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> MapMember(Guid orgId, Guid positionId, [FromBody] MapMemberRequest request)
    {
        var position = await _db.OrgPositions
            .FirstOrDefaultAsync(p => p.Id == positionId && p.OrganizationId == orgId);

        if (position == null) return NotFound();

        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId && m.OrganizationId == orgId);

        if (member == null) return NotFound("Member not found");

        var mapping = new MemberPosition
        {
            PositionId = positionId,
            MemberId = request.MemberId,
            EffectiveDate = DateTime.UtcNow
        };

        _db.MemberPositions.Add(mapping);
        await _db.SaveChangesAsync();
        return Ok(mapping);
    }

    // DELETE soft-delete a custom position
    [HttpDelete("{positionId}")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> DeletePosition(Guid orgId, Guid positionId)
    {
        var position = await _db.OrgPositions
            .FirstOrDefaultAsync(p => p.Id == positionId
                && p.OrganizationId == orgId
                && !p.IsStanding); // Can't delete standing positions

        if (position == null) return NotFound();

        position.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record AddPositionRequest(string Title, bool IsRotationEligible, string? OrgType);
public record MapMemberRequest(Guid MemberId);