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
}