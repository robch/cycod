import { useEffect, useState } from "react"
import { createPortal } from "react-dom"

type Props = {
  open: boolean
  onClose: () => void
}

type TabKey = "windows" | "mac" | "linux" | "source"

export default function InstallModal({ open, onClose }: Props) {
  const [tab, setTab] = useState<TabKey>("mac")

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === "Escape") onClose()
    }
    if (open) document.addEventListener("keydown", onKey)
    return () => document.removeEventListener("keydown", onKey)
  }, [open, onClose])

  if (!open) return null

  return createPortal(
    <div className="fixed inset-0 z-[60] grid place-items-center">
      {/* overlay */}
      <div className="absolute inset-0 bg-black/60" onClick={onClose} />

      {/* dialog */}
      <div
        role="dialog"
        aria-modal="true"
        className="relative z-[61] w-[92vw] max-w-xl rounded-2xl border border-white/15 bg-white/5 p-0 text-white backdrop-blur-xl shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        {/* header */}
        <div className="relative flex items-center px-5 py-4 border-b border-white/10">
          <h3 className="mx-auto text-lg font-semibold text-center">Install cycod on your machine</h3>
          <button
            aria-label="Close"
            className="absolute right-5 top-1/2 -translate-y-1/2 rounded-md px-2 py-1 text-white/80 hover:bg-white/10 hover:text-white"
            onClick={onClose}
          >
            ×
          </button>
        </div>

        {/* tabs styled like what_is_cycodev tiles */}
        <div className="px-5 pt-4">
          <div role="tablist" aria-label="Install targets" className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-center">
            {[
              { label: "Windows", key: "windows" as TabKey, text: "text-green-400", bar: "bg-green-400" },
              { label: "MacOS", key: "mac" as TabKey, text: "text-blue-400", bar: "bg-blue-400" },
              { label: "Linux", key: "linux" as TabKey, text: "text-purple-400", bar: "bg-purple-400" },
              { label: "Build Source", key: "source" as TabKey, text: "text-yellow-400", bar: "bg-yellow-400" },
            ].map(({ label, key, text, bar }) => (
              <button
                key={key}
                role="tab"
                aria-selected={tab === key}
                onClick={() => setTab(key)}
                className={`space-y-2 px-2 py-1 transition ${tab === key ? "text-white" : "text-white/80 hover:text-white"}`}
              >
                <div className={`${text} font-mono text-sm`}>{label}</div>
                <div className={`mx-auto h-1 w-full rounded ${bar} ${tab === key ? "opacity-100" : "opacity-50"}`} />
              </button>
            ))}
          </div>
        </div>

        {/* content placeholder */}
        <div className="px-5 py-6 text-white/80">
          {tab === "windows" && <p>Windows install steps coming soon.</p>}
          {tab === "mac" && <p>MacOS install steps coming soon.</p>}
          {tab === "linux" && <p>Linux install steps coming soon.</p>}
          {tab === "source" && <p>Build from source steps coming soon.</p>}
        </div>
      </div>
    </div>,
    document.body
  )
}


