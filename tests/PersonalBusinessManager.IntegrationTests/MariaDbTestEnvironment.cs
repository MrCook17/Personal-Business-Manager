using PersonalBusinessManager.DatabaseMigrator;
using PersonalBusinessManager.Infrastructure.Configuration;

namespace PersonalBusinessManager.IntegrationTests;

internal static class MariaDbTestEnvironment
{
    static MariaDbTestEnvironment()
    {
        EnvironmentFileLoader.Load();
    }

    public static void EnsureLoaded()
    {
    }

    public static string GetRequiredRuntimeConnectionString()
    {
        string connectionString =
            Environment.GetEnvironmentVariable(
                TestDatabaseSafetyGuard
                    .RuntimeConnectionEnvironmentVariable,
                EnvironmentVariableTarget.Process)
            ?? throw new InvalidOperationException(
                TestDatabaseSafetyGuard
                    .RuntimeConnectionEnvironmentVariable
                + " is not configured for this test process.");
        TestDatabaseTargetValidation validation =
            TestDatabaseSafetyGuard
                .ValidateRuntimeConnectionString(
                    connectionString);

        if (!validation.IsSafe)
        {
            throw new InvalidOperationException(
                validation.Error
                    ?? "The test runtime target is unsafe.");
        }

        return connectionString;
    }

    public static string GetRequiredMigrationConnectionString()
    {
        string connectionString =
            Environment.GetEnvironmentVariable(
                TestDatabaseSafetyGuard
                    .MigrationConnectionEnvironmentVariable,
                EnvironmentVariableTarget.Process)
            ?? throw new InvalidOperationException(
                TestDatabaseSafetyGuard
                    .MigrationConnectionEnvironmentVariable
                + " is not configured for this test process.");
        TestDatabaseTargetValidation validation =
            TestDatabaseSafetyGuard
                .ValidateMigrationConnectionString(
                    connectionString);

        if (!validation.IsSafe)
        {
            throw new InvalidOperationException(
                validation.Error
                    ?? "The test migration target is unsafe.");
        }

        return connectionString;
    }
}

internal sealed class MariaDbTestFactAttribute : FactAttribute
{
    public MariaDbTestFactAttribute()
    {
        MariaDbTestEnvironment.EnsureLoaded();

        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(
                    TestDatabaseSafetyGuard
                        .RuntimeConnectionEnvironmentVariable,
                    EnvironmentVariableTarget.Process)))
        {
            Skip =
                "Set "
                + TestDatabaseSafetyGuard
                    .RuntimeConnectionEnvironmentVariable
                + " for the approved MariaDB test database.";
        }
    }
}

internal sealed class MariaDbMigrationTestFactAttribute
    : FactAttribute
{
    public MariaDbMigrationTestFactAttribute()
    {
        MariaDbTestEnvironment.EnsureLoaded();

        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(
                    TestDatabaseSafetyGuard
                        .MigrationConnectionEnvironmentVariable,
                    EnvironmentVariableTarget.Process)))
        {
            Skip =
                "Set "
                + TestDatabaseSafetyGuard
                    .MigrationConnectionEnvironmentVariable
                + " for migration-level MariaDB tests.";
        }
    }
}
