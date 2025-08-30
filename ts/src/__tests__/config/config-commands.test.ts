import { CliTestHelper } from '../helpers/CliTestHelper';

describe('Config Commands', () => {
  beforeAll(async () => {
    // Clean up any existing test keys
    await CliTestHelper.cleanup('TestKey');
    await CliTestHelper.cleanup('TestList');
    await CliTestHelper.cleanup('NonexistentKey');
    await CliTestHelper.cleanup('OpenAI.ApiKey');
  });

  afterAll(async () => {
    // Clean up test keys after all tests
    await CliTestHelper.cleanup('TestKey');
    await CliTestHelper.cleanup('TestList');
    await CliTestHelper.cleanup('OpenAI.ApiKey');
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
    it('Config Set (local scope)', async () => {
      const result = await CliTestHelper.run('cycodjs config set TestKey TestValue --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `TestKey: TestValue`
      );
    });

    it('Config Get (local scope)', async () => {
      const result = await CliTestHelper.run('cycodjs config get TestKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `TestKey: TestValue`
      );
    });
  });

  describe('Config Clear', () => {
    it('Config Clear (local scope)', async () => {
      const result = await CliTestHelper.run('cycodjs config clear TestKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(`TestKey: \\(cleared\\)`);
    });
  });

  describe('Config List Operations', () => {
    it('Config Add to List (local scope)', async () => {
      const result = await CliTestHelper.run('cycodjs config add TestList TestItem1 --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList:.*` +
        `- TestItem1`
      );
    });

    it('Config Add Another Value to List (local scope)', async () => {
      const result = await CliTestHelper.run('cycodjs config add TestList TestItem2 --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList:.*` +
        `- TestItem1.*` +
        `- TestItem2`
      );
    });

    it('Config Get List Value (local scope)', async () => {
      const result = await CliTestHelper.run('cycodjs config get TestList --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `TestList:.*` +
        `- TestItem1.*` +
        `- TestItem2`
      );
    });

    it('Config Remove Value from List (local scope)', async () => {
      const result = await CliTestHelper.run('cycodjs config remove TestList TestItem1 --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `TestList:.*` +
        `- TestItem2`
      );
    });

    it('Config Clear List (local scope)', async () => {
      const result = await CliTestHelper.run('cycodjs config clear TestList --local');
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
    it('Config Set creates correct casing', async () => {
      const result = await CliTestHelper.run('cycodjs config set OpenAI.ApiKey 1234567890 --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(
        `LOCATION: .*config\\.yaml \\(local\\).*` +
        `OpenAI.ApiKey: 12`
      );
    });

    it('Config Get creates correct casing', async () => {
      const result = await CliTestHelper.run('cycodjs config get OpenAI.ApiKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(`OpenAI.ApiKey: 12`);
    });

    it('Config Clear creates correct casing', async () => {
      const result = await CliTestHelper.run('cycodjs config clear OpenAI.ApiKey --local');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatchYamlRegex(`OpenAI.ApiKey: \\(cleared\\)`);
    });
  });
});