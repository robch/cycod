import { CliTestHelper } from '../utils/CliTestHelper';

describe('Config Nested Keys Tests', () => {
  beforeEach(async () => {
    // Clean up any existing test keys before each test
    await CliTestHelper.run('cycod config clear App.Setting --any').catch(() => {});
    await CliTestHelper.run('cycod config clear App.Setting.Nested --any').catch(() => {});
    await CliTestHelper.run('cycod config clear Deep.Nested.Config.Key --any').catch(() => {});
    await CliTestHelper.run('cycod config clear Scopes.Nested.Test --any').catch(() => {});
  });

  afterEach(async () => {
    // Clean up test keys after each test
    await CliTestHelper.run('cycod config clear App.Setting --any').catch(() => {});
    await CliTestHelper.run('cycod config clear App.Setting.Nested --any').catch(() => {});
    await CliTestHelper.run('cycod config clear Deep.Nested.Config.Key --any').catch(() => {});
    await CliTestHelper.run('cycod config clear Scopes.Nested.Test --any').catch(() => {});
  });

  describe('Set and get nested configuration keys', () => {
    test('Single-level nested key operations', async () => {
      // Test setting single-level nested key
      let result = await CliTestHelper.run('cycod config set App.Setting SimpleValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*App\\.Setting: SimpleValue`
      );

      // Test getting single-level nested key
      result = await CliTestHelper.run('cycod config get App.Setting --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*App\\.Setting: SimpleValue`
      );
    });

    test('Multi-level nested key operations', async () => {
      // Test setting multi-level nested key
      let result = await CliTestHelper.run('cycod config set App.Setting.Nested NestedValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*App\\.Setting\\.Nested: NestedValue`
      );

      // Test getting multi-level nested key
      result = await CliTestHelper.run('cycod config get App.Setting.Nested --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*App\\.Setting\\.Nested: NestedValue`
      );
    });

    test('Deeply nested key operations', async () => {
      // Test setting deeply nested key
      let result = await CliTestHelper.run('cycod config set Deep.Nested.Config.Key DeeplyNestedValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*Deep\\.Nested\\.Config\\.Key: DeeplyNestedValue`
      );

      // Test getting deeply nested key
      result = await CliTestHelper.run('cycod config get Deep.Nested.Config.Key --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*Deep\\.Nested\\.Config\\.Key: DeeplyNestedValue`
      );
    });

    test('Override nested key', async () => {
      // First set a nested key
      await CliTestHelper.run('cycod config set App.Setting.Nested NestedValue --local');

      // Test overriding nested key
      let result = await CliTestHelper.run('cycod config set App.Setting.Nested OverriddenNestedValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*App\\.Setting\\.Nested: OverriddenNestedValue`
      );

      // Test getting overridden nested key
      result = await CliTestHelper.run('cycod config get App.Setting.Nested --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*App\\.Setting\\.Nested: OverriddenNestedValue`
      );
    });

    test('Clear nested key', async () => {
      // First set a nested key
      await CliTestHelper.run('cycod config set App.Setting.Nested NestedValue --local');

      // Test clearing nested key
      let result = await CliTestHelper.run('cycod config clear App.Setting.Nested --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `App\\.Setting\\.Nested: \\(cleared\\)`
      );
    });
  });

  describe('Nested keys in different scopes', () => {
    test('Set and get nested keys across scopes', async () => {
      // Set nested keys in different scopes
      let result = await CliTestHelper.run('cycod config set Scopes.Nested.Test LocalNestedValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*Scopes\\.Nested\\.Test: LocalNestedValue`
      );

      result = await CliTestHelper.run('cycod config set Scopes.Nested.Test UserNestedValue --user');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(user\\).*Scopes\\.Nested\\.Test: UserNestedValue`
      );

      result = await CliTestHelper.run('cycod config set Scopes.Nested.Test GlobalNestedValue --global');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(global\\).*Scopes\\.Nested\\.Test: GlobalNestedValue`
      );

      // Test getting nested keys from specific scopes
      result = await CliTestHelper.run('cycod config get Scopes.Nested.Test --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*Scopes\\.Nested\\.Test: LocalNestedValue`
      );

      result = await CliTestHelper.run('cycod config get Scopes.Nested.Test --user');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(user\\).*Scopes\\.Nested\\.Test: UserNestedValue`
      );

      result = await CliTestHelper.run('cycod config get Scopes.Nested.Test --global');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(global\\).*Scopes\\.Nested\\.Test: GlobalNestedValue`
      );

      // Test inheritance with --any flag (should return local value due to precedence)
      result = await CliTestHelper.run('cycod config get Scopes.Nested.Test --any');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config\\.yaml \\(local\\).*Scopes\\.Nested\\.Test: LocalNestedValue`
      );
    });
  });
});