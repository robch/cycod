type Props = {
  children: React.ReactNode
}

export default function TerminalHeading({ children }: Props) {
  return (
    <h2 className="mb-5 flex items-center gap-2 font-mono text-xl text-white">
      <span className="text-green-400">$</span>
      <span className="text-white/90">{children}</span>
    </h2>
  )
}


