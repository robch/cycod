import { expect } from '@jest/globals';

declare global {
  namespace jest {
    interface Matchers<R> {
      toMatchYamlRegex(pattern: string): R;
    }
  }
}

expect.extend({
  toMatchYamlRegex(received: string, pattern: string) {
    const regex = new RegExp(pattern, 's');
    const pass = regex.test(received);
    
    if (pass) {
      return {
        message: () =>
          `Expected output not to match regex pattern:\n${pattern}\n\nActual output:\n${received}`,
        pass: true,
      };
    } else {
      return {
        message: () =>
          `Expected output to match regex pattern:\n${pattern}\n\nActual output:\n${received}`,
        pass: false,
      };
    }
  },
});