import { CliTestHelper } from '../helpers/CliTestHelper';

describe('Config Commands', () => {
  beforeAll(async () => {
    // Ensure the CLI is built before running tests
    // In a real test setup, this would be handled by the build process
    
    // Clean up test keys from all scopes before the test suite
    const testKeys = ['TestKey', 'TestList', 'NonexistentKey'];
    for (const key of testKeys) {
      await CliTestHelper.run(`cycod config clear ${key} --global`).catch(() => {});
      await CliTestHelper.run(`cycod config clear ${key} --user`).catch(() => {});
      await CliTestHelper.run(`cycod config clear ${key} --local`).catch(() => {});
    }
    // Also clear the OpenAI.ApiKey test key
    await CliTestHelper.run(`cycod config clear OpenAI.ApiKey --local`).catch(() => {});
  });

  afterAll(async () => {
    // Clean up test keys from all scopes after the test suite 
    const testKeys = ['TestKey', 'TestList', 'NonexistentKey'];
    for (const key of testKeys) {
      await CliTestHelper.run(`cycod config clear ${key} --global`).catch(() => {});
      await CliTestHelper.run(`cycod config clear ${key} --user`).catch(() => {});
      await CliTestHelper.run(`cycod config clear ${key} --local`).catch(() => {});
    }
    // Also clear the OpenAI.ApiKey test key
    await CliTestHelper.run(`cycod config clear OpenAI.ApiKey --local`).catch(() => {});
  });

  describe('Config List', () => {
    test('Config List (all scopes)', async () => {
      const result = await CliTestHelper.run('cycod config list');
      
      expect(result.exitCode).toBe(0);
      // Check that all three scope headers are present
      expect(result.stdout).toMatch(/LOCATION:.*\.cycod.*config.*\(global\)/);
      expect(result.stdout).toMatch(/LOCATION:.*\.cycod.*config.*\(user\)/);
      expect(result.stdout).toMatch(/LOCATION:.*\.cycod.*config.*\(local\)/);
    });

    test('Config List (local scope only)', async () => {
      const result = await CliTestHelper.run('cycod config list --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config(\\.yaml){0,1} \\(local\\)`
      );
      // Should not contain user or global scope headers
      expect(result.stdout).not.toMatch(/\(user\)/);
      expect(result.stdout).not.toMatch(/\(global\)/);
    });

    test('Config List (user scope only)', async () => {
      const result = await CliTestHelper.run('cycod config list --user');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config(\\.yaml){0,1} \\(user\\)`
      );
      // Should not contain local or global scope headers
      expect(result.stdout).not.toMatch(/\(local\)/);
      expect(result.stdout).not.toMatch(/\(global\)/);
    });

    test('Config List (global scope only)', async () => {
      const result = await CliTestHelper.run('cycod config list --global');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config(\\.yaml){0,1} \\(global\\)`
      );
      // Should not contain local or user scope headers
      expect(result.stdout).not.toMatch(/\(local\)/);
      expect(result.stdout).not.toMatch(/\(user\)/);
    });
  });

  describe('Config Set and Get', () => {
    test('Config Set (local scope)', async () => {
      const result = await CliTestHelper.run('cycod config set TestKey TestValue --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config(\\.yaml){0,1} \\(local\\)` +
        `.*TestKey: TestValue`
      );
    });

    test('Config Get (local scope)', async () => {
      const result = await CliTestHelper.run('cycod config get TestKey --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config(\\.yaml){0,1} \\(local\\)` +
        `.*TestKey: TestValue`
      );
    });

    test('Config Get Nonexistent (local scope)', async () => {
      const result = await CliTestHelper.run('cycod config get NonexistentKey --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `NonexistentKey: \\(not found or empty\\)`
      );
    });
  });

  describe('Config Clear', () => {
    test('Config Clear (local scope)', async () => {
      const result = await CliTestHelper.run('cycod config clear TestKey --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestKey: \\(cleared\\)`
      );
    });
  });

  describe('Config List Operations', () => {
    test('Config Add to List (local scope)', async () => {
      const result = await CliTestHelper.run('cycod config add TestList TestItem1 --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList:.*- TestItem1`
      );
    });

    test('Config Add Another Value to List (local scope)', async () => {
      const result = await CliTestHelper.run('cycod config add TestList TestItem2 --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList:.*- TestItem1.*- TestItem2`
      );
    });

    test('Config Get List Value (local scope)', async () => {
      const result = await CliTestHelper.run('cycod config get TestList --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config(\\.yaml){0,1} \\(local\\)` +
        `.*TestList:.*- TestItem1.*- TestItem2`
      );
    });

    test('Config Remove Value from List (local scope)', async () => {
      const result = await CliTestHelper.run('cycod config remove TestList TestItem1 --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList:.*- TestItem2`
      );
    });

    test('Config Clear List (local scope)', async () => {
      const result = await CliTestHelper.run('cycod config clear TestList --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList: \\(cleared\\)`
      );
    });
  });

  describe('Config Key Casing', () => {
    test('Config Set creates correct casing', async () => {
      const result = await CliTestHelper.run('cycod config set openai.apiKey 1234567890 --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config(\\.yaml){0,1} \\(local\\)` +
        `.*OpenAI\\.ApiKey: 12`
      );
    });

    test('Config Get creates correct casing', async () => {
      const result = await CliTestHelper.run('cycod config get openai.apiKey --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `OpenAI\\.ApiKey: 12`
      );
    });

    test('Config Clear creates correct casing', async () => {
      const result = await CliTestHelper.run('cycod config clear openai.apiKey --local');
      
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `OpenAI\\.ApiKey: \\(cleared\\)`
      );
    });
  });
});