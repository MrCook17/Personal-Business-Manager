using PersonalBusinessManager.DatabaseMigrator;

namespace PersonalBusinessManager.IntegrationTests;

public sealed class MigrationCommandOptionsTests
{
    [Fact]
    public void ParseRequiresAnExplicitCommand()
    {
        MigrationCommandParseResult result =
            MigrationCommandOptions.Parse([]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(
            "An explicit command is required.",
            result.Error);
    }

    [Fact]
    public void ParseStatusRequiresExplicitMigrationConnectionVariable()
    {
        MigrationCommandParseResult result =
            MigrationCommandOptions.Parse(["status"]);

        Assert.False(result.IsSuccessful);
        Assert.Contains(
            "--connection-env",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ParseRejectsRuntimeConnectionVariable()
    {
        MigrationCommandParseResult result =
            MigrationCommandOptions.Parse(
            [
                "status",
                "--connection-env",
                "PBM_CONNECTION_STRING",
            ]);

        Assert.False(result.IsSuccessful);
        Assert.Contains(
            "PBM_MIGRATION_CONNECTION_STRING",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ParseRejectsRawConnectionStringOption()
    {
        MigrationCommandParseResult result =
            MigrationCommandOptions.Parse(
            [
                "status",
                "--connection",
                "Server=localhost;Database=pbm_test",
            ]);

        Assert.False(result.IsSuccessful);
        Assert.Equal(
            "Unknown option: --connection",
            result.Error);
    }

    [Fact]
    public void ParseMigrateRequiresConfirmation()
    {
        MigrationCommandParseResult result =
            MigrationCommandOptions.Parse(
            [
                "migrate",
                "--connection-env",
                "PBM_MIGRATION_CONNECTION_STRING",
            ]);

        Assert.False(result.IsSuccessful);
        Assert.Contains(
            "--confirm",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ParseAcceptsExplicitStatusCommand()
    {
        MigrationCommandParseResult result =
            MigrationCommandOptions.Parse(
            [
                "status",
                "--connection-env",
                "PBM_MIGRATION_CONNECTION_STRING",
            ]);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Options);
        Assert.Equal(
            MigrationCommand.Status,
            result.Options.Command);
    }

    [Fact]
    public void ParseAcceptsExplicitMigrateCommandAndConfirmation()
    {
        MigrationCommandParseResult result =
            MigrationCommandOptions.Parse(
            [
                "migrate",
                "--connection-env",
                "PBM_TEST_MIGRATION_CONNECTION_STRING",
                "--confirm",
                "MIGRATE pbm_test",
            ]);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Options);
        Assert.Equal(
            "MIGRATE pbm_test",
            result.Options.Confirmation);
    }
}
