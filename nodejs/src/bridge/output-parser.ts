export enum OutputState {
  WaitingForPrompt,
  ReceivingResponse,
  InCodeBlock,
  InProgress,
  InError
}

export interface ParsedOutput {
  type: 'prompt' | 'response' | 'code' | 'streaming' | 'error' | 'progress';
  content: string;
  metadata?: {
    language?: string;
    percentage?: number;
  };
}

export class OutputParser {
  private state = OutputState.WaitingForPrompt;
  private currentBlock = '';
  private blockType = '';
  private codeLanguage = '';
  private codeBuffer = '';
  
  parse(chunk: string): ParsedOutput[] {
    const results: ParsedOutput[] = [];
    const lines = chunk.split('\n');
    
    for (const line of lines) {
      // Handle code blocks
      if (line.trim().startsWith('```')) {
        if (this.state !== OutputState.InCodeBlock) {
          // Starting code block
          this.codeLanguage = line.trim().slice(3) || 'text';
          this.state = OutputState.InCodeBlock;
          this.codeBuffer = '';
        } else {
          // Ending code block
          results.push({
            type: 'code',
            content: this.codeBuffer,
            metadata: { language: this.codeLanguage }
          });
          this.state = OutputState.ReceivingResponse;
          this.codeBuffer = '';
        }
        continue;
      }
      
      if (this.state === OutputState.InCodeBlock) {
        this.codeBuffer += line + '\n';
        continue;
      }
      
      // Detect prompts
      if (line.endsWith('> ') || line.endsWith(': ')) {
        results.push({ type: 'prompt', content: line });
        this.state = OutputState.WaitingForPrompt;
      }
      // Detect error patterns
      else if (line.includes('Error:') || line.includes('Exception:')) {
        results.push({ type: 'error', content: line });
        this.state = OutputState.InError;
      }
      // Detect progress bars
      else if (this.detectProgressBar(line)) {
        const percentage = this.extractPercentage(line);
        results.push({
          type: 'progress',
          content: line,
          metadata: { percentage }
        });
      }
      // Detect streaming indicators
      else if (this.detectStreamingPattern(line)) {
        results.push({ type: 'streaming', content: line });
      }
      // Regular response
      else if (line.trim()) {
        results.push({ type: 'response', content: line });
        this.state = OutputState.ReceivingResponse;
      }
    }
    
    return results;
  }
  
  private detectStreamingPattern(text: string): boolean {
    return /\.\.\.$/.test(text) || 
           text.includes('Processing') ||
           text.includes('Thinking') ||
           text.includes('Loading');
  }
  
  private detectProgressBar(text: string): boolean {
    return /\[[\=\-\s]+\]/.test(text) || 
           /\d+%/.test(text);
  }
  
  private extractPercentage(text: string): number {
    const match = text.match(/(\d+)%/);
    return match ? parseInt(match[1]) : 0;
  }
  
  reset(): void {
    this.state = OutputState.WaitingForPrompt;
    this.currentBlock = '';
    this.codeBuffer = '';
    this.codeLanguage = '';
  }
}
