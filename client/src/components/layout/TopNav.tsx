import { UserButton } from '@clerk/clerk-react'
import { useNavigate } from 'react-router-dom'

interface TopNavProps {
  title: string
  subtitle?: string
  backTo?: string
  backLabel?: string
}

export default function TopNav({ title, subtitle, backTo, backLabel }: TopNavProps) {
  const navigate = useNavigate()

  return (
    <nav className="navbar navbar-light bg-white border-bottom sticky-top">
      <div className="container-fluid px-3">
        <div className="d-flex align-items-center gap-2">
          {backTo && (
            <button
              className="btn btn-link p-0 text-secondary me-1"
              onClick={() => navigate(backTo)}
              style={{ textDecoration: 'none' }}
            >
              ← {backLabel ?? 'Back'}
            </button>
          )}
          <div>
            <span className="navbar-brand mb-0 fw-semibold">{title}</span>
            {subtitle && (
              <span className="text-muted small ms-2">{subtitle}</span>
            )}
          </div>
        </div>
        <UserButton />
      </div>
    </nav>
  )
}