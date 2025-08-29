import { KnownSettings } from '../../Configuration/KnownSettings';

describe('KnownSettings', () => {
  describe('Known Settings Detection', () => {
    test('should recognize known settings', () => {
      expect(KnownSettings.isKnown('provider')).toBe(true);
      expect(KnownSettings.isKnown('openai.token')).toBe(true);
      expect(KnownSettings.isKnown('github.copilot.token')).toBe(true);
      expect(KnownSettings.isKnown('azure.openai.endpoint')).toBe(true);
    });

    test('should recognize aliases', () => {
      expect(KnownSettings.isKnown('openai_token')).toBe(true);
      expect(KnownSettings.isKnown('copilot_token')).toBe(true);
      expect(KnownSettings.isKnown('debug')).toBe(true);
      expect(KnownSettings.isKnown('quiet')).toBe(true);
    });

    test('should reject unknown settings', () => {
      expect(KnownSettings.isKnown('unknown.setting')).toBe(false);
      expect(KnownSettings.isKnown('random_key')).toBe(false);
    });
  });

  describe('Canonical Form', () => {
    test('should return canonical form for known settings', () => {
      expect(KnownSettings.getCanonicalForm('provider')).toBe('provider');
      expect(KnownSettings.getCanonicalForm('openai.token')).toBe('openai.token');
    });

    test('should convert aliases to canonical form', () => {
      expect(KnownSettings.getCanonicalForm('openai_token')).toBe('openai.token');
      expect(KnownSettings.getCanonicalForm('copilot_token')).toBe('github.copilot.token');
      expect(KnownSettings.getCanonicalForm('debug')).toBe('system.debug');
      expect(KnownSettings.getCanonicalForm('quiet')).toBe('system.quiet');
    });

    test('should return original key for unknown settings', () => {
      expect(KnownSettings.getCanonicalForm('unknown.setting')).toBe('unknown.setting');
    });
  });

  describe('Secret Detection', () => {
    test('should identify secret settings', () => {
      expect(KnownSettings.isSecret('openai.token')).toBe(true);
      expect(KnownSettings.isSecret('github.copilot.token')).toBe(true);
      expect(KnownSettings.isSecret('azure.openai.token')).toBe(true);
    });

    test('should identify non-secret settings', () => {
      expect(KnownSettings.isSecret('provider')).toBe(false);
      expect(KnownSettings.isSecret('openai.endpoint')).toBe(false);
      expect(KnownSettings.isSecret('system.debug')).toBe(false);
    });

    test('should handle aliases for secret detection', () => {
      expect(KnownSettings.isSecret('openai_token')).toBe(true);
      expect(KnownSettings.isSecret('copilot_token')).toBe(true);
    });
  });

  describe('Dot Notation Conversion', () => {
    test('should convert underscores to dots', () => {
      expect(KnownSettings.toDotNotation('openai_token')).toBe('openai.token');
      expect(KnownSettings.toDotNotation('azure_openai_endpoint')).toBe('azure.openai.endpoint');
      expect(KnownSettings.toDotNotation('system_debug')).toBe('system.debug');
    });

    test('should leave dots unchanged', () => {
      expect(KnownSettings.toDotNotation('openai.token')).toBe('openai.token');
      expect(KnownSettings.toDotNotation('github.copilot.token')).toBe('github.copilot.token');
    });

    test('should convert to lowercase', () => {
      expect(KnownSettings.toDotNotation('OPENAI_TOKEN')).toBe('openai.token');
      expect(KnownSettings.toDotNotation('GitHub_Copilot_Token')).toBe('github.copilot.token');
    });
  });

  describe('Underscore Notation Conversion', () => {
    test('should convert dots to underscores', () => {
      expect(KnownSettings.toUnderscoreNotation('openai.token')).toBe('openai_token');
      expect(KnownSettings.toUnderscoreNotation('azure.openai.endpoint')).toBe('azure_openai_endpoint');
      expect(KnownSettings.toUnderscoreNotation('system.debug')).toBe('system_debug');
    });

    test('should leave underscores unchanged', () => {
      expect(KnownSettings.toUnderscoreNotation('openai_token')).toBe('openai_token');
      expect(KnownSettings.toUnderscoreNotation('github_copilot_token')).toBe('github_copilot_token');
    });

    test('should convert to lowercase', () => {
      expect(KnownSettings.toUnderscoreNotation('OPENAI.TOKEN')).toBe('openai_token');
      expect(KnownSettings.toUnderscoreNotation('GitHub.Copilot.Token')).toBe('github_copilot_token');
    });
  });

  describe('All Known Settings', () => {
    test('should return all known settings', () => {
      const allSettings = KnownSettings.getAllKnownSettings();
      
      expect(allSettings).toContain('provider');
      expect(allSettings).toContain('openai.token');
      expect(allSettings).toContain('github.copilot.token');
      expect(allSettings).toContain('azure.openai.endpoint');
      expect(allSettings).toContain('system.debug');
      
      // Should be sorted
      const sortedSettings = [...allSettings].sort();
      expect(allSettings).toEqual(sortedSettings);
    });
  });

  describe('Description', () => {
    test('should return description for known settings', () => {
      expect(KnownSettings.getDescription('provider')).toContain('AI provider');
      expect(KnownSettings.getDescription('openai.token')).toContain('OpenAI');
      expect(KnownSettings.getDescription('system.debug')).toContain('debug');
    });

    test('should return undefined for unknown settings', () => {
      expect(KnownSettings.getDescription('unknown.setting')).toBeUndefined();
    });
  });
});