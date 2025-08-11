import React from "react";
import "./aurora.css";

type Props = {
  children?: React.ReactNode;
  className?: string;
};

export default function AuroraBackground({ children, className }: Props) {
  return (
    <div className={`aurora-root ${className ?? ""}`}>
      {/* Rotating conic gradient layer */}
      <div className="aurora-ink" aria-hidden="true" />

      {/* Soft glow “blobs” */}
      <div className="aurora-blob a" aria-hidden="true" />
      <div className="aurora-blob b" aria-hidden="true" />
      <div className="aurora-blob c" aria-hidden="true" />

      {/* Subtle grain for texture */}
      <div className="aurora-grain" aria-hidden="true" />

      {/* Content */}
      <div className="aurora-content">{children}</div>
    </div>
  );
}
