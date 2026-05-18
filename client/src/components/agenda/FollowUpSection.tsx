interface Props {
  orgId: string
  meetingId: string
  members: { id: string; name: string; calling: string }[]
}
export default function FollowUpSection({
  orgId: _orgId,
  meetingId: _meetingId,
  members: _members,
}: Props) {
  return (
    <div>
      <h5 className="mb-3">Follow-up Assignments</h5>
      <div className="alert alert-info">Coming soon — carry-forward assignments.</div>
    </div>
  )
}
