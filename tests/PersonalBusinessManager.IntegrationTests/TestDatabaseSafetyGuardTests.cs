using PersonalBusinessManager.DatabaseMigrator;

namespace PersonalBusinessManager.IntegrationTests;

public sealed class TestDatabaseSafetyGuardTests
{
    [Fact]
    public void RuntimeGuardAcceptsApprovedLocalTargetAndAccount()
    {
        TestDatabaseTargetValidation result =
            TestDatabaseSafetyGuard
                .ValidateRuntimeConnectionString(
                    BuildConnectionString(
                        TestDatabaseSafetyGuard
                            .ApprovedDatabaseName,
                        TestDatabaseSafetyGuard
                            .ApprovedRuntimeAccount));

        Assert.True(result.IsSafe);
        Assert.Null(result.Error);
    }

    [Fact]
    public void MigrationGuardAcceptsApprovedLocalTargetAndAccount()
    {
        TestDatabaseTargetValidation result =
            TestDatabaseSafetyGuard
                .ValidateMigrationConnectionString(
                    BuildConnectionString(
                        TestDatabaseSafetyGuard
                            .ApprovedDatabaseName,
                        TestDatabaseSafetyGuard
                            .ApprovedMigrationAccount));

        Assert.True(result.IsSafe);
        Assert.Null(result.Error);
    }

    [Theory]
    [InlineData("personal_business_manager")]
    [InlineData("personal_business_manager_dev")]
    [InlineData("personal_business_manager_backup")]
    public void RuntimeGuardRejectsNameWithoutTestMarker(
        string databaseName)
    {
        TestDatabaseTargetValidation result =
            TestDatabaseSafetyGuard
                .ValidateRuntimeConnectionString(
                    BuildConnectionString(
                        databaseName,
                        TestDatabaseSafetyGuard
                            .ApprovedRuntimeAccount));

        Assert.False(result.IsSafe);
        Assert.NotNull(result.Error);
    }

    [Theory]
    [InlineData("personal_business_manager_prod_test")]
    [InlineData("personal_business_manager_production_test")]
    [InlineData("personal_business_manager_live_test")]
    [InlineData("personal_business_manager_staging_test")]
    public void RuntimeGuardRejectsProductionLikeName(
        string databaseName)
    {
        TestDatabaseTargetValidation result =
            TestDatabaseSafetyGuard
                .ValidateRuntimeConnectionString(
                    BuildConnectionString(
                        databaseName,
                        TestDatabaseSafetyGuard
                            .ApprovedRuntimeAccount));

        Assert.False(result.IsSafe);
        Assert.Contains(
            "Production-like",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeGuardRejectsDifferentTestDatabase()
    {
        TestDatabaseTargetValidation result =
            TestDatabaseSafetyGuard
                .ValidateRuntimeConnectionString(
                    BuildConnectionString(
                        "another_application_test",
                        TestDatabaseSafetyGuard
                            .ApprovedRuntimeAccount));

        Assert.False(result.IsSafe);
        Assert.Contains(
            TestDatabaseSafetyGuard.ApprovedDatabaseName,
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeGuardRejectsRemoteServer()
    {
        TestDatabaseTargetValidation result =
            TestDatabaseSafetyGuard
                .ValidateRuntimeConnectionString(
                    BuildConnectionString(
                        TestDatabaseSafetyGuard
                            .ApprovedDatabaseName,
                        TestDatabaseSafetyGuard
                            .ApprovedRuntimeAccount,
                        "database.example.test"));

        Assert.False(result.IsSafe);
        Assert.Contains(
            "localhost",
            result.Error,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("root")]
    [InlineData("personal_business_test_migrator")]
    [InlineData("personal_business_app")]
    public void RuntimeGuardRejectsWrongRuntimeAccount(
        string accountName)
    {
        TestDatabaseTargetValidation result =
            TestDatabaseSafetyGuard
                .ValidateRuntimeConnectionString(
                    BuildConnectionString(
                        TestDatabaseSafetyGuard
                            .ApprovedDatabaseName,
                        accountName));

        Assert.False(result.IsSafe);
        Assert.Contains(
            TestDatabaseSafetyGuard.ApprovedRuntimeAccount,
            result.Error,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("root")]
    [InlineData("personal_business_test_app")]
    [InlineData("personal_business_migrator")]
    public void MigrationGuardRejectsWrongMigrationAccount(
        string accountName)
    {
        TestDatabaseTargetValidation result =
            TestDatabaseSafetyGuard
                .ValidateMigrationConnectionString(
                    BuildConnectionString(
                        TestDatabaseSafetyGuard
                            .ApprovedDatabaseName,
                        accountName));

        Assert.False(result.IsSafe);
        Assert.Contains(
            TestDatabaseSafetyGuard.ApprovedMigrationAccount,
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void GuardRejectsMissingDatabase()
    {
        TestDatabaseTargetValidation result =
            TestDatabaseSafetyGuard
                .ValidateRuntimeConnectionString(
                    "Server=localhost;"
                    + "User ID=personal_business_test_app");

        Assert.False(result.IsSafe);
        Assert.Contains(
            "name a database",
            result.Error,
            StringComparison.Ordinal);
    }

    private static string BuildConnectionString(
        string databaseName,
        string accountName,
        string server = "localhost")
    {
        return $"Server={server};Database={databaseName};"
            + $"User ID={accountName}";
    }
}
