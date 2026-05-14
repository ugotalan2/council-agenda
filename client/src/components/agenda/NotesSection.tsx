interface Props {
  orgId: string
  meetingId: string
  members: { id: string; name: string; calling: string }[]
}
export default function NotesSection({ orgId }: Props) {
  return (
    <div>
      <h5 className="mb-3">Meeting Notes</h5>
      <div className="alert alert-info">Coming soon — meeting notes and action items.</div>
    </div>
  )
}