/**
 * Known configuration settings and their properties.
 */
export class KnownSettings {
  private static readonly _knownSettings: Map<string, SettingInfo> = new Map([
    // Provider settings
    ['provider', { canonical: 'provider', isSecret: false, description: 'AI provider to use' }],
    
    // GitHub Copilot settings
    ['github.copilot.token', { canonical: 'github.copilot.token', isSecret: true, description: 'GitHub Copilot token' }],
    ['github.copilot.endpoint', { canonical: 'github.copilot.endpoint', isSecret: false, description: 'GitHub Copilot endpoint' }],
    
    // OpenAI settings  
    ['OpenAI.ApiKey', { canonical: 'OpenAI.ApiKey', isSecret: true, description: 'OpenAI API key' }],
    ['OpenAI.Endpoint', { canonical: 'OpenAI.Endpoint', isSecret: false, description: 'OpenAI API endpoint' }],
    ['OpenAI.ChatModelName', { canonical: 'OpenAI.ChatModelName', isSecret: false, description: 'OpenAI chat model name' }],
    
    // Azure OpenAI settings
    ['azure.openai.token', { canonical: 'azure.openai.token', isSecret: true, description: 'Azure OpenAI token' }],
    ['azure.openai.endpoint', { canonical: 'azure.openai.endpoint', isSecret: false, description: 'Azure OpenAI endpoint' }],
    ['azure.openai.deployment', { canonical: 'azure.openai.deployment', isSecret: false, description: 'Azure OpenAI deployment name' }],
    
    // Chat history settings
    ['chat.history.directory', { canonical: 'chat.history.directory', isSecret: false, description: 'Directory for chat history files' }],
    ['chat.history.max.files', { canonical: 'chat.history.max.files', isSecret: false, description: 'Maximum number of chat history files to keep' }],
    
    // System settings
    ['system.prompt.default', { canonical: 'system.prompt.default', isSecret: false, description: 'Default system prompt' }],
    ['system.debug', { canonical: 'system.debug', isSecret: false, description: 'Enable debug output' }],
    ['system.quiet', { canonical: 'system.quiet', isSecret: false, description: 'Suppress verbose output' }],
    
    // Timeout settings
    ['timeout.request', { canonical: 'timeout.request', isSecret: false, description: 'Request timeout in seconds' }],
    ['timeout.response', { canonical: 'timeout.response', isSecret: false, description: 'Response timeout in seconds' }],
  ]);

  private static readonly _aliases: Map<string, string> = new Map([
    // Provider aliases
    ['ai.provider', 'provider'],
    ['ai_provider', 'provider'],
    
    // GitHub aliases
    ['github_copilot_token', 'github.copilot.token'],
    ['copilot.token', 'github.copilot.token'],
    ['copilot_token', 'github.copilot.token'],
    
    // OpenAI aliases
    ['openai_token', 'openai.token'],
    ['openai_api_key', 'openai.token'],
    ['openai.api.key', 'openai.token'],
    ['openai_endpoint', 'openai.endpoint'],
    ['openai_model', 'openai.model'],
    
    // Azure aliases
    ['azure_openai_token', 'azure.openai.token'],
    ['azure_openai_key', 'azure.openai.token'],
    ['azure.openai.key', 'azure.openai.token'],
    ['azure_openai_endpoint', 'azure.openai.endpoint'],
    ['azure_openai_deployment', 'azure.openai.deployment'],
    
    // System aliases
    ['debug', 'system.debug'],
    ['quiet', 'system.quiet'],
  ]);

  /**
   * Checks if a setting is a known configuration setting.
   */
  static isKnown(key: string): boolean {
    const normalized = this.toDotNotation(key);
    // Check with case-insensitive comparison
    for (const [knownKey] of this._knownSettings) {
      if (knownKey.toLowerCase() === normalized.toLowerCase()) return true;
    }
    for (const [aliasKey] of this._aliases) {
      if (aliasKey.toLowerCase() === normalized.toLowerCase()) return true;
    }
    return false;
  }

  /**
   * Gets the canonical form of a setting key.
   */
  static getCanonicalForm(key: string): string {
    const normalized = this.toDotNotation(key);
    
    // Check aliases first with case-insensitive comparison
    for (const [aliasKey, canonicalKey] of this._aliases) {
      if (aliasKey.toLowerCase() === normalized.toLowerCase()) {
        return canonicalKey;
      }
    }
    
    // Check known settings with case-insensitive comparison  
    for (const [knownKey] of this._knownSettings) {
      if (knownKey.toLowerCase() === normalized.toLowerCase()) {
        return knownKey; // Return the original casing from known settings
      }
    }
    
    return key; // Return original if not known
  }

  /**
   * Checks if a setting is a secret (should be masked in display).
   */
  static isSecret(key: string): boolean {
    const canonical = this.getCanonicalForm(key);
    const settingInfo = this._knownSettings.get(canonical);
    return settingInfo?.isSecret ?? false;
  }

  /**
   * Gets the description for a setting.
   */
  static getDescription(key: string): string | undefined {
    const canonical = this.getCanonicalForm(key);
    const settingInfo = this._knownSettings.get(canonical);
    return settingInfo?.description;
  }

  /**
   * Gets all known setting keys.
   */
  static getAllKnownSettings(): string[] {
    return Array.from(this._knownSettings.keys()).sort();
  }

  /**
   * Converts a key to dot notation format.
   */
  static toDotNotation(key: string): string {
    // Keep the original casing but convert underscores to dots
    return key.replace(/_/g, '.');
  }

  /**
   * Converts a key from dot notation to underscore format.
   */
  static toUnderscoreNotation(key: string): string {
    return key.toLowerCase().replace(/\./g, '_');
  }
}

interface SettingInfo {
  canonical: string;
  isSecret: boolean;
  description: string;
}