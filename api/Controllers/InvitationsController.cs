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
[Route(ApiRoutes.Invitations)]
[Authorize]
public class InvitationsController : BaseController
{
    private readonly IInvitationService _invitationService;

    public InvitationsController(AppDbContext db, IInvitationService invitationService) : base(db)
    {
        _invitationService = invitationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPendingInvitations()
    {
        var invitations = await _invitationService.GetPendingInvitations(GetClerkUserId());
        return Ok(invitations);
    }

    [HttpPost]
    public async Task<IActionResult> InvitePerson([FromBody] InvitePersonRequest request)
    {
        var invitation = await _invitationService.InvitePerson(GetClerkUserId(), request);
        return CreatedAtAction(nameof(GetPendingInvitations), invitation);
    }

    [HttpDelete("{invitationId}")]
    public async Task<IActionResult> CancelInvitation(Guid invitationId)
    {
        var cancelled = await _invitationService.CancelInvitation(GetClerkUserId(), invitationId);
        if (!cancelled) return NotFound();
        return NoContent();
    }

    [HttpPost("{invitationId}/sync")]
    public async Task<IActionResult> SyncInvitation(Guid invitationId)
    {
        var synced = await _invitationService.SyncInvitation(GetClerkUserId(), invitationId);
        if (!synced) return BadRequest("Could not find a Clerk account for this email.");
        return Ok();
    }
}
