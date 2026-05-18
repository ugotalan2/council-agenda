using Asp.Versioning;
using CouncilAgendaApi.Data;
using CouncilAgendaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v1/organizations/{orgId}/meetings/{meetingId}/attendees")]
[Authorize]
public class AgendaAttendeesController : BaseController
{
    public AgendaAttendeesController(AppDbContext db) : base(db) { }

    // GET attendees for a meeting — returns all standing positions + any guests added
    [HttpGet]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetAttendees(Guid orgId, Guid meetingId)
    {
        // Verify meeting belongs to org
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.OrganizationId == orgId);
        if (meeting == null) return NotFound();

        // Get all standing positions for this org
        var standingPositions = await _db.OrgPositions
            .Where(p => p.OrganizationId == orgId && p.IsStanding && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.IsStanding,
                p.IsRotationEligible,
                MappedMemberName = p.MemberPositions
                    .OrderByDescending(mp => mp.EffectiveDate)
                    .Select(mp => mp.Member.Name)
                    .FirstOrDefault()
            })
            .ToListAsync();

        // Get existing attendance records for this meeting
        var attendeeRecords = await _db.AgendaAttendees
            .Where(a => a.MeetingId == meetingId)
            .ToListAsync();

        // Merge: for each standing position, find or default attendance
        var result = standingPositions.Select(p =>
        {
            var record = attendeeRecords.FirstOrDefault(a => a.PositionId == p.Id);
            return new
            {
                AttendeeId = record?.Id,
                PositionId = p.Id,
                p.Title,
                p.IsStanding,
                p.IsRotationEligible,
                DisplayName = p.MappedMemberName ?? p.Title,
                Attending = record?.Attending ?? true, // default to attending
                GuestLabel = (string?)null
            };
        }).ToList<object>();

        // Add any guest attendees for this meeting
        var guests = attendeeRecords
            .Where(a => a.PositionId == null && a.GuestLabel != null)
            .Select(a => new
            {
                AttendeeId = (Guid?)a.Id,
                PositionId = (Guid?)null,
                Title = a.GuestLabel!,
                IsStanding = false,
                IsRotationEligible = false,
                DisplayName = a.GuestLabel!,
                Attending = a.Attending,
                GuestLabel = a.GuestLabel
            }).ToList<object>();

        result.AddRange(guests);
        return Ok(result);
    }

    // PUT upsert attendance for a position
    [HttpPut("{positionId}")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> SetAttendance(
        Guid orgId, Guid meetingId, Guid positionId,
        [FromBody] SetAttendanceRequest request)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.OrganizationId == orgId);
        if (meeting == null) return NotFound();

        var existing = await _db.AgendaAttendees
            .FirstOrDefaultAsync(a => a.MeetingId == meetingId && a.PositionId == positionId);

        if (existing != null)
        {
            existing.Attending = request.Attending;
        }
        else
        {
            _db.AgendaAttendees.Add(new AgendaAttendee
            {
                MeetingId = meetingId,
                PositionId = positionId,
                Attending = request.Attending
            });
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    // POST add a guest to this meeting
    [HttpPost("guests")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> AddGuest(Guid orgId, Guid meetingId,
        [FromBody] AddGuestRequest request)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.OrganizationId == orgId);
        if (meeting == null) return NotFound();

        // Check if this guest label already exists for this meeting
        var existing = await _db.AgendaAttendees
            .FirstOrDefaultAsync(a => a.MeetingId == meetingId
                && a.GuestLabel != null
                && a.GuestLabel.ToLower() == request.GuestLabel.ToLower());

        if (existing != null) return Ok(new {
			existing.Id,
			existing.MeetingId,
			existing.GuestLabel,
			existing.Attending,
			PositionId = (Guid?)null
		});

        var attendee = new AgendaAttendee
        {
            MeetingId = meetingId,
            GuestLabel = request.GuestLabel,
            Attending = true
        };

        _db.AgendaAttendees.Add(attendee);
        await _db.SaveChangesAsync();
        return Ok(new {
			attendee.Id,
			attendee.MeetingId,
			attendee.GuestLabel,
			attendee.Attending,
			PositionId = (Guid?)null
		});
    }

    // DELETE remove a guest from this meeting
    [HttpDelete("guests/{attendeeId}")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> RemoveGuest(Guid orgId, Guid meetingId, Guid attendeeId)
    {
        var attendee = await _db.AgendaAttendees
            .FirstOrDefaultAsync(a => a.Id == attendeeId
                && a.MeetingId == meetingId
                && a.GuestLabel != null); // Only guests can be removed

        if (attendee == null) return NotFound();

        _db.AgendaAttendees.Remove(attendee);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record SetAttendanceRequest(bool Attending);
public record AddGuestRequest(string GuestLabel);