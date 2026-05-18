import { SignedIn, SignedOut, RedirectToSignIn, useAuth } from '@clerk/clerk-react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { setAuthToken } from './lib/api'
import Dashboard from './pages/Dashboard'
import OrgLandingPage from './pages/OrgLandingPage'
import AgendaEditorPage from './pages/AgendaEditorPage'

function AuthSync({ onReady }: { onReady: () => void }) {
  const { getToken } = useAuth()

  useEffect(() => {
    const syncToken = async () => {
      const token = await getToken()
      setAuthToken(token)
      onReady()
    }
    syncToken()
    const interval = setInterval(syncToken, 60000)
    return () => clearInterval(interval)
  }, [onReady, getToken])

  return null
}

export default function App() {
  const [authReady, setAuthReady] = useState(false)

  return (
    <>
      <SignedIn>
        <AuthSync onReady={() => setAuthReady(true)} />
        {authReady && (
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/organizations/:orgId" element={<OrgLandingPage />} />
            <Route
              path="/organizations/:orgId/meetings/:meetingId"
              element={<AgendaEditorPage />}
            />
            <Route path="*" element={<Navigate to="/" />} />
          </Routes>
        )}
      </SignedIn>
      <SignedOut>
        <RedirectToSignIn />
      </SignedOut>
    </>
  )
}
