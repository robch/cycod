// ANSI color codes for terminal output
const Colors = {
  reset: '\x1b[0m',
  bright: '\x1b[1m',
  dim: '\x1b[2m',
  red: '\x1b[31m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  magenta: '\x1b[35m',
  cyan: '\x1b[36m',
  white: '\x1b[37m',
  gray: '\x1b[90m',
  darkgray: '\x1b[90m',
  darkyellow: '\x1b[33m\x1b[2m',
  darkblue: '\x1b[34m\x1b[2m'
} as const;

export type ColorName = 'red' | 'green' | 'yellow' | 'blue' | 'magenta' | 'cyan' | 'white' | 'gray' | 'darkgray' | 'darkyellow' | 'darkblue' | 'reset';

export class ConsoleHelpers {
  private static isQuiet: boolean = false;
  private static isVerbose: boolean = false;

  static setQuiet(quiet: boolean): void {
    this.isQuiet = quiet;
  }

  static setVerbose(verbose: boolean): void {
    this.isVerbose = verbose;
  }

  static isQuietMode(): boolean {
    return this.isQuiet;
  }

  static isVerboseMode(): boolean {
    return this.isVerbose;
  }

  static writeLine(message: string, skipNewline: boolean = false): void {
    if (skipNewline) {
      process.stdout.write(message);
    } else {
      console.log(message);
    }
  }

  // Method that matches C# ConsoleHelpers.Write with color support
  static writeWithColor(message: string, color: ColorName, overrideQuiet: boolean = false): void {
    if (this.isQuiet && !overrideQuiet) {
      return;
    }

    const colorCode = Colors[color] || Colors.reset;
    process.stdout.write(`${colorCode}${message}${Colors.reset}`);
  }

  // Method that matches C# ConsoleHelpers.WriteLine with color support  
  static writeLineWithColor(message: string, color: ColorName, overrideQuiet: boolean = false): void {
    if (this.isQuiet && !overrideQuiet) {
      return;
    }

    const colorCode = Colors[color] || Colors.reset;
    console.log(`${colorCode}${message}${Colors.reset}`);
  }

  // Convenience methods matching C# ConsoleHelpers patterns
  static writeWarning(message: string): void {
    this.writeLineWithColor(message, 'yellow', true);
  }

  static writeError(message: string): void {
    this.writeLineWithColor(message, 'red', true);
  }

  static writeDebug(message: string): void {
    if (this.isVerbose) {
      this.writeLineWithColor(message, 'darkgray', true);
    }
  }

  static writeInfo(message: string): void {
    this.writeLineWithColor(message, 'blue');
  }

  static displayLocationHeader(locationPath: string, scope: string): void {
    this.writeLine(`LOCATION: ${locationPath} (${scope})`);
  }

  static displayConfigValue(key: string, value: any, indent: boolean = true): void {
    const prefix = indent ? '  ' : '';
    
    if (Array.isArray(value)) {
      this.writeLine(`${prefix}${key}:`);
      for (const item of value) {
        this.writeLine(`${indent ? '    ' : ''}- ${item}`);
      }
    } else {
      let displayValue = value;
      
      // Mask API keys (show only first 2 characters)
      if (key.toLowerCase().includes('apikey') && typeof value === 'string' && value.length > 2) {
        displayValue = value.substring(0, 2);
      }
      
      this.writeLine(`${prefix}${key}: ${displayValue}`);
    }
  }

  static displayClearedMessage(key: string): void {
    this.writeLine(`${key}: (cleared)`);
  }

  static displayNotFoundMessage(key: string): void {
    this.writeLine(`${key}: (not found or empty)`);
  }

  static formatYamlList(items: string[]): string {
    if (items.length === 0) {
      return '';
    }
    
    const lines = [`${items[0]} (if this is first item)`];
    for (let i = 1; i < items.length; i++) {
      lines.push(`- ${items[i]}`);
    }
    
    return lines.join('\n');
  }

  static displayYamlContent(data: any, baseKey?: string): void {
    if (!data || typeof data !== 'object') {
      return;
    }

    const displayObject = (obj: any, prefix: string = '') => {
      for (const [key, value] of Object.entries(obj)) {
        const fullKey = prefix ? `${prefix}.${key}` : key;
        
        if (value && typeof value === 'object' && !Array.isArray(value)) {
          // Nested object - recurse
          displayObject(value, fullKey);
        } else {
          // Leaf value - display it
          this.displayConfigValue(fullKey, value, true);
        }
      }
    };

    displayObject(data);
  }
}