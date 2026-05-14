import { SignedIn, SignedOut, RedirectToSignIn, useAuth } from '@clerk/clerk-react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { useEffect } from 'react'
import { setAuthToken } from './lib/api'
import Dashboard from './pages/Dashboard'
import OrgLandingPage from './pages/OrgLandingPage'
import AgendaEditorPage from './pages/AgendaEditorPage'

function AuthSync() {
  const { getToken } = useAuth()

  useEffect(() => {
    const syncToken = async () => {
      const token = await getToken()
      setAuthToken(token)
    }
    syncToken()
    const interval = setInterval(syncToken, 60000)
    return () => clearInterval(interval)
  }, [getToken])

  return null
}

export default function App() {
  return (
    <>
      <SignedIn>
        <AuthSync />
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/organizations/:orgId" element={<OrgLandingPage />} />
          <Route path="/organizations/:orgId/meetings/:meetingId" element={<AgendaEditorPage />} />
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </SignedIn>
      <SignedOut>
        <RedirectToSignIn />
      </SignedOut>
    </>
  )
}