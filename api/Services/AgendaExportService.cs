using CouncilAgendaApi.Data;
using Google.Apis.Docs.v1.Data;
using Microsoft.EntityFrameworkCore;
using DocsRange = Google.Apis.Docs.v1.Data.Range;

namespace CouncilAgendaApi.Services
{
    public interface IAgendaExportService
    {
        Task<string> ExportToGoogleDocAsync(Guid orgId, Guid meetingId);
    }

    public class AgendaExportService : IAgendaExportService
    {
        private readonly AppDbContext _db;
        private readonly IGoogleDocService _googleDocService;

        public AgendaExportService(AppDbContext db, IGoogleDocService googleDocService)
        {
            _db = db;
            _googleDocService = googleDocService;
        }

        public async Task<string> ExportToGoogleDocAsync(Guid orgId, Guid meetingId)
		{
			var org = await _db.Organizations
				.FirstOrDefaultAsync(o => o.Id == orgId)
				?? throw new KeyNotFoundException("Organization not found");

			var meeting = await _db.Meetings
				.FirstOrDefaultAsync(m => m.Id == meetingId && m.OrganizationId == orgId)
				?? throw new KeyNotFoundException("Meeting not found");

			var assignments = await _db.Assignments
				.Include(a => a.Owner)
				.Where(a => a.MeetingId == meetingId)
				.ToListAsync();

			var priorAssignments = await _db.Assignments
				.Include(a => a.Owner)
				.Include(a => a.Meeting)
				.Where(a => a.Meeting.OrganizationId == orgId
					&& a.Meeting.MeetingDate < meeting.MeetingDate
					&& a.Status == "open")
				.OrderBy(a => a.Meeting.MeetingDate)
				.ToListAsync();

            var attendees = await _db.AgendaAttendees
                .Include(a => a.Position)
                    .ThenInclude(p => p!.MemberPositions)
                        .ThenInclude(mp => mp.Member)
                .Where(a => a.MeetingId == meetingId && a.Attending)
                .ToListAsync();
            
            var agendaItems = await _db.AgendaItems
                .Include(a => a.Position)
                    .ThenInclude(p => p!.MemberPositions)
                        .ThenInclude(mp => mp.Member)
                .Where(a => a.MeetingId == meetingId)
                .ToListAsync();

            var refreshToken = org.GoogleRefreshToken
                ?? throw new InvalidOperationException("Google account not connected for this organization. Please connect via Settings.");

            var folderId = org.GoogleDriveFolderId
                ?? throw new InvalidOperationException("No Google Drive folder configured for this organization.");

            var centralTime = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago"); // TO DO make this a setting for the org
            var meetingDateLocal = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(meeting.MeetingDate, DateTimeKind.Utc), 
                centralTime);
			var title = $"{org.Name} — {meetingDateLocal:MMMM d, yyyy}";
            
            var requests = BuildDocRequests(org, meeting, assignments, priorAssignments, attendees, agendaItems, meetingDateLocal);
            
            // Pass refresh token to doc service
            var docUrl = await _googleDocService.CreateAgendaDocAsync(title, requests, folderId, refreshToken);
			meeting.GoogleDocUrl = docUrl;
			await _db.SaveChangesAsync();

			return docUrl;
		}

        private List<Request> BuildDocRequests(
            Models.Organization org,
            Models.Meeting meeting,
            List<Models.Assignment> assignments,
            List<Models.Assignment> priorAssignments,
            List<Models.AgendaAttendee> attendees,
            List<Models.AgendaItem> agendaItems,
            DateTime meetingDateLocal)
        {
            var builder = new DocBuilder();

            // grab the assignments
            var openingPrayer = GetAssignedDisplayName(agendaItems, "opening_prayer");
            var handbookTrainer = GetAssignedDisplayName(agendaItems, "handbook_training");
            var closingPrayer = GetAssignedDisplayName(agendaItems, "closing_prayer");

            // Title
            builder.AddText($"{org.Name.ToUpper()} — {meetingDateLocal:MMMM d, yyyy}\n", "TITLE");
            builder.AddText($"{meetingDateLocal:dddd, h:mm tt}\n", "SUBTITLE");
            builder.AddText("Conducting: Bishop\n", "SUBTITLE");

            // Special guests — only show if any guests were added
            var guests = attendees.Where(a => a.GuestLabel != null).ToList();
            if (guests.Any())
            {
                builder.AddHeading("SPECIAL GUESTS\n");
                foreach (var guest in guests)
                {
                    builder.AddBullet($"{guest.GuestLabel}\n");
                }
                builder.AddText("\n", "NORMAL");
            }

            // Opening
            builder.AddHeading("OPENING\n");
            builder.AddText($"Prayer: {openingPrayer}\n\n", "NORMAL");

            // Follow-up from prior meetings
            if (priorAssignments.Any())
            {
                builder.AddHeading("FOLLOW-UP FROM PREVIOUS MEETINGS\n");
                foreach (var a in priorAssignments)
                {
                    var assignedDate = a.Meeting.MeetingDate.ToLocalTime().ToString("MMMM d, yyyy");
                    builder.AddBullet($"{a.Owner?.Name ?? "Unassigned"} — {a.Description} (assigned {assignedDate})\n");
                }
                builder.AddText("\n", "NORMAL");
            }

            // Handbook Training
            builder.AddHeading("HANDBOOK TRAINING\n");
            builder.AddText($"Assigned: {handbookTrainer}\n", "NORMAL");
            builder.AddText("Section: \n", "NORMAL");
            builder.AddText("Discussion Question: \n\n", "NORMAL");

            // Discussion
            builder.AddHeading("DISCUSSION\n");
            builder.AddText("1. \n", "NORMAL");
            builder.AddText("2. \n", "NORMAL");
            builder.AddText("3. \n\n", "NORMAL");

            // Notes
            builder.AddHeading("NOTES\n");
            builder.AddText("\n\n\n", "NORMAL");

            // Assignments from this meeting
            if (assignments.Any())
            {
                builder.AddHeading("ASSIGNMENTS\n");
                foreach (var a in assignments)
                {
                    var due = a.FollowUpDate == default ? "" : $" by {a.FollowUpDate:MMMM d}";
                    builder.AddBullet($"{a.Owner?.Name ?? "Unassigned"} — {a.Description}{due}\n");
                }
                builder.AddText("\n", "NORMAL");
            }

            // Closing
            builder.AddHeading("CLOSING\n");
            builder.AddText($"Prayer: {closingPrayer}\n", "NORMAL");

            return builder.Build();
        }

