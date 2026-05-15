# Next Steps

## Known Bugs to Fix
- local deploys still use the native publish + simple Dockerfile approach, while CI uses Dockerfile.multistage.
1. **Names showing "Unknown" in People list** — `GetMyPeople` Clerk call returns null name properties for Google OAuth users; fall back to external account name
2. **Role dropdown not re-rendering** — onChange fires but state mutation isn't creating new array reference; fix by creating new array

## Build Priorities (After Deploy)
1. Persist attendee checkbox state to API (build AgendaAttendees controller/service/DTO)
2. Auto-assign prayer/training rotation from checked attendees
3. Wire HandbookSection.tsx to existing handbook controller + AI question generation
4. Wire FollowUpSection.tsx to show open assignments from prior meetings
5. Wire DiscussionSection.tsx to ministry areas + topic backlog (build TopicBacklogItem API)
6. Build NotesSection.tsx wiring (AgendaNotes API needed)
7. Build PrepFocusSection.tsx with OpenAI Prep assistant
8. Attachments API + UI
9. Recurring responsibilities surfacing on agenda (RecurringResponsibility API + UI)
10. OrganizationSettings API + UI (meeting schedule config)
11. Layout components: MobileBottomNav, Sidebar
12. useMeeting hook

## Working Patterns
- Working directly in chat (not Claude Code) for tighter control
- Commit after each meaningful chunk with descriptive messages
- Backend pattern: Controller → Service (interface) → DbContext; DTOs for all API responses