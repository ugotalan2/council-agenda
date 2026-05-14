interface Props { orgId: string; meetingId: string }
export default function PrepFocusSection({ orgId: _orgId, meetingId: _meetingId }: Props) {
  return (
    <div>
      <h5 className="mb-3">Prep & Focus</h5>
      <div className="alert alert-info">Coming soon — AI meeting preparation assistant.</div>
    </div>
  )
}