using Asp.Versioning;
using CouncilAgendaApi.Constants;
using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route(ApiRoutes.Members)]
[Authorize]
public class MembersController : BaseController
{
    private readonly IMemberService _memberService;

    public MembersController(AppDbContext db, IMemberService memberService) : base(db)
    {
        _memberService = memberService;
    }

    [HttpGet]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetMembers(Guid orgId)
    {
        var members = await _memberService.GetMembers(orgId);
        return Ok(members);
    }

    [HttpPost]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> AddMember(Guid orgId, [FromBody] MemberRequest request)
    {
        var member = await _memberService.AddMember(orgId, request);
        return CreatedAtAction(nameof(GetMembers), new { orgId }, member);
    }

    [HttpPut("{memberId}")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> UpdateMember(Guid orgId, Guid memberId, [FromBody] MemberRequest request)
    {
        var member = await _memberService.UpdateMember(orgId, memberId, request);
        if (member == null) return NotFound();
        return Ok(member);
    }

    [HttpDelete("{memberId}")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> DeactivateMember(Guid orgId, Guid memberId)
    {
        var deactivated = await _memberService.DeactivateMember(orgId, memberId);
        if (!deactivated) return NotFound();
        return NoContent();
    }
}
