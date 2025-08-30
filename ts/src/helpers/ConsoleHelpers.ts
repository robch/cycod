export class ConsoleHelpers {
  static writeLine(message: string, skipNewline: boolean = false): void {
    if (skipNewline) {
      process.stdout.write(message);
    } else {
      console.log(message);
    }
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