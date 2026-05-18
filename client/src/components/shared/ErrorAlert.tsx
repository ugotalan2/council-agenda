interface ErrorAlertProps {
  message: string
  onRetry?: () => void
}

export default function ErrorAlert({ message, onRetry }: ErrorAlertProps) {
  return (
    <div className="alert alert-danger d-flex justify-content-between align-items-center">
      <span>{message}</span>
      {onRetry && (
        <button className="btn btn-sm btn-outline-danger" onClick={onRetry}>
          Retry
        </button>
      )}
    </div>
  )
}
