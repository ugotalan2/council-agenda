# Frontend Structure

## File Layout
client/src/
pages/
Dashboard.tsx          ✅
OrgLandingPage.tsx     ✅
AgendaEditorPage.tsx   ✅ shell
components/
layout/
TopNav.tsx           ✅
(MobileBottomNav.tsx — planned)
(Sidebar.tsx — planned)
agenda/
AttendeesSection.tsx       ✅ UI only
DiscussionSection.tsx      ✅ scaffold
FollowUpSection.tsx        ✅ scaffold
HandbookSection.tsx        ✅ scaffold
NotesSection.tsx           ✅ scaffold
PrepFocusSection.tsx       ✅ scaffold
shared/
ErrorAlert.tsx       ✅
LoadingSpinner.tsx   ✅
lib/
api.ts                 ✅
hooks/
useOrganization.ts   ✅
(useMeeting.ts — planned)
test/
App.test.tsx
setup.ts

## Navigation Pattern
- Top nav: app-wide (orgs, profile); hamburger/overflow on mobile
- Left sidebar (desktop) / bottom nav (mobile): agenda-specific sections
- Meeting date/time as persistent header across all agenda sections

## Agenda Editor — 6 Sections
1. **Attendees** — member checklist; auto-assigns prayer/training live as checked/unchecked
2. **Handbook** — auto-suggests next section; allows override; AI questions; auto-linked churchofjesuschrist.org URL
3. **Discussion (Ministry Areas)** — 4-5 topic slots, last-focused dates, ministry area tagging, yes/no backlog prompt, AI Prep assistant
4. **Follow-up** — auto-carries forward open assignments; manual add option
5. **Notes** — post-meeting; per-topic or meeting-level; spawns assignments tied to member + follow-up weeks
6. **Prep & Focus** — AI assistant: bishop prompts topic, returns goal + LCR indicators (auto-linked) + discussion questions + suggested outcomes

## Organization Landing Page
- Previous agendas list (status: draft/published/past)
- New Agenda button (auto-populates date from org settings)
- Settings gear (meeting day, time, frequency, week-of-month)

## Dashboard
- Tab 1: My Organizations
- Tab 2: My People — unified per-org role management + invite by email