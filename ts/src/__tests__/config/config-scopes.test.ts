import { CliTestHelper } from '../utils/CliTestHelper';

describe('Config Scope Tests', () => {
  beforeAll(async () => {
    // Clean up any existing test keys from ALL scopes before the test suite
    for (const key of ['ScopeTestKey', 'BoolInheritTest', 'ScopeGetKey']) {
      await CliTestHelper.run(`cycod config clear ${key} --global`).catch(() => {});
      await CliTestHelper.run(`cycod config clear ${key} --user`).catch(() => {});
      await CliTestHelper.run(`cycod config clear ${key} --local`).catch(() => {});
    }
  });

  afterAll(async () => {
    // Clean up test keys from ALL scopes after the test suite
    for (const key of ['ScopeTestKey', 'BoolInheritTest', 'ScopeGetKey']) {
      await CliTestHelper.run(`cycod config clear ${key} --global`).catch(() => {});
      await CliTestHelper.run(`cycod config clear ${key} --user`).catch(() => {});
      await CliTestHelper.run(`cycod config clear ${key} --local`).catch(() => {});
    }
  });

  describe('Value inheritance across scopes', () => {
    test('Boolean value inheritance with --any flag', async () => {
      // Set boolean to true in global scope
      let result = await CliTestHelper.run('cycod config set BoolInheritTest true --global');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(global\\).*BoolInheritTest: true`
      );

      // Set boolean to false in user scope
      result = await CliTestHelper.run('cycod config set BoolInheritTest false --user');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(user\\).*BoolInheritTest: false`
      );

      // Test inheritance using --any flag (should get user scope's false)
      result = await CliTestHelper.run('cycod config get BoolInheritTest --any');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(user\\).*BoolInheritTest: false`
      );

      // Clear user scope value to test inheritance 
      result = await CliTestHelper.run('cycod config clear BoolInheritTest --user');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `BoolInheritTest: \\(cleared\\)`
      );

      // Test inheritance again (should now get global scope's true)
      result = await CliTestHelper.run('cycod config get BoolInheritTest --any');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(global\\).*BoolInheritTest: true`
      );
    });
  });

  describe('Get values from different scopes', () => {
    test('Set and get values from specific scopes', async () => {
      // Set value in local scope
      let result = await CliTestHelper.run('cycod config set ScopeGetKey LocalValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*ScopeGetKey: LocalValue`
      );

      // Set value in user scope
      result = await CliTestHelper.run('cycod config set ScopeGetKey UserValue --user');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(user\\).*ScopeGetKey: UserValue`
      );

      // Set value in global scope
      result = await CliTestHelper.run('cycod config set ScopeGetKey GlobalValue --global');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(global\\).*ScopeGetKey: GlobalValue`
      );

      // Test getting values from specific scopes
      result = await CliTestHelper.run('cycod config get ScopeGetKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*ScopeGetKey: LocalValue`
      );

      result = await CliTestHelper.run('cycod config get ScopeGetKey --user');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(user\\).*ScopeGetKey: UserValue`
      );

      result = await CliTestHelper.run('cycod config get ScopeGetKey --global');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(global\\).*ScopeGetKey: GlobalValue`
      );

      // Test inheritance with --any flag (should return local value due to precedence)
      result = await CliTestHelper.run('cycod config get ScopeGetKey --any');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*ScopeGetKey: LocalValue`
      );
    });
  });

  describe('Non-existent keys from different scopes', () => {
    test('Get non-existent keys from different scopes', async () => {
      // Test getting non-existent keys from different scopes
      let result = await CliTestHelper.run('cycod config get NonExistentLocal --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `NonExistentLocal: \\(not found or empty\\)`
      );

      result = await CliTestHelper.run('cycod config get NonExistentUser --user');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `NonExistentUser: \\(not found or empty\\)`
      );

      result = await CliTestHelper.run('cycod config get NonExistentGlobal --global');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `NonExistentGlobal: \\(not found or empty\\)`
      );

      result = await CliTestHelper.run('cycod config get NonExistentAny --any');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `NonExistentAny: \\(not found or empty\\)`
      );
    });
  });
});