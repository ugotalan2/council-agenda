import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useAuth } from '@clerk/clerk-react'
import api, { setAuthToken } from '../lib/api'
import TopNav from '../components/layout/TopNav'
import LoadingSpinner from '../components/shared/LoadingSpinner'
import ErrorAlert from '../components/shared/ErrorAlert'
import AttendeesSection from '../components/agenda/AttendeesSection'
import HandbookSection from '../components/agenda/HandbookSection'
import DiscussionSection from '../components/agenda/DiscussionSection'
import FollowUpSection from '../components/agenda/FollowUpSection'
import NotesSection from '../components/agenda/NotesSection'
import PrepFocusSection from '../components/agenda/PrepFocusSection'

type AgendaSection = 'attendees' | 'handbook' | 'discussion' | 'followup' | 'notes' | 'prep'

const SECTIONS: { id: AgendaSection; label: string; icon: string }[] = [
  { id: 'attendees', label: 'Attendees', icon: '👥' },
  { id: 'handbook', label: 'Handbook', icon: '📖' },
  { id: 'discussion', label: 'Discussion', icon: '💬' },
  { id: 'followup', label: 'Follow-up', icon: '✅' },
  { id: 'notes', label: 'Notes', icon: '📝' },
  { id: 'prep', label: 'Prep & Focus', icon: '🎯' },
]

interface Meeting {
  id: string
  meetingDate: string
  status: string
  agendaGenerated: boolean
  agendaPublished: boolean
}

interface Member {
  id: string
  name: string
  calling: string
}

export default function AgendaEditorPage() {
  const { orgId, meetingId } = useParams<{ orgId: string; meetingId: string }>()
  const navigate = useNavigate()
  const { getToken, isLoaded } = useAuth()
  const [meeting, setMeeting] = useState<Meeting | null>(null)
  const [members, setMembers] = useState<Member[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [activeSection, setActiveSection] = useState<AgendaSection>('attendees')
  const [meetingDate, setMeetingDate] = useState('')
  const [meetingTime, setMeetingTime] = useState('11:00')
  const [savingDate, setSavingDate] = useState(false)

  useEffect(() => {
    if (!isLoaded || !orgId || !meetingId) return
    const fetch = async () => {
      try {
        const token = await getToken()
        setAuthToken(token)
        const [meetingRes, membersRes] = await Promise.all([
          api.get(`/api/v1/organizations/${orgId}/meetings/${meetingId}`),
          api.get(`/api/v1/organizations/${orgId}/members`)
        ])
        setMeeting(meetingRes.data)
        setMembers(membersRes.data)
        const date = new Date(meetingRes.data.meetingDate)
        setMeetingDate(date.toISOString().split('T')[0])
        setMeetingTime(date.toTimeString().slice(0, 5))
      } catch (err: any) {
        setError(err.message)
      } finally {
        setLoading(false)
      }
    }
    fetch()
  }, [isLoaded, orgId, meetingId, getToken])

  const saveMeetingDate = async () => {
    if (!meetingDate) return
    setSavingDate(true)
    try {
      const token = await getToken()
      setAuthToken(token)
      const combined = new Date(`${meetingDate}T${meetingTime}:00`)
      await api.put(`/api/v1/organizations/${orgId}/meetings/${meetingId}`, {
        meetingDate: combined.toISOString()
      })
    } finally {
      setSavingDate(false)
    }
  }

  const formattedDate = meeting
    ? new Date(meeting.meetingDate).toLocaleDateString('en-US', {
        weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
      })
    : ''

  if (loading) return <LoadingSpinner />
  if (error) return <ErrorAlert message={error} />

  return (
    <div className="min-vh-100 bg-light">
      <TopNav
        title={formattedDate}
        backTo={`/organizations/${orgId}`}
        backLabel="Agendas"
      />

      {/* Meeting date/time bar */}
      <div className="bg-white border-bottom px-3 py-2">
        <div className="container-fluid d-flex align-items-center gap-3 flex-wrap">
          <div className="d-flex align-items-center gap-2">
            <label className="form-label mb-0 small text-muted">Date</label>
            <input
              type="date"
              className="form-control form-control-sm"
              style={{ width: 'auto' }}
              value={meetingDate}
              onChange={e => setMeetingDate(e.target.value)}
            />
          </div>
          <div className="d-flex align-items-center gap-2">
            <label className="form-label mb-0 small text-muted">Time</label>
            <input
              type="time"
              className="form-control form-control-sm"
              style={{ width: 'auto' }}
              value={meetingTime}
              onChange={e => setMeetingTime(e.target.value)}
            />
          </div>
          <button
            className="btn btn-outline-primary btn-sm"
            onClick={saveMeetingDate}
            disabled={savingDate}
          >
            {savingDate ? 'Saving...' : 'Save'}
          </button>
          <span className={`badge ms-auto ${meeting?.status === 'published' ? 'bg-success' : 'bg-secondary'}`}>
            {meeting?.status}
          </span>
        </div>
      </div>

      <div className="d-flex" style={{ minHeight: 'calc(100vh - 120px)' }}>
        {/* Desktop sidebar */}
        <div
          className="d-none d-md-flex flex-column bg-white border-end py-3"
          style={{ width: 180, minWidth: 180 }}
        >
          {SECTIONS.map(section => (
            <button
              key={section.id}
              className={`btn btn-link text-start px-3 py-2 text-decoration-none ${
                activeSection === section.id
                  ? 'fw-semibold text-primary border-end border-primary border-3'
                  : 'text-secondary'
              }`}
              onClick={() => setActiveSection(section.id)}
            >
              <span className="me-2">{section.icon}</span>
              {section.label}
            </button>
          ))}
        </div>

        {/* Main content */}
        <div className="flex-grow-1 p-3 p-md-4" style={{ maxWidth: 800 }}>
          {activeSection === 'attendees' && (
            <AttendeesSection
              orgId={orgId!}
              meetingId={meetingId!}
              members={members}
            />
          )}
          {activeSection === 'handbook' && (
            <HandbookSection
              orgId={orgId!}
              meetingId={meetingId!}
            />
          )}
          {activeSection === 'discussion' && (
            <DiscussionSection
              orgId={orgId!}
              meetingId={meetingId!}
            />
          )}
          {activeSection === 'followup' && (
            <FollowUpSection
              orgId={orgId!}
              meetingId={meetingId!}
              members={members}
            />
          )}
          {activeSection === 'notes' && (
            <NotesSection
              orgId={orgId!}
              meetingId={meetingId!}
              members={members}
            />
          )}
          {activeSection === 'prep' && (
            <PrepFocusSection
              orgId={orgId!}
              meetingId={meetingId!}
            />
          )}
        </div>
      </div>

      {/* Mobile bottom nav */}
      <div
        className="d-md-none fixed-bottom bg-white border-top d-flex"
        style={{ zIndex: 1000 }}
      >
        {SECTIONS.map(section => (
          <button
            key={section.id}
            className={`btn btn-link flex-grow-1 py-2 text-decoration-none d-flex flex-column align-items-center ${
              activeSection === section.id ? 'text-primary' : 'text-secondary'
            }`}
            style={{ fontSize: 10 }}
            onClick={() => setActiveSection(section.id)}
          >
            <span style={{ fontSize: 18 }}>{section.icon}</span>
            {section.label}
          </button>
        ))}
      </div>
    </div>
  )
}