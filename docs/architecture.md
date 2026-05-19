# Architecture

## Multi-Tenant Model
- Users can belong to multiple Organizations
- Users can create their own Orgs and admin them
- Each Org has Members with per-org roles: admin, editor, viewer
- All data scoped by `OrganizationId`

## Org Types (V1)
- Ward Council
- Bishopric

Deferred to V2: Family Council, Ward Youth Council, Presidency Meeting

## Schema

### Core Tables
- `Organizations` — name, type, conducting rotation flag
- `Members` — scoped to org, references ClerkUserId
- `UserOrganizations` — user-to-org join with role
- `Meetings` — date, status (draft/published/past), agenda flags, Google Doc URL
- `AgendaItems` — meeting_id, item_type, display_order, handbook/backlog refs
- `Assignments` — meeting_id (required), owner_id, optional agenda_item_id, description, follow-up date, status (open/resolved/extended)
- `RotationLog` — tracks last prayer/training/conducting per member
- `MinistryAreas` — scoped to org, last focused date
- `HandbookSections` — content tagged to ministry area, last used
- `TopicBacklogItem` — undated discussion items, priority, used flag

### Extended Tables
- `OrganizationSettings` — meeting day, time, frequency, week-of-month
- `AgendaAttendees` — checklist state per meeting
- `AgendaNotes` — meeting_id required, agenda_item_id nullable (null = meeting-level)
- `Attachments` — org-level reusable OR agenda-item-scoped
- `RecurringResponsibilities` — title, LCR URL, min/max weeks, last checked date, scoped to one org type
- `ResponsibilityCheckins` — acknowledgment history
- `Invitations` + `InvitationOrganization` — pending invites; one invite can grant access to multiple orgs
- `OrgPositions` — org-scoped named positions (Bishop, EQP etc), seeded per org type on creation, IsStanding/IsGuestDefault/IsRotationEligible flags
- `MemberPositions` — maps a Member to a Position with effective date
- `AgendaAttendees` — updated: PositionId (nullable), GuestLabel (nullable), MemberId now nullable
- `RotationLogs` — updated: PositionId replaces MemberId, AssignedDate uses meeting date not generation date

### Seed Data
- `Data/DefaultResponsibilities.cs` seeds recurring responsibilities per org type on org creation

## Google Drive Integration
- Per-org OAuth2 refresh token stored on Organization.GoogleRefreshToken
- Per-org folder ID stored on Organization.GoogleDriveFolderId  
- Admin connects once via /api/v1/google/connect/{orgId}
- Export creates formatted Google Doc in org folder, stores URL on Meeting.GoogleDocUrl
- Google Cloud project: council-agenda (ugotalan account)
- Service account approach abandoned — using OAuth2 with stored refresh token

## Rotation Rules
- 3 assignments per meeting: opening_prayer, closing_prayer, handbook_training
- Opening and closing prayer cannot be assigned to same position in same meeting
- Handbook training excluded cross-org on same calendar day
- Round-robin with shuffle, history-aware
- AssignedDate on RotationLog uses meeting date not generation date
- Rotation history excludes current meeting date (shows previous meetings only)
- Delete meeting cascades rotation log cleanup

## Business Rules

### Conducting
- Fixed (bishop): Ward Council, Bishopric
- Rotates: Family Council, Ward Youth Council (V2)

### Rotation
- Prayer + training round-robin through active checked attendees
- Ministry area focus rotates so none get neglected

### Follow-up Carry-forward
- New agenda checks previous meetings for open assignments
- Auto-invites assigned members, pre-populates follow-up section

### Backlog
- Fills empty agenda slots
- Yes/no prompt to accept backlog suggestions

### Recurring Responsibilities
- Scoped one-or-the-other (sensitive items like temple recommends → bishopric only; broader items like move-in/out → ward council only)
- Surface on agenda when interval (min/max weeks) hits
- Three responses: "checked, nothing urgent" (snooze), "needs follow-up" (creates assignment), ignore (keeps surfacing)

### AI Features
- Discussion question generation from handbook content (3 options, user picks)
- Prep & Focus assistant: bishop describes a topic, returns goal + LCR indicators + discussion questions + suggested outcomes