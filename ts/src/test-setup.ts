// Test setup file for Jest
// Add any global test setup here

import * as fs from 'fs-extra';
import * as path from 'path';
import * as os from 'os';
import { RegexMatcher } from './__tests__/utils/CliTestHelper';

// Add custom Jest matcher for YAML-style regex testing
expect.extend({
  toMatchYamlRegex(received: string, pattern: string) {
    const matches = RegexMatcher.matches(received, pattern);
    
    if (matches) {
      return {
        message: () => `Expected output NOT to match regex pattern:\n${pattern}\n\nBut it matched:\n${received}`,
        pass: true
      };
    } else {
      return {
        message: () => `Expected output to match regex pattern:\n${pattern}\n\nActual output:\n${received}`,
        pass: false
      };
    }
  }
});

// Setup temporary directories for tests
export const createTempDir = async (): Promise<string> => {
  const tempDir = await fs.mkdtemp(path.join(os.tmpdir(), 'cycod-test-'));
  return tempDir;
};

export const cleanupTempDir = async (tempDir: string): Promise<void> => {
  await fs.remove(tempDir);
};

// Mock console methods for testing
export const mockConsole = () => {
  const originalLog = console.log;
  const originalError = console.error;
  const logs: string[] = [];
  const errors: string[] = [];

  console.log = jest.fn((...args: any[]) => {
    logs.push(args.join(' '));
  });

  console.error = jest.fn((...args: any[]) => {
    errors.push(args.join(' '));
  });

  return {
    logs,
    errors,
    restore: () => {
      console.log = originalLog;
      console.error = originalError;
    },
  };
};