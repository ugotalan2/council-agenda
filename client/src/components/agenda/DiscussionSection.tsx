interface Props { orgId: string; meetingId: string }
export default function DiscussionSection({ orgId }: Props) {
  return (
    <div>
      <h5 className="mb-3">Discussion Topics</h5>
      <div className="alert alert-info">Coming soon — ministry area topics and backlog.</div>
    </div>
  )
}