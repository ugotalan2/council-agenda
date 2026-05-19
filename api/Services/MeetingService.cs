using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Models;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Services;

public class MeetingService : IMeetingService
{
    private readonly AppDbContext _db;

    public MeetingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<MeetingResponse>> GetMeetings(Guid orgId)
    {
        return await _db.Meetings
            .Where(m => m.OrganizationId == orgId)
            .OrderByDescending(m => m.MeetingDate)
            .Select(m => new MeetingResponse(
                m.Id,
                m.MeetingDate,
                m.AgendaGenerated,
                m.AgendaPublished,
                m.GoogleDocUrl,
                m.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<MeetingDetailResponse?> GetMeeting(Guid orgId, Guid meetingId)
    {
        var meeting = await _db.Meetings
            .Where(m => m.Id == meetingId && m.OrganizationId == orgId)
            .Include(m => m.AgendaItems)
                .ThenInclude(a => a.HandbookSection)
            .Include(m => m.AgendaItems)
                .ThenInclude(a => a.TopicBacklogItem)
            .Include(m => m.Assignments)
                .ThenInclude(a => a.Owner)
            .FirstOrDefaultAsync();

        if (meeting == null) return null;

        return new MeetingDetailResponse(
            meeting.Id,
            meeting.MeetingDate,
            meeting.AgendaGenerated,
            meeting.AgendaPublished,
            meeting.GoogleDocUrl,
            meeting.CreatedAt,
            meeting.AgendaItems.OrderBy(a => a.DisplayOrder).Select(a => new AgendaItemResponse(
                a.Id,
                a.ItemType,
                a.DisplayOrder,
                a.Notes,
                a.HandbookSectionId,
                a.TopicBacklogId
            )).ToList(),
            meeting.Assignments.Select(a => new AssignmentResponse(
                a.Id,
                a.Description,
                a.FollowUpDate,
                a.Status,
                a.Owner.Name
            )).ToList()
        );
    }

    public async Task<MeetingResponse> CreateMeeting(Guid orgId, CreateMeetingRequest request)
    {
        var meeting = new Meeting
        {
            OrganizationId = orgId,
            MeetingDate = request.MeetingDate,
            AgendaGenerated = false,
            AgendaPublished = false
        };

        _db.Meetings.Add(meeting);
        await _db.SaveChangesAsync();

        return new MeetingResponse(
            meeting.Id,
            meeting.MeetingDate,
            meeting.AgendaGenerated,
            meeting.AgendaPublished,
            meeting.GoogleDocUrl,
            meeting.CreatedAt
        );
    }

    public async Task<MeetingResponse?> UpdateMeeting(Guid orgId, Guid meetingId, UpdateMeetingRequest request)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.OrganizationId == orgId);
        if (meeting == null) return null;

        var oldDate = meeting.MeetingDate.Date;
        var newDate = request.MeetingDate.Date;

        meeting.MeetingDate = request.MeetingDate;

        // If date changed, update any rotation logs tied to this meeting's old date + org
        if (oldDate != newDate)
        {
            var assignedPositionIds = await _db.AgendaItems
                .Where(a => a.MeetingId == meetingId && a.PositionId != null)
                .Select(a => a.PositionId!.Value)
                .ToListAsync();

            if (assignedPositionIds.Any())
            {
                var logsToUpdate = await _db.RotationLogs
                    .Where(r => assignedPositionIds.Contains(r.PositionId)
                        && r.AssignedDate.Date == oldDate
                        && r.OrganizationId == orgId)
                    .ToListAsync();

                foreach (var log in logsToUpdate)
                    log.AssignedDate = request.MeetingDate;
            }
        }

        await _db.SaveChangesAsync();
        return new MeetingResponse(
            meeting.Id,
            meeting.MeetingDate,
            meeting.AgendaGenerated,
            meeting.AgendaPublished,
            meeting.GoogleDocUrl,
            meeting.CreatedAt
        );
    }

    public async Task<bool> DeleteMeeting(Guid orgId, Guid meetingId)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.OrganizationId == orgId);
        if (meeting == null) return false;

        // Get position IDs assigned in this meeting's agenda items
        var assignedPositionIds = await _db.AgendaItems
            .Where(a => a.MeetingId == meetingId && a.PositionId != null)
            .Select(a => a.PositionId!.Value)
            .ToListAsync();

        // Delete rotation logs created for this meeting's date + those positions
        if (assignedPositionIds.Any())
        {
            var meetingDate = meeting.MeetingDate.Date;
            var logsToDelete = await _db.RotationLogs
                .Where(r => assignedPositionIds.Contains(r.PositionId)
                    && r.AssignedDate.Date == meetingDate
                    && r.OrganizationId == orgId)
                .ToListAsync();

            _db.RotationLogs.RemoveRange(logsToDelete);
        }

        _db.Meetings.Remove(meeting);
        await _db.SaveChangesAsync();
        return true;
    }
}
