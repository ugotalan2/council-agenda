using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[Route("api/organizations/{orgId}/members")]
[Authorize]
public class MembersController : BaseController
{
    private readonly IMemberService _memberService;

    public MembersController(AppDbContext db, IMemberService memberService) : base(db)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(Guid orgId)
    {
        if (!await HasAccess(orgId)) return Forbid();
        var members = await _memberService.GetMembers(orgId);
        return Ok(members);
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(Guid orgId, [FromBody] MemberRequest request)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();
        var member = await _memberService.AddMember(orgId, request);
        return CreatedAtAction(nameof(GetMembers), new { orgId }, member);
    }

    [HttpPut("{memberId}")]
    public async Task<IActionResult> UpdateMember(Guid orgId, Guid memberId, [FromBody] MemberRequest request)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();
        var member = await _memberService.UpdateMember(orgId, memberId, request);
        if (member == null) return NotFound();
        return Ok(member);
    }

    [HttpDelete("{memberId}")]
    public async Task<IActionResult> DeactivateMember(Guid orgId, Guid memberId)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();
        var deactivated = await _memberService.DeactivateMember(orgId, memberId);
        if (!deactivated) return NotFound();
        return NoContent();
    }
}