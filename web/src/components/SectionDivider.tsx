export default function SectionDivider() {
  return (
    <div className="mx-auto my-16 w-full max-w-5xl px-6">
      <div className="relative h-px w-full overflow-visible">
        <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/70 to-transparent" />
        <div className="absolute -inset-x-8 -top-2 h-4 bg-white/10 blur-xl" />
      </div>
    </div>
  );
}


