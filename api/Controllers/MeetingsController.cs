using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Services;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[Route("api/organizations/{orgId}/meetings")]
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
    public async Task<IActionResult> GetMeetings(Guid orgId)
    {
        if (!await HasAccess(orgId)) return Forbid();
        var meetings = await _meetingService.GetMeetings(orgId);
        return Ok(meetings);
    }

    [HttpGet("{meetingId}")]
    public async Task<IActionResult> GetMeeting(Guid orgId, Guid meetingId)
    {
        if (!await HasAccess(orgId)) return Forbid();
        var meeting = await _meetingService.GetMeeting(orgId, meetingId);
        if (meeting == null) return NotFound();
        return Ok(meeting);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMeeting(Guid orgId, [FromBody] CreateMeetingRequest request)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();
        var meeting = await _meetingService.CreateMeeting(orgId, request);
        return CreatedAtAction(nameof(GetMeeting), new { orgId, meetingId = meeting.Id }, meeting);
    }

	[HttpPost("{meetingId}/generate")]
    public async Task<IActionResult> GenerateAgenda(Guid orgId, Guid meetingId)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();
        var meeting = await _meetingService.GetMeeting(orgId, meetingId);
        if (meeting == null) return NotFound();
        if (meeting.AgendaGenerated)
            return BadRequest("Agenda has already been generated for this meeting.");

        try
        {
            var items = await _agendaGenerator.GenerateAgenda(orgId, meetingId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{meetingId}")]
    public async Task<IActionResult> DeleteMeeting(Guid orgId, Guid meetingId)
    {
        if (!await HasAccess(orgId, "admin")) return Forbid();
        var deleted = await _meetingService.DeleteMeeting(orgId, meetingId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}