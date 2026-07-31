using System.Text.RegularExpressions;
using MySqlConnector;

namespace PersonalBusinessManager.DatabaseMigrator;

public static partial class TestDatabaseSafetyGuard
{
    public const string ApprovedDatabaseName =
        "personal_business_manager_test";

    public const string ApprovedRuntimeAccount =
        "personal_business_test_app";

    public const string ApprovedMigrationAccount =
        "personal_business_test_migrator";

    public const string RuntimeConnectionEnvironmentVariable =
        "PBM_TEST_CONNECTION_STRING";

    public const string MigrationConnectionEnvironmentVariable =
        "PBM_TEST_MIGRATION_CONNECTION_STRING";

    private const string DevelopmentDatabaseName =
        "personal_business_manager";

    public static TestDatabaseTargetValidation
        ValidateRuntimeConnectionString(
            string? connectionString)
    {
        return ValidateConnectionString(
            connectionString,
            ApprovedRuntimeAccount,
            "runtime");
    }

    public static TestDatabaseTargetValidation
        ValidateMigrationConnectionString(
            string? connectionString)
    {
        return ValidateConnectionString(
            connectionString,
            ApprovedMigrationAccount,
            "migration");
    }

    private static TestDatabaseTargetValidation
        ValidateConnectionString(
            string? connectionString,
            string requiredAccount,
            string accountRole)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return TestDatabaseTargetValidation.Failure(
                "The test connection string is absent or empty.");
        }

        MySqlConnectionStringBuilder builder;

        try
        {
            builder = new MySqlConnectionStringBuilder(
                connectionString);
        }
        catch (ArgumentException)
        {
            return TestDatabaseTargetValidation.Failure(
                "The test connection string is invalid.");
        }

        string databaseName = builder.Database.Trim();
        string serverHost = builder.Server.Trim();
        string accountName = builder.UserID.Trim();

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return TestDatabaseTargetValidation.Failure(
                "The test connection must name a database.");
        }

        if (string.Equals(
                databaseName,
                DevelopmentDatabaseName,
                StringComparison.OrdinalIgnoreCase))
        {
            return TestDatabaseTargetValidation.Failure(
                "The normal development database is never a test target.");
        }

        if (!databaseName.EndsWith(
                "_test",
                StringComparison.OrdinalIgnoreCase))
        {
            return TestDatabaseTargetValidation.Failure(
                "The database name must end with the '_test' marker.");
        }

        if (ProductionLikeNamePattern().IsMatch(databaseName))
        {
            return TestDatabaseTargetValidation.Failure(
                "Production-like database names are never test targets.");
        }

        if (!string.Equals(
                databaseName,
                ApprovedDatabaseName,
                StringComparison.Ordinal))
        {
            return TestDatabaseTargetValidation.Failure(
                $"The approved test database is {ApprovedDatabaseName}.");
        }

        if (!IsLocalHost(serverHost))
        {
            return TestDatabaseTargetValidation.Failure(
                "The approved development test database must use localhost.");
        }

        if (!string.Equals(
                accountName,
                requiredAccount,
                StringComparison.Ordinal))
        {
            return TestDatabaseTargetValidation.Failure(
                $"The test {accountRole} connection must use account "
                + $"{requiredAccount}.");
        }

        return TestDatabaseTargetValidation.Success(
            databaseName,
            serverHost,
            accountName);
    }

    private static bool IsLocalHost(string serverHost)
    {
        return string.Equals(
                serverHost,
                "localhost",
                StringComparison.OrdinalIgnoreCase)
            || serverHost is "127.0.0.1" or "::1";
    }

    [GeneratedRegex(
        "(^|_)(prod|production|live|staging)($|_)",
        RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant)]
    private static partial Regex ProductionLikeNamePattern();
}

public sealed record TestDatabaseTargetValidation(
    bool IsSafe,
    string DatabaseName,
    string ServerHost,
    string AccountName,
    string? Error)
{
    public static TestDatabaseTargetValidation Success(
        string databaseName,
        string serverHost,
        string accountName)
    {
        return new TestDatabaseTargetValidation(
            true,
            databaseName,
            serverHost,
            accountName,
            null);
    }

    public static TestDatabaseTargetValidation Failure(
        string error)
    {
        return new TestDatabaseTargetValidation(
            false,
            string.Empty,
            string.Empty,
            string.Empty,
            error);
    }
}
