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

        {/* content */}
        <div className="px-5 py-6 text-white/80 space-y-6">
          {tab === "windows" && (
            <div className="space-y-4">
              <p>
                Use <code className="font-mono text-white">winget</code> to install the .NET 9 SDK,
                then use <code className="font-mono text-white">dotnet</code> to install the CycoD CLI (and CYCODMD CLI).
              </p>
              <pre className="bg-black/40 border border-white/10 rounded-lg p-4 overflow-x-auto text-sm text-white">
<code>{`winget install --cask dotnet-sdk
dotnet tool install --global CycoD --prerelease
dotnet tool install --global cycodmd --prerelease`}</code>
              </pre>

              <details className="group rounded-lg border border-emerald-400/60 bg-emerald-500/10">
                <summary className="list-none cursor-pointer select-none px-4 py-3 font-semibold text-emerald-300 flex items-center justify-between">
                  <span>If you don't have winget …</span>
                  <svg className="h-4 w-4 text-emerald-300 transition-transform -rotate-90 group-open:rotate-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </summary>
                <div className="px-4 pb-4 text-white/85 space-y-2">
                  <a className="text-emerald-300 underline" href="https://learn.microsoft.com/windows/package-manager/winget/" target="_blank" rel="noreferrer">Install WinGet</a>
                  <p>Walks you through installing the Windows Package Manager.</p>
                </div>
              </details>

              <details className="group rounded-lg border border-emerald-400/60 bg-emerald-500/10">
                <summary className="list-none cursor-pointer select-none px-4 py-3 font-semibold text-emerald-300 flex items-center justify-between">
                  <span>Why do I need the <code className="font-mono">cycodmd</code> CLI?</span>
                  <svg className="h-4 w-4 text-emerald-300 transition-transform -rotate-90 group-open:rotate-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </summary>
                <div className="px-4 pb-4 text-white/85 space-y-3">
                  <p>
                    The <code className="font-mono">cycodmd</code> CLI is used to dynamically generate <code className="font-mono">.md</code> content for LLM context.
                  </p>
                  <div>
                    <p className="mb-1">Slash commands powered by <span className="font-mono">cycodmd</span>:</p>
                    <ul className="list-disc pl-6 space-y-1">
                      <li><code className="font-mono">/file &lt;filename&gt;</code> - Include single file, or part of a file</li>
                      <li><code className="font-mono">/files &lt;pattern&gt;</code> - Include multiple files, or parts of those files</li>
                      <li><code className="font-mono">/search &lt;query&gt;</code> - Include web search results and page content</li>
                      <li><code className="font-mono">/get &lt;url&gt;</code> - Include content from a URL, with or without HTML tags</li>
                      <li><code className="font-mono">/run &lt;command&gt;</code> - Include the output of a command/script</li>
                    </ul>
                  </div>
                  <div>
                    <p className="mb-1">Coming soon:</p>
                    <ul className="list-disc pl-6 space-y-1">
                      <li><code className="font-mono">cycod cycodmd files</code> - list/find files by glob + regex</li>
                      <li><code className="font-mono">cycod cycodmd run</code> - run commands/scripts via Bash, Cmd, or Powershell</li>
                      <li><code className="font-mono">cycod cycodmd web get</code> - retrieve content from URLs, with or w/o HTML tags</li>
                      <li><code className="font-mono">cycod cycodmd web search</code> - search for content via Bing, DuckDuckGo, Google, Yahoo, etc.</li>
                    </ul>
                  </div>
                  <p>
                    You can read more about <span className="font-mono">cycodmd</span> at
                    {" "}
                    <a className="text-emerald-300 underline" href="https://github.com/robch/cycod" target="_blank" rel="noreferrer">https://github.com/robch/cycod</a>.
                  </p>
                </div>
              </details>
            </div>
          )}
          {tab === "mac" && (
            <div className="space-y-4">
              <p>
                Use <code className="font-mono text-white">brew</code> to install the CycoD CLI. The tool will be installed in /opt/homebrew/bin
              </p>
              <pre className="bg-black/40 border border-white/10 rounded-lg p-4 overflow-x-auto text-sm text-white">
<code>{`brew tap robch/cycod
brew install cycod`}</code>
              </pre>

              <details className="group rounded-lg border border-emerald-400/60 bg-emerald-500/10">
                <summary className="list-none cursor-pointer select-none px-4 py-3 font-semibold text-emerald-300 flex items-center justify-between">
                  <span>If you don't have brew …</span>
                  <svg className="h-4 w-4 text-emerald-300 transition-transform -rotate-90 group-open:rotate-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </summary>
                <div className="px-4 pb-4 text-white/85 space-y-2">
                  <a className="text-emerald-300 underline" href="https://brew.sh/" target="_blank" rel="noreferrer">Install Homebrew</a>
                  <p>Walks you through installing Homebrew, a package manager for macOS.</p>
                </div>
              </details>

              <details className="group rounded-lg border border-emerald-400/60 bg-emerald-500/10">
                <summary className="list-none cursor-pointer select-none px-4 py-3 font-semibold text-emerald-300 flex items-center justify-between">
                  <span>Why do I need the <code className="font-mono">cycodmd</code> CLI?</span>
                  <svg className="h-4 w-4 text-emerald-300 transition-transform -rotate-90 group-open:rotate-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </summary>
                <div className="px-4 pb-4 text-white/85 space-y-3">
                  <p>
                    The <code className="font-mono">cycodmd</code> CLI is used to dynamically generate <code className="font-mono">.md</code> content for LLM context.
                  </p>
                  <div>
                    <p className="mb-1">Slash commands powered by <span className="font-mono">cycodmd</span>:</p>
                    <ul className="list-disc pl-6 space-y-1">
                      <li><code className="font-mono">/file &lt;filename&gt;</code> - Include single file, or part of a file</li>
                      <li><code className="font-mono">/files &lt;pattern&gt;</code> - Include multiple files, or parts of those files</li>
                      <li><code className="font-mono">/search &lt;query&gt;</code> - Include web search results and page content</li>
                      <li><code className="font-mono">/get &lt;url&gt;</code> - Include content from a URL, with or without HTML tags</li>
                      <li><code className="font-mono">/run &lt;command&gt;</code> - Include the output of a command/script</li>
                    </ul>
                  </div>
                  <div>
                    <p className="mb-1">Coming soon:</p>
                    <ul className="list-disc pl-6 space-y-1">
                      <li><code className="font-mono">cycod cycodmd files</code> - list/find files by glob + regex</li>
                      <li><code className="font-mono">cycod cycodmd run</code> - run commands/scripts via Bash, Cmd, or Powershell</li>
                      <li><code className="font-mono">cycod cycodmd web get</code> - retrieve content from URLs, with or w/o HTML tags</li>
                      <li><code className="font-mono">cycod cycodmd web search</code> - search for content via Bing, DuckDuckGo, Google, Yahoo, etc.</li>
                    </ul>
                  </div>
                  <p>
                    You can read more about <span className="font-mono">cycodmd</span> at
                    {" "}
                    <a className="text-emerald-300 underline" href="https://github.com/robch/cycod" target="_blank" rel="noreferrer">https://github.com/robch/cycod</a>.
                  </p>
                </div>
              </details>
            </div>
          )}
          {tab === "linux" && (
            <div className="space-y-4">
              <p>
                Use <code className="font-mono text-white">brew</code> to install the CycoD CLI. The tool will be installed in /home/linuxbrew/.linuxbrew/bin
              </p>
              <pre className="bg-black/40 border border-white/10 rounded-lg p-4 overflow-x-auto text-sm text-white">
<code>{`brew tap robch/cycod
brew install cycod`}</code>
              </pre>

              <details className="group rounded-lg border border-emerald-400/60 bg-emerald-500/10">
                <summary className="list-none cursor-pointer select-none px-4 py-3 font-semibold text-emerald-300 flex items-center justify-between">
                  <span>If you don't have brew …</span>
                  <svg className="h-4 w-4 text-emerald-300 transition-transform -rotate-90 group-open:rotate-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </summary>
                <div className="px-4 pb-4 text-white/85 space-y-2">
                  <a className="text-emerald-300 underline" href="https://brew.sh/" target="_blank" rel="noreferrer">Install Homebrew</a>
                  <p>Walks you through installing Homebrew, a package manager for Linux.</p>
                </div>
              </details>

              <details className="group rounded-lg border border-emerald-400/60 bg-emerald-500/10">
                <summary className="list-none cursor-pointer select-none px-4 py-3 font-semibold text-emerald-300 flex items-center justify-between">
                  <span>Why do I need the <code className="font-mono">cycodmd</code> CLI?</span>
                  <svg className="h-4 w-4 text-emerald-300 transition-transform -rotate-90 group-open:rotate-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </summary>
                <div className="px-4 pb-4 text-white/85 space-y-3">
                  <p>
                    The <code className="font-mono">cycodmd</code> CLI is used to dynamically generate <code className="font-mono">.md</code> content for LLM context.
                  </p>
                  <div>
                    <p className="mb-1">Slash commands powered by <span className="font-mono">cycodmd</span>:</p>
                    <ul className="list-disc pl-6 space-y-1">
                      <li><code className="font-mono">/file &lt;filename&gt;</code> - Include single file, or part of a file</li>
                      <li><code className="font-mono">/files &lt;pattern&gt;</code> - Include multiple files, or parts of those files</li>
                      <li><code className="font-mono">/search &lt;query&gt;</code> - Include web search results and page content</li>
                      <li><code className="font-mono">/get &lt;url&gt;</code> - Include content from a URL, with or without HTML tags</li>
                      <li><code className="font-mono">/run &lt;command&gt;</code> - Include the output of a command/script</li>
                    </ul>
                  </div>
                  <div>
                    <p className="mb-1">Coming soon:</p>
                    <ul className="list-disc pl-6 space-y-1">
                      <li><code className="font-mono">cycod cycodmd files</code> - list/find files by glob + regex</li>
                      <li><code className="font-mono">cycod cycodmd run</code> - run commands/scripts via Bash, Cmd, or Powershell</li>
                      <li><code className="font-mono">cycod cycodmd web get</code> - retrieve content from URLs, with or w/o HTML tags</li>
                      <li><code className="font-mono">cycod cycodmd web search</code> - search for content via Bing, DuckDuckGo, Google, Yahoo, etc.</li>
                    </ul>
                  </div>
                  <p>
                    You can read more about <span className="font-mono">cycodmd</span> at
                    {" "}
                    <a className="text-emerald-300 underline" href="https://github.com/robch/cycod" target="_blank" rel="noreferrer">https://github.com/robch/cycod</a>.
                  </p>
                </div>
              </details>
            </div>
          )}
          {tab === "source" && (
            <div className="space-y-4">
              <p>Clone the repository and build CycoD from source:</p>
              <pre className="bg-black/40 border border-white/10 rounded-lg p-4 overflow-x-auto text-sm text-white">
<code>{`# Clone the repository\ngit clone https://github.com/robch/cycod.git\ncd cycod\n\n# Build the project\ndotnet build\n\n# Run the application\ndotnet run\n\n# Update your PATH to include the cycod executable\n# ...`}</code>
              </pre>

              <details className="group rounded-lg border border-amber-400/60 bg-amber-500/10">
                <summary className="list-none cursor-pointer select-none px-4 py-3 font-semibold text-amber-300 flex items-center justify-between">
                  <span><span className="font-mono">cycod</span> requires the <span className="font-mono">cycodmd</span> CLI</span>
                  <svg className="h-4 w-4 text-amber-300 transition-transform -rotate-90 group-open:rotate-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </summary>
                <div className="px-4 pb-4 text-white/85 space-y-3">
                  <p>
                    The <code className="font-mono">cycodmd</code> CLI is used to dynamically generate <code className="font-mono">.md</code> content for LLM context.
                  </p>
                  <div>
                    <p className="mb-1">Slash commands powered by <span className="font-mono">cycodmd</span>:</p>
                    <ul className="list-disc pl-6 space-y-1">
                      <li><code className="font-mono">/file &lt;filename&gt;</code> - Include single file, or part of a file</li>
                      <li><code className="font-mono">/files &lt;pattern&gt;</code> - Include multiple files, or parts of those files</li>
                      <li><code className="font-mono">/search &lt;query&gt;</code> - Include web search results and page content</li>
                      <li><code className="font-mono">/get &lt;url&gt;</code> - Include content from a URL, with or without HTML tags</li>
                      <li><code className="font-mono">/run &lt;command&gt;</code> - Include the output of a command/script</li>
                    </ul>
                  </div>
                  <p>
                    In the future, <span className="font-mono">cycodmd</span> functionality will be integrated directly into the <span className="font-mono">cycod</span> CLI.
                  </p>
                  <p>
                    You can read more about <span className="font-mono">cycodmd</span> at <a className="text-amber-300 underline" href="https://github.com/robch/cycod" target="_blank" rel="noreferrer">https://github.com/robch/cycod</a>.
                  </p>
                </div>
              </details>

              <details className="group rounded-lg border border-emerald-400/60 bg-emerald-500/10">
                <summary className="list-none cursor-pointer select-none px-4 py-3 font-semibold text-emerald-300 flex items-center justify-between">
                  <span>How do I install or build the <span className="font-mono">cycodmd</span> CLI</span>
                  <svg className="h-4 w-4 text-emerald-300 transition-transform -rotate-90 group-open:rotate-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </summary>
                <div className="px-4 pb-4 text-white/85 space-y-3">
                  <p>
                    CycoD requires the <span className="font-mono">cycodmd</span> CLI to run. You can either install it from NuGet or build it from source.
                  </p>
                  <div>
                    <p className="mb-1 font-semibold">Install <span className="font-mono">cycodmd</span> CLI</p>
                    <pre className="bg-black/40 border border-white/10 rounded-lg p-3 overflow-x-auto text-sm text-white">
<code>{`dotnet tool install -g cycodmd --prerelease`}</code>
                    </pre>
                  </div>
                  <div>
                    <p className="mb-1">After installation, you can verify that CYCODMD is available by running:</p>
                    <pre className="bg-black/40 border border-white/10 rounded-lg p-3 overflow-x-auto text-sm text-white">
<code>{`cycodmd --version`}</code>
                    </pre>
                    <p>You should see output showing the <span className="font-mono">cycodmd</span> version, for example:</p>
                    <pre className="bg-black/40 border border-white/10 rounded-lg p-3 overflow-x-auto text-sm text-white">
<code>{`Version: 1.0.0-alpha-20250401.2`}</code>
                    </pre>
                  </div>
                </div>
              </details>
            </div>
          )}
        </div>
      </div>
    </div>,
    document.body
  )
}


