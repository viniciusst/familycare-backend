# Backend TO-DO

Living document tracking pending backend work. Updated as needs surface
from product use, frontend development, or refactoring discoveries.

---

## ✅ Recently completed

### Phase 1G — Edit appointment details

`PATCH /api/v1/appointments/{id}/details` for fixing typos in doctor name,
specialty, location, or notes of a scheduled appointment. Cancelled or
completed appointments cannot be edited via this endpoint (historical
record protected). Status: **shipped**.

### Phase 1F-3 Session 2 — List invitations endpoints

Three endpoints to make the invitation lifecycle visible:

- `GET /api/v1/invitations` — authenticated user's inbox (filtered by email)
- `GET /api/v1/families/{familyId}/invitations` — admin view, Owner/Admin only
- `GET /api/v1/invitations/{invitationId}` — single invitation, visible to invitee or Owner/Admin

Status: **shipped**.

---

## 🟡 Active backlog

### Phase 1F-5 — Member editable details (NEW)

**Discovered:** users have no way to fix typos on a family member's name,
birthdate, or relationship after creation. Domain already supports the
operations (`Family.RenameMember`, `Family.ChangeMemberRelationship`),
but no API endpoint exists.

**Endpoint:**

```http
PATCH /api/v1/families/{familyId}/members/{memberId}/details
```

**Body** (partial update — at least one field required):

```json
{
  "displayName": "João Silva",
  "birthDate": "2015-03-22",
  "relationship": "Child"
}
```

**Domain additions:**

- `Family.ChangeMemberBirthDate(memberId, newBirthDate)` — does not exist yet,
  need to add. Should raise `FamilyMemberUpdatedEvent`.
- Existing methods to wire: `RenameMember`, `ChangeMemberRelationship`.

**Application:**

- `UpdateMemberDetailsCommand(FamilyId, FamilyMemberId, string? DisplayName, DateOnly? BirthDate, RelationshipType? Relationship)`
- Validator (lengths, dates in the past)
- Handler: route to the right Family method based on which fields are present

**Authorization:**

- Owner / Admin can edit anyone
- Adult / regular member can edit only themselves (their own member record)

**Open questions:**

- Should we restrict birth date editing? Could be sensitive if used for
  medical-history age calculations.
- Cascade implications: editing birthdate might invalidate cached
  age-based privacy rules. Probably nothing to do — just note it.

---

### Phase 1F-6 — Transfer ownership endpoint (NEW)

**Discovered:** domain has `Family.TransferOwnership(newOwnerMemberId)` but
no API endpoint exposes it. Without this, the current Owner cannot pass
the family on (e.g. when a parent wants to make a teenager the Owner).

**Endpoint:**

```http
POST /api/v1/families/{familyId}/transfer-ownership
```

**Body:**

```json
{
  "newOwnerMemberId": "<guid>"
}
```

**Authorization:** only the current Owner.

**Tests to cover:** new owner must already be a member of the family,
cannot transfer to self, role of the previous Owner becomes Admin.

---

### Phase 1F-4 — Family lifecycle: deactivate (DEFERRED)

**Status:** deferred — not blocking current roadmap.

**Rationale:** archive/restore is a low-frequency operation. Backend is
functionally complete without it. Will revisit when real user feedback
or compliance requirements surface.

**Original scope (kept here as reference):**

#### Domain

- Add `FamilyStatus` enum: `Active`, `Archived`, `Deleted`
- Add `Family.Status` property + transition methods (`Archive`, `Restore`)
- Add domain events: `FamilyArchivedEvent`, `FamilyRestoredEvent`
- Auto-revoke all pending invitations on archive

#### Persistence

- New columns on `families`: `status`, `archived_at`, `archived_by_user_id`, `archive_reason`
- Global query filter to hide non-Active families by default
- Migration

#### Endpoints

```http
POST   /api/v1/families/{id}/archive    body: { reason: "..." }
POST   /api/v1/families/{id}/restore
GET    /api/v1/families?includeArchived=true
```

#### Cascade behavior

- Pending invitations auto-revoked
- Medical history queries return empty for the family by default
- All records preserved (rows stay in DB — soft-delete only)

---

## 🟢 Frontend-only (no backend work needed)

### Phase 2D — Privacy rules UI

The endpoint
`PUT /api/v1/families/{familyId}/members/{memberId}/privacy-rules/{category}`
already exists. Frontend UI is the only remaining work; tracked in the
frontend roadmap.

---

## 💭 Future / not prioritized

### Email notifications

Outbox pattern or background service for:

- Member invited
- Invitation expiring in 24h
- Family ownership transferred
- Family archived (if 1F-4 ever ships)
- Appointment reminder (24h before)
- Appointment rescheduled / cancelled

### Audit log

Generic event audit table for compliance/debugging. Listens to all domain
events and persists a record. Not required for MVP.

### Soft-delete of medical entities

Appointments and exams currently support state transitions
(Scheduled/Completed/Cancelled), but allergies, vaccines, and chronic
conditions are hard-delete only. Consider soft-delete for medical
records (compliance / audit).