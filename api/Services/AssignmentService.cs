using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Models;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Services;

public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _db;

    public AssignmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AssignmentResponse>> GetAssignments(Guid orgId, string? status = null)
    {
        var query = _db.Assignments
            .Where(a => a.Meeting.OrganizationId == orgId)
            .Include(a => a.Owner)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        return await query
            .OrderBy(a => a.FollowUpDate)
            .Select(a => new AssignmentResponse(
                a.Id,
                a.Description,
                a.FollowUpDate,
                a.Status,
                a.Owner.Name
            ))
            .ToListAsync();
    }

    public async Task<AssignmentResponse> CreateAssignment(Guid orgId, CreateAssignmentRequest request)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId && m.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Meeting not found");

        var owner = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == request.OwnerId && m.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Member not found");

        var assignment = new Assignment
        {
            MeetingId = request.MeetingId,
            OwnerId = request.OwnerId,
            Description = request.Description,
            FollowUpDate = request.FollowUpDate,
            Status = "open"
        };

        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();

        return new AssignmentResponse(
            assignment.Id,
            assignment.Description,
            assignment.FollowUpDate,
            assignment.Status,
            owner.Name
        );
    }

    public async Task<AssignmentResponse?> UpdateStatus(
        Guid orgId,
        Guid assignmentId,
        UpdateAssignmentStatusRequest request)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Owner)
            .Include(a => a.Meeting)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.Meeting.OrganizationId == orgId);

        if (assignment == null) return null;

        if (!new[] { "open", "resolved", "extended" }.Contains(request.Status))
            throw new ArgumentException("Invalid status. Must be open, resolved, or extended.");

        assignment.Status = request.Status;

        if (request.Status == "extended" && request.NewFollowUpDate.HasValue)
            assignment.FollowUpDate = request.NewFollowUpDate.Value;

        await _db.SaveChangesAsync();

        return new AssignmentResponse(
            assignment.Id,
            assignment.Description,
            assignment.FollowUpDate,
            assignment.Status,
            assignment.Owner.Name
        );
    }

    public async Task<bool> DeleteAssignment(Guid orgId, Guid assignmentId)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Meeting)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.Meeting.OrganizationId == orgId);

        if (assignment == null) return false;

        _db.Assignments.Remove(assignment);
        await _db.SaveChangesAsync();
        return true;
    }
}
