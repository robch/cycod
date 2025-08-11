import React, { useEffect, useRef } from "react";
import "./starfield.css";

type Props = {
  children?: React.ReactNode;
  className?: string;
};

type Star = {
  x: number;
  y: number;
  radius: number;
  baseAlpha: number;
  phase: number;
  speed: number;
};

export default function StarfieldBackground({ children, className }: Props) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const animationRef = useRef<number | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current!;
    const ctx = canvas.getContext("2d", { alpha: true })!;

    const stars: Star[] = [];

    function resize() {
      const dpr = Math.min(window.devicePixelRatio || 1, 2);
      const width = window.innerWidth;
      const height = window.innerHeight;
      canvas.width = Math.floor(width * dpr);
      canvas.height = Math.floor(height * dpr);
      canvas.style.width = `${width}px`;
      canvas.style.height = `${height}px`;
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

      // Recreate stars on resize to fit space nicely
      stars.length = 0;
      const area = width * height;
      // About 0.00025 stars per pixel → 400–900 typical
      const count = Math.floor(Math.min(1200, Math.max(350, area * 0.00025)));
      for (let i = 0; i < count; i += 1) {
        stars.push({
          x: Math.random() * width,
          y: Math.random() * height,
          radius: Math.random() < 0.85 ? Math.random() * 0.8 + 0.2 : Math.random() * 1.6 + 0.6,
          baseAlpha: Math.random() * 0.7 + 0.2,
          phase: Math.random() * Math.PI * 2,
          speed: 0.6 + Math.random() * 0.9,
        });
      }
    }

    function draw() {
      const { width, height } = canvas;
      // because we scaled by DPR using setTransform, width/height below are CSS pixels
      // clear with transparency to let CSS gradients show through
      ctx.clearRect(0, 0, width, height);

      // Draw stars
      for (const s of stars) {
        const alpha = s.baseAlpha * (0.65 + 0.35 * Math.sin(s.phase));
        ctx.globalAlpha = alpha;
        const gradient = ctx.createRadialGradient(s.x, s.y, 0, s.x, s.y, s.radius * 3);
        gradient.addColorStop(0, "rgba(255,255,255,1)");
        gradient.addColorStop(1, "rgba(255,255,255,0)");
        ctx.fillStyle = gradient;
        ctx.beginPath();
        ctx.arc(s.x, s.y, s.radius * 2.2, 0, Math.PI * 2);
        ctx.fill();

        // small blue tint halo for brighter stars
        if (s.radius > 1.1) {
          ctx.globalAlpha = alpha * 0.35;
          const g2 = ctx.createRadialGradient(s.x, s.y, 0, s.x, s.y, s.radius * 6);
          g2.addColorStop(0, "rgba(170,200,255,0.9)");
          g2.addColorStop(1, "rgba(170,200,255,0)");
          ctx.fillStyle = g2;
          ctx.beginPath();
          ctx.arc(s.x, s.y, s.radius * 5, 0, Math.PI * 2);
          ctx.fill();
        }

        s.phase += 0.005 * s.speed;
      }

      ctx.globalAlpha = 1;
      animationRef.current = requestAnimationFrame(draw);
    }

    resize();
    draw();
    window.addEventListener("resize", resize);
    return () => {
      if (animationRef.current) cancelAnimationFrame(animationRef.current);
      window.removeEventListener("resize", resize);
    };
  }, []);

  return (
    <div className={`starfield-root ${className ?? ""}`}>
      <canvas ref={canvasRef} className="starfield-canvas" aria-hidden="true" />
      {/* Diagonal milky-way glow */}
      <div className="starfield-milkyway" aria-hidden="true" />
      {/* Subtle noise */}
      <div className="starfield-grain" aria-hidden="true" />
      <div className="starfield-content">{children}</div>
    </div>
  );
}


