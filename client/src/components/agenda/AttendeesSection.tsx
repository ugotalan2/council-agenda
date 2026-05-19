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

interface AgendaItem {
  id: string
  itemType: string
  displayName: string | null
  notes: string | null
  positionId: string | null
}

interface RotationHistory {
  positionId: string
  rotationType: string
  lastAssigned: string | null
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

const ROTATION_LABELS: Record<string, string> = {
  opening_prayer: 'Opening Prayer',
  handbook_training: 'Handbook Training',
  closing_prayer: 'Closing Prayer',
}

export default function AttendeesSection({ orgId, meetingId }: Props) {
  const { getToken } = useAuth()
  const [attendees, setAttendees] = useState<Attendee[]>([])
  const [guestPositions, setGuestPositions] = useState<GuestPosition[]>([])
  const [agendaItems, setAgendaItems] = useState<AgendaItem[]>([])
  const [rotationHistory, setRotationHistory] = useState<RotationHistory[]>([])
  const [loading, setLoading] = useState(true)
  const [generating, setGenerating] = useState(false)
  const [saving, setSaving] = useState<string | null>(null)
  const [showAddGuest, setShowAddGuest] = useState(false)
  const [customGuestName, setCustomGuestName] = useState('')
  const [addingGuest, setAddingGuest] = useState(false)

  const fetchData = async () => {
    try {
      const token = await getToken()
      setAuthToken(token)
      const [attendeesRes, positionsRes, itemsRes, historyRes] = await Promise.all([
        api.get(`/api/v1/organizations/${orgId}/meetings/${meetingId}/attendees`),
        api.get(`/api/v1/organizations/${orgId}/positions`),
        api.get(`/api/v1/organizations/${orgId}/meetings/${meetingId}/agenda-items`),
        api.get(`/api/v1/organizations/${orgId}/meetings/${meetingId}/rotation-history`),
      ])
      setAttendees(attendeesRes.data)
      setGuestPositions(positionsRes.data.filter((p: PositionResponse) => p.isGuestDefault))
      setAgendaItems(itemsRes.data)
      setRotationHistory(historyRes.data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (!orgId || !meetingId) return
    fetchData()
  }, [orgId, meetingId]) // eslint-disable-line react-hooks/exhaustive-deps

  const generateAssignments = async () => {
    setGenerating(true)
    try {
      const token = await getToken()
      setAuthToken(token)
      await api.post(`/api/v1/organizations/${orgId}/meetings/${meetingId}/generate`)
      // Reload agenda items and history after generation
      const [itemsRes, historyRes] = await Promise.all([
        api.get(`/api/v1/organizations/${orgId}/meetings/${meetingId}/agenda-items`),
        api.get(`/api/v1/organizations/${orgId}/meetings/${meetingId}/rotation-history`),
      ])
      setAgendaItems(itemsRes.data)
      setRotationHistory(historyRes.data)
    } finally {
      setGenerating(false)
    }
  }

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

  const getLastAssigned = (positionId: string, rotationType: string) => {
    const log = rotationHistory.find(
      (h) => h.positionId === positionId && h.rotationType === rotationType
    )
    if (!log?.lastAssigned) return 'Never assigned'
    return new Date(log.lastAssigned).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    })
  }

  if (loading) return <LoadingSpinner />

  const standing = attendees.filter((a) => a.isStanding)
  const guests = attendees.filter((a) => !a.isStanding)
  const alreadyAddedGuests = guests.map((g) => g.guestLabel?.toLowerCase())
  const availableGuestDefaults = guestPositions.filter(
    (p) => !alreadyAddedGuests.includes(p.title.toLowerCase())
  )
  const hasAssignments = agendaItems.some((i) =>
    ['opening_prayer', 'handbook_training', 'closing_prayer'].includes(i.itemType)
  )

  return (
    <div>
      <h5 className="mb-1">Attendees</h5>
      <p className="text-muted small mb-3">
        Check off who is present. Rotation assignments update automatically.
      </p>

      {/* Meeting Assignments */}
      <div className="card mb-3">
        <div className="card-header small fw-semibold text-muted py-2 d-flex justify-content-between align-items-center">
          <span>Meeting Assignments</span>
          {!hasAssignments && (
            <button
              className="btn btn-primary btn-sm"
              onClick={generateAssignments}
              disabled={generating}
            >
              {generating ? 'Generating...' : '⚡ Generate'}
            </button>
          )}
        </div>
        {!hasAssignments ? (
          <div className="card-body text-center py-4">
            <p className="text-muted small mb-0">
              No assignments yet. Click Generate to auto-assign based on rotation history.
            </p>
          </div>
        ) : (
          <ul className="list-group list-group-flush">
            {['opening_prayer', 'handbook_training', 'closing_prayer'].map((type) => {
              const item = agendaItems.find((i) => i.itemType === type)
              if (!item) return null
              return (
                <li key={type} className="list-group-item py-2">
                  <div className="d-flex justify-content-between align-items-start">
                    <span className="text-muted small">{ROTATION_LABELS[type]}</span>
                    <div className="text-end">
                      <div className="fw-semibold small">
                        {item.displayName || item.notes || '—'}
                      </div>
                      {
                        <div className="text-muted" style={{ fontSize: 11 }}>
                          Last: {getLastAssigned(item.positionId, type)}
                        </div>
                      }
                    </div>
                  </div>
                </li>
              )
            })}
          </ul>
        )}
      </div>

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
