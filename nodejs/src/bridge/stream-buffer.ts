export class StreamBuffer {
  private buffer = '';
  private timeout: NodeJS.Timeout | null = null;
  private readonly FLUSH_DELAY = 10; // ms - much shorter delay for real-time streaming
  
  constructor(private onFlush: (content: string) => void) {}
  
  append(chunk: string): void {
    this.buffer += chunk;
    
    // Clear existing timeout
    if (this.timeout) {
      clearTimeout(this.timeout);
      this.timeout = null;
    }
    
    // For streaming output from Console.Write, flush immediately for any content
    // but still batch very rapid writes with a short delay
    this.timeout = setTimeout(() => this.flush(), this.FLUSH_DELAY);
  }
  
  
  flush(): void {
    if (this.buffer) {
      this.onFlush(this.buffer);
      this.buffer = '';
    }
    if (this.timeout) {
      clearTimeout(this.timeout);
      this.timeout = null;
    }
  }
  
  clear(): void {
    this.buffer = '';
    if (this.timeout) {
      clearTimeout(this.timeout);
      this.timeout = null;
    }
  }
  
  get length(): number {
    return this.buffer.length;
  }
  
  get isEmpty(): boolean {
    return this.buffer.length === 0;
  }
}
