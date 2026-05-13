using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Services.Interfaces;
using CouncilAgendaApi.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route(ApiRoutes.People)]
[Authorize]
public class PeopleController : BaseController
{
    private readonly IInvitationService _invitationService;

    public PeopleController(AppDbContext db, IInvitationService invitationService) : base(db)
    {
        _invitationService = invitationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyPeople()
    {
        var people = await _invitationService.GetMyPeople(GetClerkUserId());
        return Ok(people);
    }

    [HttpPut("{targetClerkUserId}/access")]
    public async Task<IActionResult> UpdateAccess(
        string targetClerkUserId,
        [FromBody] List<OrgRoleAssignment> assignments)
    {
        await _invitationService.UpdatePersonAccess(GetClerkUserId(), targetClerkUserId, assignments);
        return Ok();
    }

    [HttpDelete("{targetClerkUserId}/organizations/{orgId}")]
    public async Task<IActionResult> RemoveFromOrg(string targetClerkUserId, Guid orgId)
    {
        await _invitationService.RemovePersonFromOrg(GetClerkUserId(), targetClerkUserId, orgId);
        return NoContent();
    }
}