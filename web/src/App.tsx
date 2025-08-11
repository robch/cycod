import { Button } from "@/components/ui/button"
import { ChevronDown } from "lucide-react"
import StarfieldBackground from "./components/StarfieldBackground"

function App() {
  return (
    <StarfieldBackground>
    <div className="min-h-screen text-white flex items-center justify-center">
      <div className="text-center space-y-8">
        <div className="space-y-4">
          <h1 className="text-6xl font-bold">
            <span className="text-green-500">&gt;_</span> Cyco<span className="text-green-500">Dev</span>
          </h1>
          <p className="text-xl text-gray-300 max-w-3xl mx-auto">
            It's a toolset, a mindset, a culture, and a{" "}
            <span className="border-b-2 border-green-500">movement.</span>
          </p>
        </div>
        
        <div className="space-y-6">
          <div className="text-lg text-gray-400 font-mono">
            <span>Freedom</span>
            <span className="text-gray-600 mx-4">→</span>
            <span>Control</span>
            <span className="text-gray-600 mx-4">→</span>
            <span>Collaboration</span>
            <span className="text-gray-600 mx-4">→</span>
            <span className="text-green-500">Flow</span>
          </div>
          
          <Button 
            className="bg-green-500 hover:bg-green-600 text-black font-semibold px-8 py-6 text-lg rounded-lg"
          >
            Join the Movement
            <ChevronDown className="ml-2 h-5 w-5" />
          </Button>
        </div>
      </div>
      </div>
    </StarfieldBackground>
  )
}

export default App
