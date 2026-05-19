using Asp.Versioning;
using CouncilAgendaApi.Constants;
using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Services;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route(ApiRoutes.Meetings)]
[Authorize]
public class MeetingsController : BaseController
{
    private readonly IMeetingService _meetingService;
    private readonly AgendaGeneratorService _agendaGenerator;
    private readonly IAgendaExportService _agendaExportService;

    public MeetingsController(
        AppDbContext db,
        IMeetingService meetingService,
        AgendaGeneratorService agendaGenerator,
        IAgendaExportService agendaExportService) : base(db)
    {
        _meetingService = meetingService;
        _agendaGenerator = agendaGenerator;
        _agendaExportService = agendaExportService;
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

    [HttpPut("{meetingId}")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> UpdateMeeting(Guid orgId, Guid meetingId, [FromBody] UpdateMeetingRequest request)
    {
        var meeting = await _meetingService.UpdateMeeting(orgId, meetingId, request);
        if (meeting == null) return NotFound();
        return Ok(meeting);
    }

    [HttpDelete("{meetingId}")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> DeleteMeeting(Guid orgId, Guid meetingId)
    {
        var deleted = await _meetingService.DeleteMeeting(orgId, meetingId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("{meetingId}/export")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> ExportToGoogleDoc(Guid orgId, Guid meetingId)
    {
        try
        {
            var url = await _agendaExportService.ExportToGoogleDocAsync(orgId, meetingId);
            return Ok(new { url });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{meetingId}/agenda-items")]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetAgendaItems(Guid orgId, Guid meetingId)
    {
        var meeting = await _meetingService.GetMeeting(orgId, meetingId);
        if (meeting == null) return NotFound();

        var items = await _db.AgendaItems
            .Where(a => a.MeetingId == meetingId)
            .Include(a => a.Position)
                .ThenInclude(p => p!.MemberPositions)
                    .ThenInclude(mp => mp.Member)
            .Include(a => a.HandbookSection)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new
            {
                a.Id,
                a.ItemType,
                a.DisplayOrder,
                a.Notes,
                a.PositionId,
                DisplayName = a.Position != null
                    ? a.Position.MemberPositions
                        .OrderByDescending(mp => mp.EffectiveDate)
                        .Select(mp => mp.Member.Name)
                        .FirstOrDefault() ?? a.Position.Title
                    : a.Notes,
                HandbookSection = a.HandbookSection == null ? null : new
                {
                    a.HandbookSection.Id,
                    a.HandbookSection.Title,
                    a.HandbookSection.Chapter,
                    a.HandbookSection.Section
                }
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{meetingId}/rotation-history")]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetRotationHistory(Guid orgId, Guid meetingId)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.OrganizationId == orgId);
        if (meeting == null) return NotFound();

        var meetingDate = meeting.MeetingDate.Date;

        var positions = await _db.OrgPositions
            .Where(p => p.OrganizationId == orgId && p.IsStanding && p.IsActive)
            .ToListAsync();

        var positionIds = positions.Select(p => p.Id).ToList();
        var rotationTypes = new[] { "opening_prayer", "closing_prayer", "handbook_training" };
        var history = new List<object>();

        foreach (var type in rotationTypes)
        {
            var logs = await _db.RotationLogs
                .Where(r => r.OrganizationId == orgId
                    && r.RotationType == type
                    && positionIds.Contains(r.PositionId)
                    && r.AssignedDate.Date != meetingDate) // exclude current meeting
                .OrderByDescending(r => r.AssignedDate)
                .Take(positions.Count)
                .ToListAsync();

            foreach (var position in positions)
            {
                var lastLog = logs.FirstOrDefault(l => l.PositionId == position.Id);
                history.Add(new
                {
                    PositionId = position.Id,
                    RotationType = type,
                    LastAssigned = lastLog?.AssignedDate
                });
            }
        }

        return Ok(history);
    }
}
