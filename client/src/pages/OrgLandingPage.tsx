import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useAuth } from '@clerk/clerk-react'
import TopNav from '../components/layout/TopNav'
import LoadingSpinner from '../components/shared/LoadingSpinner'
import ErrorAlert from '../components/shared/ErrorAlert'
import { useOrganization } from '../lib/hooks/useOrganization'
import api, { setAuthToken } from '../lib/api'

interface Meeting {
  id: string
  meetingDate: string
  status: string
  agendaGenerated: boolean
  agendaPublished: boolean
}

const ORG_TYPE_LABELS: Record<string, string> = {
  ward_council: 'Ward Council',
  bishopric: 'Bishopric Meeting',
  ward_youth_council: 'Ward Youth Council',
  family_council: 'Family Council',
  presidency_meeting: 'Presidency Meeting',
}

const STATUS_BADGES: Record<string, string> = {
  draft: 'bg-secondary',
  published: 'bg-success',
  past: 'bg-light text-dark',
}

export default function OrgLandingPage() {
  const { orgId } = useParams<{ orgId: string }>()
  const navigate = useNavigate()
  const { getToken, isLoaded } = useAuth()
  const { org, members, loading, error } = useOrganization(orgId ?? '')
  const [meetings, setMeetings] = useState<Meeting[]>([])
  const [meetingsLoading, setMeetingsLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<'agendas' | 'members' | 'responsibilities'>('agendas')
  const [showAddMember, setShowAddMember] = useState(false)
  const [newMemberName, setNewMemberName] = useState('')
  const [newMemberCalling, setNewMemberCalling] = useState('')
  const [saving, setSaving] = useState(false)
  const [successMessage, setSuccessMessage] = useState<string | null>(() => {
    const params = new URLSearchParams(window.location.search)
    if (params.get('googleConnected') === 'true') {
      window.history.replaceState({}, '', window.location.pathname)
      return 'Google account connected successfully!'
    }
    return null
  })

  useEffect(() => {
    if (!isLoaded || !orgId) return
    const fetchMeetings = async () => {
      try {
        const token = await getToken()
        setAuthToken(token)
        const res = await api.get(`/api/v1/organizations/${orgId}/meetings`)
        setMeetings(res.data)
      } finally {
        setMeetingsLoading(false)
      }
    }
    fetchMeetings()
  }, [isLoaded, orgId, getToken])

  const createAgenda = async () => {
    try {
      const token = await getToken()
      setAuthToken(token)
      const res = await api.post(`/api/v1/organizations/${orgId}/meetings`, {
        meetingDate: new Date().toISOString(),
      })
      navigate(`/organizations/${orgId}/meetings/${res.data.id}`)
    } catch (err: unknown) {
      console.error(err instanceof Error ? err.message : 'An error occurred')
    }
  }

  const addMember = async () => {
    if (!newMemberName.trim()) return
    setSaving(true)
    try {
      const token = await getToken()
      setAuthToken(token)
      await api.post(`/api/v1/organizations/${orgId}/members`, {
        name: newMemberName,
        calling: newMemberCalling,
        clerkUserId: '',
      })
      setNewMemberName('')
      setNewMemberCalling('')
      setShowAddMember(false)
    } finally {
      setSaving(false)
    }
  }

  const deleteMeeting = async (e: React.MouseEvent, meetingId: string) => {
    e.stopPropagation() // prevent navigating to the meeting
    if (!window.confirm('Delete this agenda? This cannot be undone.')) return
    try {
      const token = await getToken()
      setAuthToken(token)
      await api.delete(`/api/v1/organizations/${orgId}/meetings/${meetingId}`)
      setMeetings((prev) => prev.filter((m) => m.id !== meetingId))
    } catch {
      // ignore
    }
  }

  const connectGoogle = () => {
    window.location.href = `${import.meta.env.VITE_API_URL}/api/v1/google/connect/${orgId}`
  }

  if (loading) return <LoadingSpinner />
  if (error) return <ErrorAlert message={error} />

  return (
    <div className="min-vh-100 bg-light">
      <TopNav
        title={org?.name ?? 'Loading...'}
        subtitle={ORG_TYPE_LABELS[org?.orgType ?? ''] ?? ''}
        backTo="/"
        backLabel="Dashboard"
      />

      {successMessage && (
        <div className="alert alert-success alert-dismissible mx-3 mt-3" role="alert">
          {successMessage}
          <button type="button" className="btn-close" onClick={() => setSuccessMessage(null)} />
        </div>
      )}

      <button className="btn btn-outline-secondary btn-sm" onClick={connectGoogle}>
        🔗 Connect Google Account
      </button>

      <div className="container py-4">
        {error && <ErrorAlert message={error} />}

        {loading ? (
          <LoadingSpinner />
        ) : (
          <>
            {/* Tabs */}
            <ul className="nav nav-tabs mb-4">
              <li className="nav-item">
                <button
                  className={`nav-link ${activeTab === 'agendas' ? 'active' : ''}`}
                  onClick={() => setActiveTab('agendas')}
                >
                  Agendas
                </button>
              </li>
              <li className="nav-item">
                <button
                  className={`nav-link ${activeTab === 'members' ? 'active' : ''}`}
                  onClick={() => setActiveTab('members')}
                >
                  Members
                </button>
              </li>
              <li className="nav-item">
                <button
                  className={`nav-link ${activeTab === 'responsibilities' ? 'active' : ''}`}
                  onClick={() => setActiveTab('responsibilities')}
                >
                  Responsibilities
                </button>
              </li>
            </ul>

            {/* Agendas Tab */}
            {activeTab === 'agendas' && (
              <div>
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <h5 className="mb-0">Meeting Agendas</h5>
                  <button className="btn btn-primary btn-sm" onClick={createAgenda}>
                    + New Agenda
                  </button>
                </div>

                {meetingsLoading && <LoadingSpinner />}

                {!meetingsLoading && meetings.length === 0 && (
                  <div className="alert alert-info">
                    No agendas yet. Create your first one to get started.
                  </div>
                )}

                {meetings.map((meeting) => (
                  <div
                    key={meeting.id}
                    className="card mb-3"
                    onClick={() => navigate(`/organizations/${orgId}/meetings/${meeting.id}`)}
                    style={{ cursor: 'pointer' }}
                  >
                    <div className="card-body d-flex justify-content-between align-items-center">
                      <div>
                        <h6 className="card-title mb-1">
                          {new Date(meeting.meetingDate).toLocaleDateString('en-US', {
                            weekday: 'long',
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                          })}
                        </h6>
                        <small className="text-muted">
                          {meeting.agendaGenerated ? 'Agenda generated' : 'Draft'}
                        </small>
                      </div>
                      <div className="d-flex align-items-center gap-2">
                        <span
                          className={`badge ${STATUS_BADGES[meeting.status] ?? 'bg-secondary'}`}
                        >
                          {meeting.status}
                        </span>
                        {meeting.status !== 'published' && (
                          <button
                            className="btn btn-link btn-sm text-danger p-0"
                            onClick={(e) => deleteMeeting(e, meeting.id)}
                            title="Delete agenda"
                          >
                            🗑
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {/* Members Tab */}
            {activeTab === 'members' && (
              <div>
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <h5 className="mb-0">Council Members</h5>
                  <button
                    className="btn btn-primary btn-sm"
                    onClick={() => setShowAddMember(!showAddMember)}
                  >
                    + Add Member
                  </button>
                </div>

                {showAddMember && (
                  <div className="card mb-3">
                    <div className="card-body">
                      <h6 className="card-title">Add Member</h6>
                      <div className="mb-2">
                        <label className="form-label small">Name</label>
                        <input
                          className="form-control form-control-sm"
                          value={newMemberName}
                          onChange={(e) => setNewMemberName(e.target.value)}
                          placeholder="Full name"
                        />
                      </div>
                      <div className="mb-3">
                        <label className="form-label small">Calling</label>
                        <input
                          className="form-control form-control-sm"
                          value={newMemberCalling}
                          onChange={(e) => setNewMemberCalling(e.target.value)}
                          placeholder="e.g. Relief Society President"
                        />
                      </div>
                      <div className="d-flex gap-2">
                        <button
                          className="btn btn-primary btn-sm"
                          onClick={addMember}
                          disabled={saving}
                        >
                          {saving ? 'Saving...' : 'Save'}
                        </button>
                        <button
                          className="btn btn-outline-secondary btn-sm"
                          onClick={() => setShowAddMember(false)}
                        >
                          Cancel
                        </button>
                      </div>
                    </div>
                  </div>
                )}

                {members.length === 0 && (
                  <div className="alert alert-info">
                    No members yet. Add council members to get started.
                  </div>
                )}

                {members.map((member) => (
                  <div key={member.id} className="card mb-2">
                    <div className="card-body py-2 d-flex justify-content-between align-items-center">
                      <div>
                        <div className="fw-semibold">{member.name}</div>
                        <small className="text-muted">{member.calling}</small>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {/* Responsibilities Tab */}
            {activeTab === 'responsibilities' && (
              <div>
                <h5 className="mb-3">Recurring Responsibilities</h5>
                <div className="alert alert-info">
                  Responsibilities will appear here. Coming soon.
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}
