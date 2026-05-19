# Next Steps

## Known Bugs to Fix
- Clerk is still on dev keys (no custom domain) — intentional for now
- Local deploys use native publish + simple Dockerfile; CI uses Dockerfile.multistage
- Timezone is hardcoded to America/Chicago in AgendaExportService — should be stored in OrganizationSettings long term
- GoogleDriveFolderId must be manually set in DB for new orgs until Settings UI is built

## Build Priorities
1. Wire FollowUpSection — show open assignments from prior meetings (backend exists)
2. Wire HandbookSection — connect to handbook controller + AI question generation
3. Wire DiscussionSection — ministry areas + topic backlog (build TopicBacklogItem API)
4. Build NotesSection — auto-save every 30s + AI assignment extraction post-meeting
5. Build PrepFocusSection — OpenAI prep assistant
6. OrganizationSettings API + UI (meeting schedule config)
7. Attachments API + UI
8. Recurring responsibilities surfacing on agenda
9. Layout components: MobileBottomNav, Sidebar
10. useMeeting hook
11. Unit tests for rotation logic (scaffolds in place)
12. Position mapping UI — let admin map Member to OrgPosition
13. Delete orphan drafts on org landing page (hide toggle)
14. Apply Supabase migrations + seed OrgPositions for production orgs

## Working Patterns
- Working directly in chat (not Claude Code) for tighter control
- Commit after each meaningful chunk with descriptive messages
- Backend pattern: Controller → Service (interface) → DbContext; DTOs for all API responses
- Frontend pattern: hooks fetch data, components are dumb, api.ts for all calls
- Circular reference fix: ReferenceHandler.IgnoreCycles set globally in Program.cs

## Google Drive Notes
- Each org needs GoogleDriveFolderId set (pgAdmin or future settings UI)
- Each org needs admin to connect Google account once via "Connect Google Account" button
- Service account approach was abandoned due to storage quota issues with personal Gmail
- OAuth refresh tokens persist indefinitely until revoked

## Position Seeding Notes
- New orgs get positions seeded automatically via DefaultPositions.cs on creation
- Existing orgs need manual SQL INSERT (see session notes)
- ward_council and bishopric fully seeded
- ward_youth_council seeded
- family_council, presidency_meeting — no positions defined yet