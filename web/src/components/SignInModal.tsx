import { useEffect, useState } from "react"
import { createPortal } from "react-dom"
import { signInWithGooglePopup } from "@/lib/firebase"

type Props = {
  open: boolean
  onClose: () => void
}

function GoogleIcon() {
  return (
    <svg viewBox="0 0 48 48" className="h-5 w-5" aria-hidden>
      <path fill="#EA4335" d="M24 9.5c3.54 0 6 1.54 7.38 2.83l5.02-4.91C33.64 4.09 29.25 2 24 2 14.91 2 7.09 7.58 3.9 15.09l6.54 5.07C12.02 14.2 17.47 9.5 24 9.5z"/>
      <path fill="#4285F4" d="M46.5 24.5c0-1.64-.15-3.2-.43-4.68H24v8.86h12.7c-.55 2.96-2.19 5.47-4.68 7.16l7.14 5.54C43.89 37.12 46.5 31.31 46.5 24.5z"/>
      <path fill="#FBBC05" d="M10.44 28.37A14.492 14.492 0 0 1 9.5 24c0-1.52.26-2.98.73-4.35L3.9 14.09C2.68 16.67 2 19.46 2 22.5s.68 5.83 1.9 8.41l6.54-5.07z"/>
      <path fill="#34A853" d="M24 46c5.85 0 10.77-1.93 14.36-5.26l-7.14-5.54C29.35 36.73 26.88 37.5 24 37.5c-6.53 0-12.04-4.7-13.56-10.66l-6.54 5.07C7.09 40.42 14.91 46 24 46z"/>
    </svg>
  )
}

function MicrosoftIcon() {
  return (
    <svg viewBox="0 0 24 24" className="h-5 w-5" aria-hidden>
      <rect width="10" height="10" x="1" y="1" fill="#F35325"/>
      <rect width="10" height="10" x="13" y="1" fill="#81BC06"/>
      <rect width="10" height="10" x="1" y="13" fill="#05A6F0"/>
      <rect width="10" height="10" x="13" y="13" fill="#FFBA08"/>
    </svg>
  )
}

export default function SignInModal({ open, onClose }: Props) {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === "Escape") onClose()
    }
    if (open) document.addEventListener("keydown", onKey)
    return () => document.removeEventListener("keydown", onKey)
  }, [open, onClose])

  if (!open) return null

  return createPortal(
    <div className="fixed inset-0 z-[70] grid place-items-center">
      <div className="absolute inset-0 bg-black/60" onClick={onClose} />
      <div
        role="dialog"
        aria-modal="true"
        className="relative z-[71] w-[92vw] max-w-md rounded-2xl border border-white/15 bg-white/5 p-0 text-white backdrop-blur-xl shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="relative flex items-center px-5 py-4 border-b border-white/10">
          <h3 className="mx-auto text-lg font-semibold text-center">Sign in or create an account</h3>
          <button
            aria-label="Close"
            className="absolute right-5 top-1/2 -translate-y-1/2 rounded-md px-2 py-1 text-white/80 hover:bg-white/10 hover:text-white"
            onClick={onClose}
          >
            ×
          </button>
        </div>

        <div className="px-6 py-6 space-y-3">
          {error && <div className="rounded-lg border border-red-500/50 bg-red-500/10 px-4 py-2 text-sm text-red-200">{error}</div>}
          <button
            onClick={async () => {
              try {
                setError(null)
                setLoading(true)
                await signInWithGooglePopup()
                // TODO: store user or token in your app state if needed
                onClose()
              } catch (e: any) {
                setError(e?.message ?? "Sign-in failed")
              } finally {
                setLoading(false)
              }
            }}
            disabled={loading}
            className="w-full inline-flex items-center justify-center gap-3 rounded-lg border border-white/20 bg-white/10 px-4 py-3 hover:bg-white/15 transition disabled:opacity-50"
          >
            <GoogleIcon />
            <span className="font-medium">{loading ? "Signing in…" : "Continue with Google"}</span>
          </button>
          <button className="w-full inline-flex items-center justify-center gap-3 rounded-lg border border-white/20 bg-white/10 px-4 py-3 hover:bg-white/15 transition">
            <MicrosoftIcon />
            <span className="font-medium">Continue with Microsoft</span>
          </button>
        </div>
      </div>
    </div>,
    document.body
  )
}


