using PersonalBusinessManager.DatabaseMigrator;

namespace PersonalBusinessManager.IntegrationTests;

internal static class MariaDbTestEnvironment
{
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
}

internal sealed class MariaDbTestFactAttribute : FactAttribute
{
    public MariaDbTestFactAttribute()
    {
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
