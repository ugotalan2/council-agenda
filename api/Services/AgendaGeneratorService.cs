using CouncilAgendaApi.Data;
using CouncilAgendaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Services;

public class AgendaGeneratorService
{
    private readonly AppDbContext _db;

    public AgendaGeneratorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AgendaItem>> GenerateAgenda(Guid orgId, Guid meetingId)
    {
        var org = await _db.Organizations.FindAsync(orgId)
            ?? throw new KeyNotFoundException("Organization not found");

        var positions = await _db.OrgPositions
            .Where(p => p.OrganizationId == orgId && p.IsStanding && p.IsRotationEligible && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();

        var items = new List<AgendaItem>();
        int order = 0;

        // 1. Opening prayer
        var openingPrayer = await AssignRotation(orgId, meetingId, positions, "opening_prayer", order++);
        items.Add(openingPrayer);

        // 2. Conducting — fixed to bishop or rotated depending on org type
        if (!org.ConductingRotates)
        {
            items.Add(new AgendaItem
            {
                MeetingId = meetingId,
                ItemType = "conducting",
                DisplayOrder = order++,
                Notes = "Bishop conducting"
            });
        }
        else
        {
            var conducting = await AssignRotation(orgId, meetingId, positions, "conducting", order++);
            items.Add(conducting);
        }

        // 3. Carry forward open assignments due by this meeting
        var meeting = await _db.Meetings.FindAsync(meetingId)
            ?? throw new KeyNotFoundException("Meeting not found");

        var dueAssignments = await _db.Assignments
            .Where(a => a.Meeting.OrganizationId == orgId
                && a.Status == "open"
                && a.FollowUpDate <= meeting.MeetingDate)
            .Include(a => a.Owner)
            .ToListAsync();

        foreach (var assignment in dueAssignments)
        {
            items.Add(new AgendaItem
            {
                MeetingId = meetingId,
                ItemType = "follow_up",
                DisplayOrder = order++,
                Notes = $"Follow up: {assignment.Description} ({assignment.Owner.Name})"
            });
        }

        // 4. Ministry area focus
        var ministryArea = await GetNextMinistryArea(orgId);
        if (ministryArea != null)
        {
            items.Add(new AgendaItem
            {
                MeetingId = meetingId,
                ItemType = "ministry_focus",
                DisplayOrder = order++,
                Notes = $"Ministry focus: {ministryArea.Name}"
            });

            ministryArea.LastFocused = DateTime.UtcNow;

            // 5. Handbook section for this ministry area
            var handbookSection = await GetNextHandbookSection(orgId, ministryArea.Id);
            if (handbookSection != null)
            {
                items.Add(new AgendaItem
                {
                    MeetingId = meetingId,
                    ItemType = "handbook_training",
                    DisplayOrder = order++,
                    HandbookSectionId = handbookSection.Id,
                    Notes = $"{handbookSection.Chapter}.{handbookSection.Section} - {handbookSection.Title}"
                });

                handbookSection.LastUsed = DateTime.UtcNow;
            }
        }

        // 6. Fill remaining space from backlog
        var backlogItem = await GetNextBacklogItem(orgId, ministryArea?.Id);
        if (backlogItem != null)
        {
            items.Add(new AgendaItem
            {
                MeetingId = meetingId,
                ItemType = "backlog_topic",
                DisplayOrder = order++,
                TopicBacklogId = backlogItem.Id,
                Notes = backlogItem.Title
            });
        }

        // 7. Closing prayer
        var closingPrayer = await AssignRotation(orgId, meetingId, positions, "closing_prayer", order++);
        items.Add(closingPrayer);

        // Save all items
        _db.AgendaItems.AddRange(items);

        // Mark meeting as generated
        meeting.AgendaGenerated = true;
        await _db.SaveChangesAsync();

        return items;
    }

    private async Task<AgendaItem> AssignRotation(
        Guid orgId, Guid meetingId, List<OrgPosition> positions, string rotationType, int order)
    {
        // Get recent rotation logs for this org and type
        var recentLogs = await _db.RotationLogs
            .Where(r => r.OrganizationId == orgId && r.RotationType == rotationType)
            .OrderByDescending(r => r.AssignedDate)
            .Take(positions.Count)
            .ToListAsync();

        // For handbook training, also exclude anyone who already has it today cross-org
        List<Guid> excludedToday = new();
        if (rotationType == "handbook_training")
        {
            var today = DateTime.UtcNow.Date;
            excludedToday = await _db.RotationLogs
                .Where(r => r.RotationType == "handbook_training"
                    && r.AssignedDate.Date == today)
                .Select(r => r.PositionId)
                .ToListAsync();
        }

        // Find the last assigned position in this org
        var lastLog = recentLogs.FirstOrDefault();
        OrgPosition nextPosition;

        if (lastLog == null)
        {
            // Nobody assigned yet — pick first eligible
            nextPosition = positions
                .Where(p => !excludedToday.Contains(p.Id))
                .First();
        }
        else
        {
            // Advance round-robin from last assigned, skipping excluded
            var lastIndex = positions.FindIndex(p => p.Id == lastLog.PositionId);
            var count = positions.Count;
            OrgPosition? candidate = null;

            for (int i = 1; i <= count; i++)
            {
                var checkIndex = (lastIndex + i) % count;
                var checkPosition = positions[checkIndex];
                if (!excludedToday.Contains(checkPosition.Id))
                {
                    candidate = checkPosition;
                    break;
                }
            }

            // Fallback to first if all excluded (shouldn't happen in practice)
            nextPosition = candidate ?? positions.First();
        }

        // Resolve display name — use mapped member name if available
        var memberPosition = await _db.MemberPositions
            .Include(mp => mp.Member)
            .Where(mp => mp.PositionId == nextPosition.Id)
            .OrderByDescending(mp => mp.EffectiveDate)
            .FirstOrDefaultAsync();

        var displayName = memberPosition?.Member?.Name ?? nextPosition.Title;

        // Log the rotation
        _db.RotationLogs.Add(new RotationLog
        {
            OrganizationId = orgId,
            PositionId = nextPosition.Id,
            RotationType = rotationType,
            AssignedDate = DateTime.UtcNow
        });

        return new AgendaItem
        {
            MeetingId = meetingId,
            ItemType = rotationType,
            DisplayOrder = order,
            Notes = displayName
        };
    }

    private async Task<MinistryArea?> GetNextMinistryArea(Guid orgId)
    {
        // Get the ministry area that hasn't been focused on longest
        return await _db.MinistryAreas
            .Where(m => m.OrganizationId == orgId)
            .OrderBy(m => m.LastFocused == null ? DateTime.MinValue : m.LastFocused)
            .FirstOrDefaultAsync();
    }

    private async Task<HandbookSection?> GetNextHandbookSection(Guid orgId, Guid ministryAreaId)
    {
        // Get the handbook section for this ministry area not used longest
        return await _db.HandbookSections
            .Where(h => h.OrganizationId == orgId && h.MinistryAreaId == ministryAreaId)
            .OrderBy(h => h.LastUsed == null ? DateTime.MinValue : h.LastUsed)
            .FirstOrDefaultAsync();
    }

    private async Task<TopicBacklogItem?> GetNextBacklogItem(Guid orgId, Guid? ministryAreaId)
    {
        // Prefer backlog items matching the current ministry area focus
        var query = _db.TopicBacklogItems
            .Where(t => t.OrganizationId == orgId && !t.Used);

        if (ministryAreaId.HasValue)
        {
            var focused = await query
                .Where(t => t.MinistryAreaId == ministryAreaId)
                .OrderByDescending(t => t.Priority)
                .FirstOrDefaultAsync();

            if (focused != null) return focused;
        }

        // Fall back to highest priority untagged item
        return await query
            .OrderByDescending(t => t.Priority)
            .FirstOrDefaultAsync();
    }
}
