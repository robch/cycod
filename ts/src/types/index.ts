export interface ChatMessage {
  role: 'system' | 'user' | 'assistant';
  content: string;
}

export interface CommandOptions {
  input?: string | string[];
  inputs?: string[];
  interactive?: boolean;
  quiet?: boolean;
  systemPrompt?: string;
  addSystemPrompt?: string;
  outputChatHistory?: string;
  inputChatHistory?: string;
  chatHistory?: string;
  continue?: boolean;
  vars?: Record<string, string>;
  foreach?: {
    variable: string;
    values: string[];
  };
  threads?: number;
  profile?: string;
}

export interface ConfigurationScope {
  local: Record<string, any>;
  user: Record<string, any>;
  global: Record<string, any>;
}

export enum ConfigFileScope {
  Local = 'local',
  User = 'user', 
  Global = 'global',
  Any = 'any'
}

export interface ConfigValue {
  value: any;
  scope: ConfigFileScope;
  location: string;
}