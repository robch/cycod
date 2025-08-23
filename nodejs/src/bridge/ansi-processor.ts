import stripAnsi from 'strip-ansi';
import ansiRegex from 'ansi-regex';

export interface ProcessedOutput {
  text: string;
  formatting: Formatting[];
  cursorOps: CursorOperation[];
}

export interface Formatting {
  type: 'color' | 'bold' | 'italic' | 'underline';
  value: string;
  position?: number;
}

export interface CursorOperation {
  type: 'move' | 'clear' | 'save' | 'restore';
  value?: { x?: number; y?: number };
}

export class AnsiProcessor {
  process(raw: string): ProcessedOutput {
    // Detect ANSI sequences
    const regex = ansiRegex();
    const sequences = raw.match(regex) || [];
    const cleanText = stripAnsi(raw);
    
    return {
      text: cleanText,
      formatting: this.extractFormatting(sequences),
      cursorOps: this.extractCursorOps(sequences)
    };
  }
  
  private extractFormatting(sequences: string[]): Formatting[] {
    const formatting: Formatting[] = [];
    
    sequences.forEach(seq => {
      // Color codes
      if (seq.includes('[30m')) formatting.push({ type: 'color', value: 'black' });
      if (seq.includes('[31m')) formatting.push({ type: 'color', value: 'red' });
      if (seq.includes('[32m')) formatting.push({ type: 'color', value: 'green' });
      if (seq.includes('[33m')) formatting.push({ type: 'color', value: 'yellow' });
      if (seq.includes('[34m')) formatting.push({ type: 'color', value: 'blue' });
      if (seq.includes('[35m')) formatting.push({ type: 'color', value: 'magenta' });
      if (seq.includes('[36m')) formatting.push({ type: 'color', value: 'cyan' });
      if (seq.includes('[37m')) formatting.push({ type: 'color', value: 'white' });
      
      // Bright colors
      if (seq.includes('[90m')) formatting.push({ type: 'color', value: 'gray' });
      if (seq.includes('[91m')) formatting.push({ type: 'color', value: 'brightRed' });
      if (seq.includes('[92m')) formatting.push({ type: 'color', value: 'brightGreen' });
      if (seq.includes('[93m')) formatting.push({ type: 'color', value: 'brightYellow' });
      if (seq.includes('[94m')) formatting.push({ type: 'color', value: 'brightBlue' });
      if (seq.includes('[95m')) formatting.push({ type: 'color', value: 'brightMagenta' });
      if (seq.includes('[96m')) formatting.push({ type: 'color', value: 'brightCyan' });
      if (seq.includes('[97m')) formatting.push({ type: 'color', value: 'brightWhite' });
      
      // Text styles
      if (seq.includes('[1m')) formatting.push({ type: 'bold', value: 'true' });
      if (seq.includes('[3m')) formatting.push({ type: 'italic', value: 'true' });
      if (seq.includes('[4m')) formatting.push({ type: 'underline', value: 'true' });
    });
    
    return formatting;
  }
  
  private extractCursorOps(sequences: string[]): CursorOperation[] {
    const ops: CursorOperation[] = [];
    
    sequences.forEach(seq => {
      // Cursor movement
      const upMatch = seq.match(/\[(\d+)A/);
      if (upMatch) {
        ops.push({ type: 'move', value: { y: -parseInt(upMatch[1]) } });
      }
      
      const downMatch = seq.match(/\[(\d+)B/);
      if (downMatch) {
        ops.push({ type: 'move', value: { y: parseInt(downMatch[1]) } });
      }
      
      const rightMatch = seq.match(/\[(\d+)C/);
      if (rightMatch) {
        ops.push({ type: 'move', value: { x: parseInt(rightMatch[1]) } });
      }
      
      const leftMatch = seq.match(/\[(\d+)D/);
      if (leftMatch) {
        ops.push({ type: 'move', value: { x: -parseInt(leftMatch[1]) } });
      }
      
      // Clear operations
      if (seq.includes('[2J')) ops.push({ type: 'clear', value: undefined });
      if (seq.includes('[K')) ops.push({ type: 'clear', value: undefined });
      
      // Save/restore cursor
      if (seq.includes('[s')) ops.push({ type: 'save', value: undefined });
      if (seq.includes('[u')) ops.push({ type: 'restore', value: undefined });
    });
    
    return ops;
  }
}
