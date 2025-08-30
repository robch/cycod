import { CliTestHelper } from '../helpers/CliTestHelper';

describe('Config Nested Keys', () => {
  beforeAll(async () => {
    // Clean up any existing test keys
    await CliTestHelper.cleanup('App.Setting');
    await CliTestHelper.cleanup('App.Setting.Nested');
    await CliTestHelper.cleanup('Deep.Nested.Config.Key');
    await CliTestHelper.cleanup('Scopes.Nested.Test');
  });

  afterAll(async () => {
    // Clean up test keys after all tests
    await CliTestHelper.cleanup('App.Setting');
    await CliTestHelper.cleanup('App.Setting.Nested');
    await CliTestHelper.cleanup('Deep.Nested.Config.Key');
    await CliTestHelper.cleanup('Scopes.Nested.Test');
  });

  describe('Basic Nested Key Operations', () => {
    it('should set and get single-level nested key', async () => {
      // Set single-level nested key
      let result = await CliTestHelper.run('cycodjs config set App.Setting SimpleValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `App.Setting: SimpleValue`
      );

      // Get single-level nested key
      result = await CliTestHelper.run('cycodjs config get App.Setting --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `App.Setting: SimpleValue`
      );
    });

    it('should set and get multi-level nested key', async () => {
      // Set multi-level nested key
      let result = await CliTestHelper.run('cycodjs config set App.Setting.Nested NestedValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `App.Setting.Nested: NestedValue`
      );

      // Get multi-level nested key
      result = await CliTestHelper.run('cycodjs config get App.Setting.Nested --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `App.Setting.Nested: NestedValue`
      );
    });

    it('should set and get deeply nested key', async () => {
      // Set deeply nested key
      let result = await CliTestHelper.run('cycodjs config set Deep.Nested.Config.Key DeeplyNestedValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `Deep.Nested.Config.Key: DeeplyNestedValue`
      );

      // Get deeply nested key
      result = await CliTestHelper.run('cycodjs config get Deep.Nested.Config.Key --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `Deep.Nested.Config.Key: DeeplyNestedValue`
      );
    });

    it('should override nested key values', async () => {
      // Override nested key
      let result = await CliTestHelper.run('cycodjs config set App.Setting.Nested OverriddenNestedValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `App.Setting.Nested: OverriddenNestedValue`
      );

      // Get overridden nested key
      result = await CliTestHelper.run('cycodjs config get App.Setting.Nested --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `App.Setting.Nested: OverriddenNestedValue`
      );
    });

    it('should clear nested keys', async () => {
      // Clear nested key
      const result = await CliTestHelper.run('cycodjs config clear App.Setting.Nested --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(`App.Setting.Nested: \\(cleared\\)`);
    });
  });

  describe('Nested Keys Across Scopes', () => {
    it('should handle nested keys in different scopes with proper inheritance', async () => {
      // Set nested keys in different scopes
      await CliTestHelper.run('cycodjs config set Scopes.Nested.Test GlobalNestedValue --global');
      await CliTestHelper.run('cycodjs config set Scopes.Nested.Test UserNestedValue --user');
      await CliTestHelper.run('cycodjs config set Scopes.Nested.Test LocalNestedValue --local');

      // Get from specific scopes
      let result = await CliTestHelper.run('cycodjs config get Scopes.Nested.Test --local');
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `Scopes.Nested.Test: LocalNestedValue`
      );

      result = await CliTestHelper.run('cycodjs config get Scopes.Nested.Test --user');
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(user\\).*` +
        `Scopes.Nested.Test: UserNestedValue`
      );

      result = await CliTestHelper.run('cycodjs config get Scopes.Nested.Test --global');
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(global\\).*` +
        `Scopes.Nested.Test: GlobalNestedValue`
      );

      // Test inheritance with --any flag (should return local value)
      result = await CliTestHelper.run('cycodjs config get Scopes.Nested.Test --any');
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `Scopes.Nested.Test: LocalNestedValue`
      );
    });
  });
});