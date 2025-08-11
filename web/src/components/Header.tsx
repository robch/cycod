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

export default function Header() {
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
                  { label: "Pricing", href: "#pricing" },
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
              <a
                href="#signin"
                className="rounded-lg border border-white/25 bg-white/5 px-4 py-2 text-sm text-white/90 shadow-sm backdrop-blur-md transition hover:bg-white/10 hover:text-white"
              >
                Sign in
              </a>
              <a
                href="#install"
                className="rounded-lg border border-white/50 bg-white/70 px-4 py-2 text-sm font-medium text-gray-900 shadow-sm transition hover:bg-white"
              >
                Install
              </a>
            </div>
          </nav>
        </div>
      </div>
    </header>
  );
}