        private string GetAssignedDisplayName(List<Models.AgendaItem> agendaItems, string rotationType)
        {
            var item = agendaItems.FirstOrDefault(a => a.ItemType == rotationType);
            if (item == null) return "";

            // Use mapped member name if position is linked
            if (item.Position != null)
            {
                var memberName = item.Position.MemberPositions
                    .OrderByDescending(mp => mp.EffectiveDate)
                    .Select(mp => mp.Member?.Name)
                    .FirstOrDefault();
                return memberName ?? item.Position.Title;
            }

            return item.Notes ?? "";
        }
    }

    // Fluent builder that tracks index position and accumulates Docs API requests
    internal class DocBuilder
    {
        private readonly List<(string text, string style, bool isBullet)> _segments = new();

        public DocBuilder AddText(string text, string style)
        {
            _segments.Add((text, style, false));
            return this;
        }

        public DocBuilder AddHeading(string text)
        {
            _segments.Add((text, "HEADING_2", false));
            return this;
        }

        public DocBuilder AddBullet(string text)
        {
            _segments.Add((text, "NORMAL", true));
            return this;
        }

        public List<Request> Build()
        {
            var requests = new List<Request>();

            // Step 1: insert all text in reverse order (Docs API inserts at index,
            // so inserting front-to-back would shift subsequent indices)
            // Easier: insert all as one pass forward starting at index 1
            int index = 1;
            var indexedSegments = new List<(string text, string style, bool isBullet, int startIndex)>();

            foreach (var (text, style, isBullet) in _segments)
            {
                indexedSegments.Add((text, style, isBullet, index));
                requests.Add(new Request
                {
                    InsertText = new InsertTextRequest
                    {
                        Text = text,
                        Location = new Location { Index = index }
                    }
                });
                index += text.Length;
            }

            // Step 2: apply paragraph styles
            foreach (var (text, style, isBullet, startIndex) in indexedSegments)
            {
                if (text == "\n" || text.Trim() == "") continue;

                var endIndex = startIndex + text.Length;

                if (style == "TITLE" || style == "SUBTITLE" || style.StartsWith("HEADING"))
                {
                    requests.Add(new Request
                    {
                        UpdateParagraphStyle = new UpdateParagraphStyleRequest
                        {
                            Range = new DocsRange { StartIndex = startIndex, EndIndex = endIndex },
                            ParagraphStyle = new ParagraphStyle { NamedStyleType = style },
                            Fields = "namedStyleType"
                        }
                    });
                }

                if (style == "HEADING_2")
                {
                    // Bold + spacing above heading
                    requests.Add(new Request
                    {
                        UpdateTextStyle = new UpdateTextStyleRequest
                        {
                            Range = new DocsRange { StartIndex = startIndex, EndIndex = endIndex - 1 },
                            TextStyle = new TextStyle { Bold = true },
                            Fields = "bold"
                        }
                    });
                }

                if (isBullet)
                {
                    requests.Add(new Request
                    {
                        CreateParagraphBullets = new CreateParagraphBulletsRequest
                        {
                            Range = new DocsRange { StartIndex = startIndex, EndIndex = endIndex },
                            BulletPreset = "BULLET_DISC_CIRCLE_SQUARE"
                        }
                    });
                }
            }

            return requests;
        }
    }
}