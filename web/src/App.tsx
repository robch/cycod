import { useEffect, useState } from "react"
import { ChevronDown, Terminal, Smartphone, Mic, GitBranch, Users, Zap } from "lucide-react"
import StarfieldBackground from "./components/StarfieldBackground"
import Header from "./components/Header"

function App() {
  const [email, setEmail] = useState("")
  const [submitted, setSubmitted] = useState(false)
  const [typed, setTyped] = useState("")
  const headline = "It's a toolset, a mindset, a culture, and a movement."

  useEffect(() => {
    let i = 0
    const id = setInterval(() => {
      if (i < headline.length) {
        setTyped(headline.slice(0, i + 1))
        i += 1
      } else {
        clearInterval(id)
      }
    }, 50)
    return () => clearInterval(id)
  }, [])

  const onSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!email) return
    setSubmitted(true)
  }

  return (
    <StarfieldBackground>
      <Header />

      {/* Hero */}
      <section className="min-h-[64vh] flex flex-col justify-center items-center px-4 relative text-white">
        <div className="max-w-4xl mx-auto text-center space-y-8">
          <div className="flex items-center justify-center space-x-3 mb-8">
            <Terminal className="w-12 h-12 text-green-400" />
            <h1 className="text-4xl md:text-6xl font-mono font-bold tracking-wider">
              Cyco<span className="text-green-400">Dev</span>
            </h1>
          </div>
          <div className="h-20 flex items-center justify-center">
            <h2 className="text-2xl md:text-4xl font-light leading-tight max-w-3xl">
              {typed}
              <span className="animate-pulse text-green-400">|</span>
            </h2>
          </div>
          <p className="text-xl md:text-2xl text-gray-300 font-mono">
            Freedom → Control → Collaboration → <span className="text-green-400">Flow</span>
          </p>
          <div className="pt-8">
            <a href="#join" className="inline-flex items-center px-8 py-4 bg-green-400 text-black font-semibold rounded-lg hover:bg-green-300 transition-all duration-300 transform hover:scale-105">
              Join the Movement
              <ChevronDown className="ml-2 w-5 h-5" />
            </a>
          </div>
        </div>
        <div className="absolute bottom-8 left-1/2 -translate-x-1/2 animate-bounce">
          <ChevronDown className="w-6 h-6 text-gray-400" />
        </div>
      </section>

      {/* $ what_is_cycodev */}
      <section className="py-12 px-4 border-t border-gray-800 text-white">
        <div className="max-w-4xl mx-auto">
          <h3 className="text-3xl md:text-4xl font-mono mb-8 text-center">
            <span className="text-green-400">$</span> what_is_cycodev
          </h3>
          <div className="bg-gray-900/80 rounded-lg p-8 border border-gray-700">
            <p className="text-lg md:text-xl leading-relaxed mb-6">
              CycoDev is a <span className="text-green-400 font-mono">CLI-first</span>,
              <span className="text-blue-400 font-mono"> Git-native</span>,
              <span className="text-purple-400 font-mono"> AI-augmented</span> developer
              toolset that starts where you already live—the terminal.
            </p>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
              <div className="space-y-2">
                <div className="text-green-400 font-mono text-sm">Open Source</div>
                <div className="w-full h-1 bg-green-400 rounded" />
              </div>
              <div className="space-y-2">
                <div className="text-blue-400 font-mono text-sm">BYOK</div>
                <div className="w-full h-1 bg-blue-400 rounded" />
              </div>
              <div className="space-y-2">
                <div className="text-purple-400 font-mono text-sm">IDE Extensions</div>
                <div className="w-full h-1 bg-purple-400 rounded" />
              </div>
              <div className="space-y-2">
                <div className="text-yellow-400 font-mono text-sm">Mobile + Voice</div>
                <div className="w-full h-1 bg-yellow-400 rounded" />
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* # Coming Features */}
      <section className="py-20 px-4 border-t border-gray-800 text-white">
        <div className="max-w-6xl mx-auto">
          <h3 className="text-3xl md:text-4xl font-mono mb-12 text-center">
            <span className="text-green-400">#</span> Coming Features
          </h3>
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            <div className="bg-gray-900/80 rounded-lg p-6 border border-gray-700 hover:border-green-400 transition-all duration-300 group">
              <Terminal className="w-8 h-8 text-green-400 mb-4 group-hover:scale-110 transition-transform" />
              <h4 className="font-mono text-lg mb-2">CLI + IDE</h4>
              <p className="text-gray-400 text-sm">Native terminal integration with your favorite editor</p>
            </div>
            <div className="bg-gray-900/80 rounded-lg p-6 border border-gray-700 hover:border-blue-400 transition-all duration-300 group">
              <Smartphone className="w-8 h-8 text-blue-400 mb-4 group-hover:scale-110 transition-transform" />
              <h4 className="font-mono text-lg mb-2">Mobile</h4>
              <p className="text-gray-400 text-sm">Code review and management on the go</p>
            </div>
            <div className="bg-gray-900/80 rounded-lg p-6 border border-gray-700 hover:border-purple-400 transition-all duration-300 group">
              <Mic className="w-8 h-8 text-purple-400 mb-4 group-hover:scale-110 transition-transform" />
              <h4 className="font-mono text-lg mb-2">Voice</h4>
              <p className="text-gray-400 text-sm">Natural language interface for complex workflows</p>
            </div>
            <div className="bg-gray-900/80 rounded-lg p-6 border border-gray-700 hover:border-yellow-400 transition-all duration-300 group">
              <GitBranch className="w-8 h-8 text-yellow-400 mb-4 group-hover:scale-110 transition-transform" />
              <h4 className="font-mono text-lg mb-2">Agentic</h4>
              <p className="text-gray-400 text-sm">Branching, time-travel, multi-verse workflows</p>
            </div>
          </div>
          <div className="mt-12 text-center">
            <div className="inline-flex items-center space-x-4 px-6 py-4 bg-gray-900/80 rounded-lg border border-gray-700">
              <Zap className="w-5 h-5 text-green-400" />
              <span className="font-mono">Developer-first: No vendor lock-in, full transparency</span>
            </div>
          </div>
        </div>
      </section>

      {/* ? Why Now */}
      <section className="py-20 px-4 border-t border-gray-800 text-white">
        <div className="max-w-4xl mx-auto text-center">
          <h3 className="text-3xl md:text-4xl font-mono mb-8">
            <span className="text-green-400">?</span> Why Now
          </h3>
          <div className="space-y-6 text-lg md:text-xl leading-relaxed">
            <p>
              The intersection of <span className="text-blue-400 font-mono">AI</span> and
              <span className="text-green-400 font-mono"> DevTools</span> is red-hot, but ripe for disruption.
            </p>
            <p className="text-gray-300">While others build yet another IDE or web-based tool, we start where developers already live—</p>
            <p className="font-mono text-2xl text-green-400">the terminal.</p>
          </div>
        </div>
      </section>

      {/* # Join */}
      <section id="join" className="py-20 px-4 border-t border-gray-800 text-white">
        <div className="max-w-2xl mx-auto text-center">
          <Users className="w-16 h-16 text-green-400 mx-auto mb-8" />
          <h3 className="text-3xl md:text-4xl font-mono mb-6">Join the Movement</h3>
          <p className="text-lg text-gray-300 mb-8">Community-first, open source roots. Be among the first to shape the future of development tools.</p>
          {submitted ? (
            <div className="bg-gray-900/80 rounded-lg p-6 border border-green-400 max-w-md mx-auto">
              <div className="text-green-400 font-mono mb-2">✓ Subscribed</div>
              <p className="text-gray-300">You're in! We'll be in touch soon.</p>
            </div>
          ) : (
            <form onSubmit={onSubmit} className="space-y-4">
              <div className="flex flex-col sm:flex-row gap-4 max-w-md mx-auto">
                <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="developer@example.com" required className="flex-1 px-4 py-3 bg-gray-900/80 border border-gray-700 rounded-lg focus:border-green-400 focus:outline-none font-mono" />
                <button type="submit" className="px-6 py-3 bg-green-400 text-black font-semibold rounded-lg hover:bg-green-300 transition-all duration-300 transform hover:scale-105">Join</button>
              </div>
            </form>
          )}
        </div>
      </section>

      <footer className="py-12 px-4 border-t border-gray-800 text-center text-white">
        <div className="max-w-4xl mx-auto">
          <div className="flex items-center justify-center space-x-3 mb-4">
            <Terminal className="w-6 h-6 text-green-400" />
            <span className="font-mono text-lg">CycoDev</span>
          </div>
          <p className="text-gray-500 text-sm font-mono">cyco.ai • Building the future of developer tools</p>
        </div>
      </footer>

    </StarfieldBackground>
  )
}

export default App
