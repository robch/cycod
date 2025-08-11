/* no explicit React import required with modern JSX */

function Logo() {
  return (
    <a href="#" className="group inline-flex items-center gap-2 select-none">
      <span className="text-green-500 font-mono text-2xl leading-none">&gt;_</span>
      <span className="text-xl font-semibold tracking-tight">
        <span className="text-white">Cyco</span>
        <span className="text-green-500">Dev</span>
      </span>
    </a>
  );
}

import { useEffect, useState } from "react"
import InstallModal from "./InstallModal"
import SignInModal from "./SignInModal"
import { getFirebaseApp } from "@/lib/firebase"
import { getAuth, onAuthStateChanged, signOut, type User } from "firebase/auth"

export default function Header() {
  const [showInstall, setShowInstall] = useState(false)
  const [showSignIn, setShowSignIn] = useState(false)
  const [currentUser, setCurrentUser] = useState<User | null>(null)
  const [avatarFailed, setAvatarFailed] = useState(false)

  useEffect(() => {
    const auth = getAuth(getFirebaseApp())
    const unsub = onAuthStateChanged(auth, (u) => {
      setCurrentUser(u)
      setAvatarFailed(false)
    })
    return () => unsub()
  }, [])
  return (
    <header className="pointer-events-none fixed inset-x-0 top-0 z-50">
      {/* gradient hairline to blend into background */}
      <div className="absolute inset-0 bg-gradient-to-b from-white/10 to-transparent" />

      <div className="mx-auto max-w-6xl px-4 sm:px-6 lg:px-8">
        <div className="pointer-events-auto mt-4 rounded-2xl border border-white/15 bg-white/5 backdrop-blur-xl shadow-[0_4px_30px_rgba(0,0,0,0.25)]">
          {/* Subtle glossy top edge */}
          <div className="h-px w-full rounded-t-2xl bg-gradient-to-r from-white/30 via-white/60 to-white/30/50 opacity-60" />

          <nav className="relative flex items-center justify-between px-5 py-3">
            <div className="flex min-w-0 items-center gap-6">
              <Logo />
            </div>

            {/* Center links with a faint pill highlight behind */}
            <div className="relative hidden md:block">
              <div className="pointer-events-none absolute inset-x-0 -bottom-1 mx-2 h-2 rounded-full bg-white/10 blur-lg" />
              <ul className="flex items-center gap-8 text-sm text-white/90">
                {[
                  { label: "Product", href: "#product" },
                  { label: "Docs", href: "#docs" },
                  { label: "Blog", href: "#blog" },
                ].map((item) => (
                  <li key={item.label}>
                    <a
                      href={item.href}
                      className="relative transition-colors hover:text-white"
                    >
                      {item.label}
                    </a>
                  </li>
                ))}
              </ul>
            </div>

            {/* Right actions */}
            <div className="flex items-center gap-3">
              {currentUser ? (
                <>
                  <div className="flex items-center gap-2 pr-1">
                    {currentUser.photoURL && !avatarFailed ? (
                      <img
                        src={currentUser.photoURL}
                        alt={currentUser.displayName ?? "User"}
                        referrerPolicy="no-referrer"
                        onError={() => setAvatarFailed(true)}
                        className="h-8 w-8 rounded-full border border-white/20 object-cover"
                      />
                    ) : (
                      <div className="h-8 w-8 rounded-full border border-white/20 bg-white/10 grid place-items-center text-white/80 text-sm">
                        {(currentUser.displayName ?? currentUser.email ?? "U").slice(0,1).toUpperCase()}
                      </div>
                    )}
                    <span className="text-sm text-white/90 truncate max-w-[12rem]">{currentUser.displayName ?? currentUser.email ?? "User"}</span>
                  </div>
                  <button
                    type="button"
                    onClick={async () => {
                      try {
                        await signOut(getAuth(getFirebaseApp()))
                      } catch (err) {
                        console.error("Sign out failed", err)
                      }
                    }}
                    className="rounded-lg border border-white/25 bg-white/5 px-4 py-2 text-sm text-white/90 shadow-sm backdrop-blur-md transition hover:bg-white/10 hover:text-white"
                  >
                    Sign out
                  </button>
                </>
              ) : (
                <button
                  type="button"
                  onClick={() => setShowSignIn(true)}
                  className="rounded-lg border border-white/25 bg-white/5 px-4 py-2 text-sm text-white/90 shadow-sm backdrop-blur-md transition hover:bg-white/10 hover:text-white"
                >
                  Sign in
                </button>
              )}
              <button
                type="button"
                onClick={() => setShowInstall(true)}
                className="rounded-lg border border-white/50 bg-white/70 px-4 py-2 text-sm font-medium text-gray-900 shadow-sm transition hover:bg-white"
              >
                Install
              </button>
            </div>
          </nav>
          <InstallModal open={showInstall} onClose={() => setShowInstall(false)} />
          <SignInModal open={showSignIn} onClose={() => setShowSignIn(false)} />
        </div>
      </div>
    </header>
  );
}


