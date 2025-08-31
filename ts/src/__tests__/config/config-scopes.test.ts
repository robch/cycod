import { CliTestHelper } from '../helpers/CliTestHelper';

describe('Config Scopes', () => {
  beforeAll(async () => {
    // Clean up any existing test keys from all files
    await CliTestHelper.cleanupAllTestKeys();
  });

  afterAll(async () => {
    // Clean up all test keys after all tests
    await CliTestHelper.cleanupAllTestKeys();
  });


  describe('Scope Inheritance', () => {
    it('should follow Local > User > Global precedence', async () => {
      // Set value in global scope
      let result = await CliTestHelper.run('cycodjs config set BoolTest true --global');
      expect(result.exitCode).toBe(0);

      // Set value in user scope (should override global)
      result = await CliTestHelper.run('cycodjs config set BoolTest false --user');
      expect(result.exitCode).toBe(0);

      // Get with --any should return user value
      result = await CliTestHelper.run('cycodjs config get BoolTest --any');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(user\\).*` +
        `BoolTest: false`
      );

      // Clean up
      await CliTestHelper.cleanup('BoolTest');
    });

    it('should fall back to global when user is cleared', async () => {
      // Set values in both global and user scopes
      await CliTestHelper.run('cycodjs config set TestInheritance GlobalValue --global');
      await CliTestHelper.run('cycodjs config set TestInheritance UserValue --user');

      // Get with --any should return user value
      let result = await CliTestHelper.run('cycodjs config get TestInheritance --any');
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(user\\).*` +
        `TestInheritance: UserValue`
      );

      // Clear user value
      await CliTestHelper.run('cycodjs config clear TestInheritance --user');

      // Get with --any should now return global value
      result = await CliTestHelper.run('cycodjs config get TestInheritance --any');
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(global\\).*` +
        `TestInheritance: GlobalValue`
      );

      // Clean up
      await CliTestHelper.cleanup('TestInheritance');
    });

    it('should prioritize local over user and global', async () => {
      // Set values in all three scopes
      await CliTestHelper.run('cycodjs config set TestInheritance GlobalValue --global');
      await CliTestHelper.run('cycodjs config set TestInheritance UserValue --user');
      await CliTestHelper.run('cycodjs config set TestInheritance LocalValue --local');

      // Get with --any should return local value
      const result = await CliTestHelper.run('cycodjs config get TestInheritance --any');
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `TestInheritance: LocalValue`
      );

      // Clean up
      await CliTestHelper.cleanup('TestInheritance');
    });
  });
});