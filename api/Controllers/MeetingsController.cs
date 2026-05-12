using CouncilAgendaApi.Data;
using CouncilAgendaApi.Models;
using CouncilAgendaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[Route("api/organizations/{orgId}/meetings")]
[Authorize]
public class MeetingsController : BaseController
{
	private readonly AgendaGeneratorService _agendaGenerator;
    public MeetingsController(AppDbContext db, AgendaGeneratorService agendaGenerator) : base(db)
    {
        _agendaGenerator = agendaGenerator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMeetings(Guid orgId)
    {
        if (!await HasAccess(orgId)) return Forbid();

        var meetings = await _db.Meetings
            .Where(m => m.OrganizationId == orgId)
            .OrderByDescending(m => m.MeetingDate)
            .Select(m => new {
                m.Id,
                m.MeetingDate,
                m.AgendaGenerated,
                m.AgendaPublished,
                m.GoogleDocUrl,
                m.CreatedAt
            })
            .ToListAsync();

        return Ok(meetings);
    }

    [HttpGet("{meetingId}")]
    public async Task<IActionResult> GetMeeting(Guid orgId, Guid meetingId)
    {
        if (!await HasAccess(orgId)) return Forbid();

        var meeting = await _db.Meetings
            .Where(m => m.Id == meetingId && m.OrganizationId == orgId)
            .Include(m => m.AgendaItems)
                .ThenInclude(a => a.HandbookSection)
            .Include(m => m.AgendaItems)
                .ThenInclude(a => a.TopicBacklogItem)
            .Include(m => m.Assignments)
                .ThenInclude(a => a.Owner)
            .FirstOrDefaultAsync();

        if (meeting == null) return NotFound();

        return Ok(meeting);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMeeting(Guid orgId, [FromBody] CreateMeetingRequest request)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();

        var meeting = new Meeting
        {
            OrganizationId = orgId,
            MeetingDate = request.MeetingDate,
            AgendaGenerated = false,
            AgendaPublished = false
        };

        _db.Meetings.Add(meeting);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMeeting), new { orgId, meetingId = meeting.Id }, meeting);
    }

	[HttpPost("{meetingId}/generate")]
public async Task<IActionResult> GenerateAgenda(Guid orgId, Guid meetingId)
{
    if (!await HasAccess(orgId, "editor")) return Forbid();

    var meeting = await _db.Meetings
        .FirstOrDefaultAsync(m => m.Id == meetingId && m.OrganizationId == orgId);

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

        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.OrganizationId == orgId);

        if (meeting == null) return NotFound();

        _db.Meetings.Remove(meeting);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateMeetingRequest(DateTime MeetingDate);