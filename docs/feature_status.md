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
- OrgPositionsController — CRUD + position mapping
- AgendaAttendeesController — attendance per meeting, guest management
- GoogleOAuthController — OAuth2 connect + callback, stores refresh token per org
- AgendaExportService — assembles agenda, pushes to Google Doc via OAuth2
- GoogleDocService — Google Docs/Drive API via per-org OAuth2 credentials
- DefaultPositions.cs — seeds standing + guest positions per org type on creation
- Rotation logic refactored to position-based (OrgPositions/RotationLogs)
- AgendaGeneratorService updated — position-based rotation, meeting-date-aware, cross-org handbook exclusion, opening/closing prayer conflict prevention

### Schema Migrated, API Not Yet Built
Models exist and are in the DbContext but no controllers/services/DTOs yet:
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
- AttendeesSection.tsx — fully wired: standing positions, attendance toggle, guest invites, rotation assignments with last-assigned history, generate button
- OrgLandingPage — delete meeting with confirmation (draft/generated only, not published)
- AgendaEditorPage — export button, view doc button, auto-save date/time on blur

### Not Yet Built
- useMeeting hook
- HandbookSection wiring + AI question generation
- DiscussionSection AI question generation UI
- Follow-up carry-forward UI
- NotesSection auto-save + AI assignment extraction
- Prep & Focus AI integration
- MobileBottomNav, Sidebar layout components
- OrganizationSettings UI
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