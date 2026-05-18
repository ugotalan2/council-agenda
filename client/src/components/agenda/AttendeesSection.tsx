import { useState, useEffect } from 'react'
import { useAuth } from '@clerk/clerk-react'
import api, { setAuthToken } from '../../lib/api'
import LoadingSpinner from '../shared/LoadingSpinner'

interface Attendee {
  attendeeId: string | null
  positionId: string | null
  title: string
  displayName: string
  isStanding: boolean
  isRotationEligible: boolean
  attending: boolean
  guestLabel: string | null
}

interface GuestPosition {
  id: string
  title: string
  isGuestDefault: boolean
}

interface PositionResponse {
  id: string
  title: string
  isGuestDefault: boolean
  isStanding: boolean
}

interface Props {
  orgId: string
  meetingId: string
}

export default function AttendeesSection({ orgId, meetingId }: Props) {
  const { getToken } = useAuth()
  const [attendees, setAttendees] = useState<Attendee[]>([])
  const [guestPositions, setGuestPositions] = useState<GuestPosition[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState<string | null>(null)
  const [showAddGuest, setShowAddGuest] = useState(false)
  const [customGuestName, setCustomGuestName] = useState('')
  const [addingGuest, setAddingGuest] = useState(false)

  useEffect(() => {
    if (!orgId || !meetingId) return
    const fetch = async () => {
      try {
        const token = await getToken()
        setAuthToken(token)
        const [attendeesRes, positionsRes] = await Promise.all([
          api.get(`/api/v1/organizations/${orgId}/meetings/${meetingId}/attendees`),
          api.get(`/api/v1/organizations/${orgId}/positions`),
        ])
        setAttendees(attendeesRes.data)
        // Only guest-default positions for the invite dropdown
        setGuestPositions(positionsRes.data.filter((p: PositionResponse) => p.isGuestDefault))
      } finally {
        setLoading(false)
      }
    }
    fetch()
  }, [orgId, meetingId, getToken])

  const toggleAttendance = async (attendee: Attendee) => {
    if (!attendee.positionId) return
    setSaving(attendee.positionId)
    try {
      const token = await getToken()
      setAuthToken(token)
      await api.put(
        `/api/v1/organizations/${orgId}/meetings/${meetingId}/attendees/${attendee.positionId}`,
        { attending: !attendee.attending }
      )
      setAttendees((prev) =>
        prev.map((a) =>
          a.positionId === attendee.positionId ? { ...a, attending: !a.attending } : a
        )
      )
    } finally {
      setSaving(null)
    }
  }

  const addGuestFromDropdown = async (position: GuestPosition) => {
    setAddingGuest(true)
    try {
      const token = await getToken()
      setAuthToken(token)
      const res = await api.post(
        `/api/v1/organizations/${orgId}/meetings/${meetingId}/attendees/guests`,
        { guestLabel: position.title }
      )
      setAttendees((prev) => [
        ...prev,
        {
          attendeeId: res.data.id,
          positionId: null,
          title: position.title,
          displayName: position.title,
          isStanding: false,
          isRotationEligible: false,
          attending: true,
          guestLabel: position.title,
        },
      ])
    } finally {
      setAddingGuest(false)
    }
  }

  const addCustomGuest = async () => {
    if (!customGuestName.trim()) return
    setAddingGuest(true)
    try {
      const token = await getToken()
      setAuthToken(token)
      const res = await api.post(
        `/api/v1/organizations/${orgId}/meetings/${meetingId}/attendees/guests`,
        { guestLabel: customGuestName.trim() }
      )
      setAttendees((prev) => [
        ...prev,
        {
          attendeeId: res.data.id,
          positionId: null,
          title: customGuestName.trim(),
          displayName: customGuestName.trim(),
          isStanding: false,
          isRotationEligible: false,
          attending: true,
          guestLabel: customGuestName.trim(),
        },
      ])
      setCustomGuestName('')
      setShowAddGuest(false)
    } finally {
      setAddingGuest(false)
    }
  }

  const removeGuest = async (attendee: Attendee) => {
    if (!attendee.attendeeId) return
    try {
      const token = await getToken()
      setAuthToken(token)
      await api.delete(
        `/api/v1/organizations/${orgId}/meetings/${meetingId}/attendees/guests/${attendee.attendeeId}`
      )
      setAttendees((prev) => prev.filter((a) => a.attendeeId !== attendee.attendeeId))
    } catch {
      // ignore
    }
  }

  if (loading) return <LoadingSpinner />

  const standing = attendees.filter((a) => a.isStanding)
  const guests = attendees.filter((a) => !a.isStanding)
  const alreadyAddedGuests = guests.map((g) => g.guestLabel?.toLowerCase())
  const availableGuestDefaults = guestPositions.filter(
    (p) => !alreadyAddedGuests.includes(p.title.toLowerCase())
  )

  return (
    <div>
      <h5 className="mb-1">Attendees</h5>
      <p className="text-muted small mb-3">
        Check off who is present. Rotation assignments update automatically.
      </p>

      {/* Standing positions */}
      <div className="card mb-3">
        <div className="card-header small fw-semibold text-muted py-2">Standing Members</div>
        <ul className="list-group list-group-flush">
          {standing.map((attendee) => (
            <li
              key={attendee.positionId}
              className="list-group-item d-flex align-items-center gap-3 py-2"
            >
              <div className="form-check mb-0">
                <input
                  type="checkbox"
                  className="form-check-input"
                  id={`attendee-${attendee.positionId}`}
                  checked={attendee.attending}
                  onChange={() => toggleAttendance(attendee)}
                  disabled={saving === attendee.positionId}
                />
              </div>
              <label
                className={`form-check-label flex-grow-1 mb-0 ${!attendee.attending ? 'text-muted text-decoration-line-through' : ''}`}
                htmlFor={`attendee-${attendee.positionId}`}
                style={{ cursor: 'pointer' }}
              >
                <span className="fw-semibold">{attendee.displayName}</span>
                {attendee.displayName !== attendee.title && (
                  <span className="text-muted small ms-2">({attendee.title})</span>
                )}
              </label>
              {attendee.isRotationEligible && (
                <span className="badge bg-light text-secondary border small">rotation</span>
              )}
              {saving === attendee.positionId && (
                <span className="spinner-border spinner-border-sm text-secondary" />
              )}
            </li>
          ))}
        </ul>
      </div>

      {/* Guests */}
      {guests.length > 0 && (
        <div className="card mb-3">
          <div className="card-header small fw-semibold text-muted py-2">Special Guests</div>
          <ul className="list-group list-group-flush">
            {guests.map((guest) => (
              <li
                key={guest.attendeeId}
                className="list-group-item d-flex align-items-center gap-3 py-2"
              >
                <span className="flex-grow-1">{guest.displayName}</span>
                <button
                  className="btn btn-link btn-sm text-danger p-0"
                  onClick={() => removeGuest(guest)}
                >
                  ✕
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Add guest */}
      <div className="card">
        <div className="card-header small fw-semibold text-muted py-2 d-flex justify-content-between align-items-center">
          <span>Invite Special Guest</span>
          <button
            className="btn btn-link btn-sm p-0 text-primary"
            onClick={() => setShowAddGuest(!showAddGuest)}
          >
            {showAddGuest ? 'Cancel' : '+ Add'}
          </button>
        </div>
        {showAddGuest && (
          <div className="card-body">
            {/* Default guest positions as quick-add buttons */}
            {availableGuestDefaults.length > 0 && (
              <div className="mb-3">
                <p className="small text-muted mb-2">Common invitees:</p>
                <div className="d-flex flex-wrap gap-2">
                  {availableGuestDefaults.map((p) => (
                    <button
                      key={p.id}
                      className="btn btn-outline-secondary btn-sm"
                      onClick={() => addGuestFromDropdown(p)}
                      disabled={addingGuest}
                    >
                      + {p.title}
                    </button>
                  ))}
                </div>
              </div>
            )}
            {/* Custom name input */}
            <div className="d-flex gap-2">
              <input
                type="text"
                className="form-control form-control-sm"
                placeholder="Custom name or calling..."
                value={customGuestName}
                onChange={(e) => setCustomGuestName(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && addCustomGuest()}
              />
              <button
                className="btn btn-primary btn-sm"
                onClick={addCustomGuest}
                disabled={addingGuest || !customGuestName.trim()}
              >
                Add
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
