import { useState, useEffect } from 'react'
import { useAuth } from '@clerk/clerk-react'
import api, { setAuthToken } from '../api'

interface Organization {
  id: string
  name: string
  orgType: string
  conductingRotates: boolean
  role: string
}

interface OrganizationSettings {
  meetingDay: string
  meetingTime: string
  frequency: string
  weekOfMonth?: number
  meetingDurationMinutes: number
}

interface Member {
  id: string
  name: string
  calling: string
  active: boolean
  clerkUserId: string
}

export function useOrganization(orgId: string) {
  const [org, setOrg] = useState<Organization | null>(null)
  const [settings] = useState<OrganizationSettings | null>(null)
  const [members, setMembers] = useState<Member[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const { getToken, isLoaded } = useAuth()

  useEffect(() => {
    if (!isLoaded || !orgId) return

    const fetch = async () => {
      try {
        const token = await getToken()
        setAuthToken(token)

        const [orgsRes, membersRes] = await Promise.all([
          api.get('/api/v1/organizations'),
          api.get(`/api/v1/organizations/${orgId}/members`),
        ])

        const found = orgsRes.data.find((o: Organization) => o.id === orgId)
        if (!found) throw new Error('Organization not found')

        setOrg(found)
        setMembers(membersRes.data)
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : 'An error occurred')
      } finally {
        setLoading(false)
      }
    }

    fetch()
  }, [isLoaded, orgId, getToken])

  return { org, settings, members, loading, error, setMembers }
}
