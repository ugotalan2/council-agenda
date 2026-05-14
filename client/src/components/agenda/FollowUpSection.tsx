interface Props {
  orgId: string
  meetingId: string
  members: { id: string; name: string; calling: string }[]
}
export default function FollowUpSection({ orgId }: Props) {
  return (
    <div>
      <h5 className="mb-3">Follow-up Assignments</h5>
      <div className="alert alert-info">Coming soon — carry-forward assignments.</div>
    </div>
  )
}