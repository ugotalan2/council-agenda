interface Props {
  orgId: string
  meetingId: string
  members: { id: string; name: string; calling: string }[]
}

export default function AttendeesSection({ members }: Props) {
  return (
    <div>
      <h5 className="mb-3">Attendees & Assignments</h5>
      <p className="text-muted small mb-3">
        Check who is attending. Prayer and training will be auto-assigned based on rotation.
      </p>
      {members.length === 0 && (
        <div className="alert alert-info">
          No members yet. Add members from the organization page.
        </div>
      )}
      {members.map(member => (
        <div key={member.id} className="card mb-2">
          <div className="card-body py-2 d-flex align-items-center gap-3">
            <input type="checkbox" className="form-check-input mt-0" defaultChecked />
            <div>
              <div className="fw-semibold">{member.name}</div>
              <small className="text-muted">{member.calling}</small>
            </div>
          </div>
        </div>
      ))}
    </div>
  )
}