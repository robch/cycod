import { CliTestHelper } from '../helpers/CliTestHelper';

describe('Config Commands', () => {
  beforeAll(async () => {
    // Clean up any existing test keys from all files
    await CliTestHelper.cleanupAllTestKeys();
  });

  afterAll(async () => {
    // Clean up all test keys after all tests
    await CliTestHelper.cleanupAllTestKeys();
  });


  describe('Config List', () => {
    it('Config List (all scopes)', async () => {
      const result = await CliTestHelper.run('cycodjs config list');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(global\\).*` +
        `LOCATION: .*config\\.yaml \\(user\\).*` +
        `LOCATION: .*config\\.yaml \\(local\\)`
      );
    });

    it('Config List (local scope only)', async () => {
      const result = await CliTestHelper.run('cycodjs config list --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\)`
      );
    });

    it('Config List (user scope only)', async () => {
      const result = await CliTestHelper.run('cycodjs config list --user');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(user\\)`
      );
    });

    it('Config List (global scope only)', async () => {
      const result = await CliTestHelper.run('cycodjs config list --global');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(global\\)`
      );
    });
  });

  describe('Config Set and Get', () => {
    it('Config Set and Get (local scope)', async () => {
      // Set the value
      let result = await CliTestHelper.run('cycodjs config set TestKey TestValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `TestKey: TestValue`
      );

      // Get the value  
      result = await CliTestHelper.run('cycodjs config get TestKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `TestKey: TestValue`
      );
    });
  });

  describe('Config Clear', () => {
    it('Config Clear (local scope)', async () => {
      // First set a value
      await CliTestHelper.run('cycodjs config set TestKey TestValue --local');
      
      // Then clear it
      const result = await CliTestHelper.run('cycodjs config clear TestKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(`TestKey: \\(cleared\\)`);
    });
  });

  describe('Config List Operations', () => {
    it('Config List Operations (complete workflow)', async () => {
      // Add first item to list
      let result = await CliTestHelper.run('cycodjs config add TestList TestItem1 --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList:.*` +
        `- TestItem1`
      );

      // Add second item to list
      result = await CliTestHelper.run('cycodjs config add TestList TestItem2 --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList:.*` +
        `- TestItem1.*` +
        `- TestItem2`
      );

      // Get the list value
      result = await CliTestHelper.run('cycodjs config get TestList --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `TestList:.*` +
        `- TestItem1.*` +
        `- TestItem2`
      );

      // Remove one item from list
      result = await CliTestHelper.run('cycodjs config remove TestList TestItem1 --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList:.*` +
        `- TestItem2`
      );

      // Clear the list
      result = await CliTestHelper.run('cycodjs config clear TestList --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(`TestList: \\(cleared\\)`);
    });
  });

  describe('Config Error Cases', () => {
    it('Config Get Nonexistent (local scope)', async () => {
      const result = await CliTestHelper.run('cycodjs config get NonexistentKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(`NonexistentKey: \\(not found or empty\\)`);
    });
  });

  describe('Config Key Normalization', () => {
    it('Config Key Normalization (complete workflow)', async () => {
      // Set API key (shows masked value)
      let result = await CliTestHelper.run('cycodjs config set OpenAI.ApiKey 1234567890 --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `OpenAI.ApiKey: 12`
      );

      // Get API key (shows masked value)
      result = await CliTestHelper.run('cycodjs config get OpenAI.ApiKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `OpenAI.ApiKey: 12`
      );

      // Clear API key
      result = await CliTestHelper.run('cycodjs config clear OpenAI.ApiKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(`OpenAI.ApiKey: \\(cleared\\)`);
    });
  });
});