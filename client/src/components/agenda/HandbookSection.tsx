interface Props { orgId: string; meetingId: string }
export default function HandbookSection({ orgId: _orgId, meetingId: _meetingId }: Props) {
  return (
    <div>
      <h5 className="mb-3">Handbook Training</h5>
      <div className="alert alert-info">Coming soon — handbook section selector and AI questions.</div>
    </div>
  )
}