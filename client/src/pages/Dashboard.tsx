import { UserButton, useAuth } from '@clerk/clerk-react'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api, { setAuthToken } from '../lib/api'

interface Organization {
  id: string
  name: string
  orgType: string
  conductingRotates: boolean
  role: string
}

interface OrgAccessSummary {
  orgId: string
  orgName: string
  role: string
}

interface Person {
  clerkUserId: string
  email: string
  name: string | null
  organizations: OrgAccessSummary[]
}

interface PendingInvitation {
  id: string
  email: string
  status: string
  createdAt: string
  organizations: { organizationId: string; role: string }[]
}

const ORG_TYPES: Record<string, string> = {
  ward_council: 'Ward Council',
  bishopric: 'Bishopric Meeting',
  ward_youth_council: 'Ward Youth Council',
  family_council: 'Family Council',
  presidency_meeting: 'Presidency Meeting'
}

export default function Dashboard() {
  const [orgs, setOrgs] = useState<Organization[]>([])
  const [people, setPeople] = useState<Person[]>([])
  const [pendingInvites, setPendingInvites] = useState<PendingInvitation[]>([])
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<'orgs' | 'people'>('orgs')
  const [showCreate, setShowCreate] = useState(false)
  const [showInvite, setShowInvite] = useState(false)
  const [newOrgName, setNewOrgName] = useState('')
  const [newOrgType, setNewOrgType] = useState('ward_council')
  const [creating, setCreating] = useState(false)
  const [inviteEmail, setInviteEmail] = useState('')
  const [inviteOrgAssignments, setInviteOrgAssignments] = useState<Record<string, string>>({})
  const [inviting, setInviting] = useState(false)
  const { getToken, isLoaded } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (!isLoaded) return
    const fetchAll = async () => {
      const token = await getToken()
      setAuthToken(token)
      try {
        const [orgsRes, peopleRes, invitesRes] = await Promise.all([
          api.get('/api/v1/organizations'),
          api.get('/api/v1/people'),
          api.get('/api/v1/invitations')
        ])
        setOrgs(orgsRes.data)
        setPeople(peopleRes.data)
        setPendingInvites(invitesRes.data)
      } finally {
        setLoading(false)
      }
    }
    fetchAll()
  }, [isLoaded, getToken])

  const createOrg = async () => {
    if (!newOrgName.trim()) return
    setCreating(true)
    const conductingRotates = ['ward_youth_council', 'family_council'].includes(newOrgType)
    try {
      const res = await api.post('/api/v1/organizations', {
        name: newOrgName,
        orgType: newOrgType,
        conductingRotates
      })
      setOrgs(prev => [...prev, res.data])
      setNewOrgName('')
      setShowCreate(false)
    } finally {
      setCreating(false)
    }
  }

  const invitePerson = async () => {
    if (!inviteEmail.trim()) return
    const assignments = Object.entries(inviteOrgAssignments)
      .filter(([, role]) => role !== '')
      .map(([orgId, role]) => ({ organizationId: orgId, role }))

    if (assignments.length === 0) return

    setInviting(true)
    try {
      await api.post('/api/v1/invitations', {
        email: inviteEmail,
        organizations: assignments
      })
      setInviteEmail('')
      setInviteOrgAssignments({})
      setShowInvite(false)
      const invitesRes = await api.get('/api/v1/invitations')
      setPendingInvites(invitesRes.data)
    } finally {
      setInviting(false)
    }
  }

  const adminOrgs = orgs.filter(o => o.role === 'admin')

  return (
    <div className="min-vh-100 bg-light">
      <nav className="navbar bg-white border-bottom sticky-top px-3">
        <div className="container-fluid">
          <span className="navbar-brand fw-bold">Council Agenda</span>
          <UserButton />
        </div>
      </nav>

      <div className="container py-4">
        <ul className="nav nav-tabs mb-4">
          <li className="nav-item">
            <button
              className={`nav-link ${activeTab === 'orgs' ? 'active' : ''}`}
              onClick={() => setActiveTab('orgs')}
            >
              My Organizations
            </button>
          </li>
          <li className="nav-item">
            <button
              className={`nav-link ${activeTab === 'people' ? 'active' : ''}`}
              onClick={() => setActiveTab('people')}
            >
              People
              {pendingInvites.length > 0 && (
                <span className="badge bg-warning text-dark ms-2">{pendingInvites.length}</span>
              )}
            </button>
          </li>
        </ul>

        {/* Organizations Tab */}
        {activeTab === 'orgs' && (
          <div>
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="mb-0">Organizations</h5>
              <button
                className="btn btn-primary btn-sm"
                onClick={() => setShowCreate(!showCreate)}
              >
                + New Organization
              </button>
            </div>

            {showCreate && (
              <div className="card mb-4">
                <div className="card-body">
                  <h6 className="card-title">Create Organization</h6>
                  <div className="mb-3">
                    <label className="form-label small">Name</label>
                    <input
                      className="form-control"
                      value={newOrgName}
                      onChange={e => setNewOrgName(e.target.value)}
                      placeholder="e.g. SCV Ward Council"
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label small">Type</label>
                    <select
                      className="form-select"
                      value={newOrgType}
                      onChange={e => setNewOrgType(e.target.value)}
                    >
                      {Object.entries(ORG_TYPES).map(([value, label]) => (
                        <option key={value} value={value}>{label}</option>
                      ))}
                    </select>
                  </div>
                  <div className="d-flex gap-2">
                    <button className="btn btn-primary btn-sm" onClick={createOrg} disabled={creating}>
                      {creating ? 'Creating...' : 'Create'}
                    </button>
                    <button className="btn btn-outline-secondary btn-sm" onClick={() => setShowCreate(false)}>
                      Cancel
                    </button>
                  </div>
                </div>
              </div>
            )}

            {loading && <div className="text-center py-4"><div className="spinner-border text-primary" /></div>}

            {!loading && orgs.length === 0 && (
              <div className="alert alert-info">No organizations yet. Create one to get started.</div>
            )}

            {orgs.map(org => (
              <div
                key={org.id}
                className="card mb-3"
                onClick={() => navigate(`/organizations/${org.id}`)}
                style={{ cursor: 'pointer' }}
              >
                <div className="card-body d-flex justify-content-between align-items-center">
                  <div>
                    <h5 className="card-title mb-1">{org.name}</h5>
                    <p className="card-text text-muted small mb-0" style={{ paddingLeft: '0.75rem' }}>
                      {ORG_TYPES[org.orgType] ?? org.orgType}
                    </p>
                  </div>
                  <span className="badge bg-secondary">{org.role}</span>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* People Tab */}
        {activeTab === 'people' && (
          <div>
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="mb-0">People</h5>
              <button
                className="btn btn-primary btn-sm"
                onClick={() => setShowInvite(!showInvite)}
              >
                + Invite Person
              </button>
            </div>

            {showInvite && (
              <div className="card mb-4">
                <div className="card-body">
                  <h6 className="card-title">Invite Person</h6>
                  <div className="mb-3">
                    <label className="form-label small">Email address</label>
                    <input
                      className="form-control"
                      type="email"
                      value={inviteEmail}
                      onChange={e => setInviteEmail(e.target.value)}
                      placeholder="email@example.com"
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label small">Assign to organizations</label>
                    {adminOrgs.length === 0 && (
                      <p className="text-muted small">You need to be an admin of at least one organization.</p>
                    )}
                    {adminOrgs.map(org => (
                      <div key={org.id} className="d-flex align-items-center gap-3 mb-2">
                        <span className="small" style={{ minWidth: 200 }}>{org.name}</span>
                        <select
                          className="form-select form-select-sm"
                          style={{ width: 'auto' }}
                          value={inviteOrgAssignments[org.id] ?? ''}
                          onChange={e => setInviteOrgAssignments(prev => ({
                            ...prev,
                            [org.id]: e.target.value
                          }))}
                        >
                          <option value="">No access</option>
                          <option value="viewer">Viewer</option>
                          <option value="editor">Editor</option>
                          <option value="admin">Admin</option>
                        </select>
                      </div>
                    ))}
                  </div>
                  <div className="d-flex gap-2">
                    <button
                      className="btn btn-primary btn-sm"
                      onClick={invitePerson}
                      disabled={inviting}
                    >
                      {inviting ? 'Sending...' : 'Send Invite'}
                    </button>
                    <button
                      className="btn btn-outline-secondary btn-sm"
                      onClick={() => setShowInvite(false)}
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              </div>
            )}

            {/* Pending invitations */}
            {pendingInvites.map(invite => (
              <div key={invite.id} className="card mb-2">
                <div className="card-body py-2 d-flex justify-content-between align-items-center">
                  <div>
                    <div className="fw-semibold">{invite.email}</div>
                    <small className="text-muted">
                      Invited {new Date(invite.createdAt).toLocaleDateString()}
                    </small>
                  </div>
                  <div className="d-flex align-items-center gap-2">
                    <span className="badge bg-warning text-dark">Pending</span>
                    <button
                      className="btn btn-outline-success btn-sm"
                      onClick={async (e) => {
                        e.stopPropagation()
                        try {
                          await api.post(`/api/v1/invitations/${invite.id}/sync`)
                          setPendingInvites(prev => prev.filter(i => i.id !== invite.id))
                          const peopleRes = await api.get('/api/v1/people')
                          setPeople(peopleRes.data)
                        } catch {
                          // silently fail
                        }
                      }}
                    >
                      Sync
                    </button>
                    <button
                      className="btn btn-outline-danger btn-sm"
                      onClick={async (e) => {
                        e.stopPropagation()
                        await api.delete(`/api/v1/invitations/${invite.id}`)
                        setPendingInvites(prev => prev.filter(i => i.id !== invite.id))
                      }}
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              </div>
            ))}

            {/* Active people */}
            {people.length === 0 && pendingInvites.length === 0 && (
              <div className="alert alert-info">
                No people yet. Invite someone to get started.
              </div>
            )}

            {people.map(person => (
              <div key={person.clerkUserId} className="card mb-3">
                <div className="card-body">
                  {/* Header */}
                  <div className="d-flex justify-content-between align-items-center mb-3">
                    <div>
                      <div className="fw-semibold fs-6">{person.name ?? 'Unknown'}</div>
                      <small className="text-muted">{person.email}</small>
                    </div>
                    {/* Add to org button */}
                    {adminOrgs.filter(o => !person.organizations.some(po => po.orgId === o.id)).length > 0 && (
                      <div className="dropdown">
                        <button
                          className="btn btn-outline-primary btn-sm dropdown-toggle"
                          data-bs-toggle="dropdown"
                        >
                          + Add to org
                        </button>
                        <ul className="dropdown-menu dropdown-menu-end">
                          {adminOrgs
                            .filter(o => !person.organizations.some(po => po.orgId === o.id))
                            .map(org => (
                              <li key={org.id}>
                                <button
                                  className="dropdown-item"
                                  onClick={async () => {
                                    await api.put(`/api/v1/people/${person.clerkUserId}/access`, [
                                      { organizationId: org.id, role: 'viewer' }
                                    ])
                                    setPeople(prev => prev.map(p =>
                                      p.clerkUserId === person.clerkUserId
                                        ? {
                                            ...p,
                                            organizations: [...p.organizations, {
                                              orgId: org.id,
                                              orgName: org.name,
                                              role: 'viewer'
                                            }]
                                          }
                                        : p
                                    ))
                                  }}
                                >
                                  {org.name}
                                </button>
                              </li>
                            ))}
                        </ul>
                      </div>
                    )}
                  </div>

                  {/* Org access rows */}
                  <div className="border rounded p-2">
                    {person.organizations.map((org, idx) => (
                      <div
                        key={org.orgId}
                        className={`d-flex align-items-center gap-2 py-1 ${idx < person.organizations.length - 1 ? 'border-bottom' : ''}`}
                      >
                        <span className="small flex-grow-1 text-truncate">{org.orgName}</span>
                        <select
                          className="form-select form-select-sm"
                          style={{ width: 110 }}
                          value={org.role}
                          onChange={async (e) => {
                            const newRole = e.target.value
                            const targetClerkUserId = person.clerkUserId
                            const targetOrgId = org.orgId
                            await api.put(`/api/v1/people/${person.clerkUserId}/access`, [
                              { organizationId: org.orgId, role: newRole }
                            ])
                            setPeople(prev => {
                              const updated = prev.map(p => {
                                if (p.clerkUserId !== targetClerkUserId) return p
                                return {
                                    ...p,
                                    organizations: p.organizations.map(o => {
                                      if (o.orgId !== targetOrgId) return o
                                      return { ...o, role: newRole }
                                    })
                                  }
                              })
                              return [...updated]
                            })
                          }}
                        >
                          <option value="viewer">Viewer</option>
                          <option value="editor">Editor</option>
                          <option value="admin">Admin</option>
                        </select>
                        <button
                          className="btn btn-link btn-sm text-danger p-0"
                          onClick={async () => {
                            await api.delete(`/api/v1/people/${person.clerkUserId}/organizations/${org.orgId}`)
                            setPeople(prev => prev.map(p =>
                              p.clerkUserId === person.clerkUserId
                                ? { ...p, organizations: p.organizations.filter(o => o.orgId !== org.orgId) }
                                : p
                            ).filter(p => p.organizations.length > 0))
                          }}
                        >
                          ✕
                        </button>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}