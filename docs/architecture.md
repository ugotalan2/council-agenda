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

### Seed Data
- `Data/DefaultResponsibilities.cs` seeds recurring responsibilities per org type on org creation

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