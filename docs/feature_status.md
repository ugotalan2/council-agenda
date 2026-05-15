# Feature Status

## Backend

### Built
- 8 controllers: Organizations, Members, Meetings, Handbook, MinistryAreas, Assignments, People, Invitations
- 7 services with interfaces: Organization, Member, Meeting, Handbook, MinistryArea, Assignment, Invitation
- 2 internal services: AgendaGeneratorService (rotation + follow-up carry-forward), DiscussionQuestionService (OpenAI)
- DTOs: Assignment, Handbook, Invitation, Meeting, Member, MinistryArea, Organization
- BaseController with auth helpers
- API versioning (v1)
- Global exception handling middleware
- Authorization policies (OrgAccessHandler, OrgAccessRequirement)
- Route constants
- Invitations system with Clerk SDK + multi-org invite (InvitationOrganization join)
- Sync endpoint for pre-existing Clerk accounts
- DefaultResponsibilities seeding

### Schema Migrated, API Not Yet Built
Models exist and are in the DbContext but no controllers/services/DTOs yet:
- AgendaAttendee
- AgendaNote
- Attachment
- RecurringResponsibility
- ResponsibilityCheckin
- OrganizationSettings
- TopicBacklogItem

## Frontend

### Built
- Dashboard with Organizations + People tabs
- People management UI (per-org access, invite by email)
- OrgLandingPage with agendas + members tabs
- AgendaEditorPage shell with section navigation
- All 6 agenda section components scaffolded: Attendees, Discussion, FollowUp, Handbook, Notes, PrepFocus
- TopNav layout component
- Shared: ErrorAlert, LoadingSpinner
- `useOrganization` hook
- `api.ts` client

### Not Yet Built
- MobileBottomNav, Sidebar layout components
- useMeeting hook
- Persisting attendee state
- Auto-assign rotation logic in UI
- AI question generation UI
- Follow-up carry-forward UI
- Notes assignment creation
- Prep & Focus AI integration
- Attachments UI
- Recurring responsibility surfacing

## CI/CD
- `.github/workflows/ci.yml` — build + lint + format + test scaffolds
- `.github/workflows/deploy.yml` — deployment workflow (in progress)
- ESLint flat config, Prettier, .editorconfig
- api.tests project scaffolded (no tests yet)
- vitest scaffold in client (App.test.tsx, setup.ts)
- Docker: Dockerfile, .dockerignore, docker-compose.yml, .env.docker

## Deferred to V2
- Family Council, Ward Youth Council, Presidency Meeting org types
- Email notifications to assignees
- Calling-based role templates
- Clerk webhook for invite auto-acceptance (deferred until Azure live)
- Actual test coverage (scaffolds in place)