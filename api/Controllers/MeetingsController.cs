using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Services;
using CouncilAgendaApi.Services.Interfaces;
using CouncilAgendaApi.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route(ApiRoutes.Meetings)]
[Authorize]
public class MeetingsController : BaseController
{
	private readonly IMeetingService _meetingService;
    private readonly AgendaGeneratorService _agendaGenerator;

    public MeetingsController(
        AppDbContext db,
        IMeetingService meetingService,
        AgendaGeneratorService agendaGenerator) : base(db)
    {
        _meetingService = meetingService;
        _agendaGenerator = agendaGenerator;
    }

    [HttpGet]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetMeetings(Guid orgId)
    {
        var meetings = await _meetingService.GetMeetings(orgId);
        return Ok(meetings);
    }

    [HttpGet("{meetingId}")]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetMeeting(Guid orgId, Guid meetingId)
    {
        var meeting = await _meetingService.GetMeeting(orgId, meetingId);
        if (meeting == null) return NotFound();
        return Ok(meeting);
    }

    [HttpPost]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> CreateMeeting(Guid orgId, [FromBody] CreateMeetingRequest request)
    {
        var meeting = await _meetingService.CreateMeeting(orgId, request);
        return CreatedAtAction(nameof(GetMeeting), new { orgId, meetingId = meeting.Id }, meeting);
    }

	[HttpPost("{meetingId}/generate")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> GenerateAgenda(Guid orgId, Guid meetingId)
    {
        var meeting = await _meetingService.GetMeeting(orgId, meetingId);
        if (meeting == null) return NotFound();
        if (meeting.AgendaGenerated)
            return BadRequest("Agenda has already been generated for this meeting.");

        var items = await _agendaGenerator.GenerateAgenda(orgId, meetingId);
        return Ok(items);
    }

    [HttpDelete("{meetingId}")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> DeleteMeeting(Guid orgId, Guid meetingId)
    {
        var deleted = await _meetingService.DeleteMeeting(orgId, meetingId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}