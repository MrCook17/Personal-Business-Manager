using System.Data;
using System.Globalization;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using PersonalBusinessManager.Infrastructure.Database.Migrations;

namespace PersonalBusinessManager.DatabaseMigrator;

public sealed class TestDatabaseResetService
{
    private const string ResetLockName =
        "pbm:personal_business_manager_test:reset";

    private readonly string _migrationConnectionString;
    private readonly MigrationRunnerService _migrationRunner;
    private readonly BaselineSchemaVerifier _verifier;
    private readonly ILogger<TestDatabaseResetService> _logger;

    public TestDatabaseResetService(
        string migrationConnectionString,
        MigrationRunnerService migrationRunner,
        BaselineSchemaVerifier verifier,
        ILogger<TestDatabaseResetService> logger)
    {
        _migrationConnectionString = migrationConnectionString;
        _migrationRunner = migrationRunner;
        _verifier = verifier;
        _logger = logger;
    }

    public async Task<TestDatabaseResetResult> ResetAsync(
        string? confirmation,
        CancellationToken cancellationToken)
    {
        TestDatabaseTargetValidation target =
            TestDatabaseSafetyGuard
                .ValidateMigrationConnectionString(
                    _migrationConnectionString);

        if (!target.IsSafe)
        {
            TestDatabaseResetLog.SafetyGuardBlocked(
                _logger,
                target.Error ?? "Unknown safety problem");

            return TestDatabaseResetResult.Failure(
                target.Error
                    ?? "Test database safety validation failed.");
        }

        string requiredConfirmation =
            $"RESET TEST DATABASE {target.DatabaseName}";

        if (!string.Equals(
                confirmation,
                requiredConfirmation,
                StringComparison.Ordinal))
        {
            TestDatabaseResetLog.ConfirmationMismatch(_logger);

            return TestDatabaseResetResult.Failure(
                $"Confirmation must be exactly: {requiredConfirmation}");
        }

        TestDatabaseResetLog.ResetStarted(
            _logger,
            target.ServerHost,
            target.DatabaseName,
            target.AccountName);

        var serverConnectionBuilder =
            new MySqlConnectionStringBuilder(
                _migrationConnectionString)
            {
                Database = string.Empty,
                Pooling = false,
            };

        await using var serverConnection =
            new MySqlConnection(
                serverConnectionBuilder.ConnectionString);
        bool lockAcquired = false;

        try
        {
            await serverConnection.OpenAsync(cancellationToken);
            lockAcquired = await AcquireResetLockAsync(
                serverConnection,
                cancellationToken);

            if (!lockAcquired)
            {
                return TestDatabaseResetResult.Failure(
                    "Another test database reset currently holds the reset lock.");
            }

            await RecreateDatabaseAsync(
                serverConnection,
                target.DatabaseName,
                cancellationToken);

            MigrationExecutionResult migrationResult =
                await _migrationRunner.MigrateAsync(
                    $"MIGRATE {target.DatabaseName}",
                    cancellationToken);

            if (!migrationResult.Succeeded)
            {
                return TestDatabaseResetResult.Failure(
                    "The test database was recreated, but migrations failed.");
            }

            BaselineVerificationResult verification =
                await _verifier.VerifyCurrentAsync(
                    cancellationToken);

            if (!verification.Succeeded)
            {
                return TestDatabaseResetResult.Failure(
                    "The test database was migrated, but verification failed.",
                    verification);
            }

            TestDatabaseResetLog.ResetSucceeded(
                _logger,
                target.DatabaseName,
                migrationResult.Status?.HighestAppliedVersion);

            return TestDatabaseResetResult.Success(verification);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            TestDatabaseResetLog.ResetFailed(
                _logger,
                target.DatabaseName,
                exception.GetType().Name);

            return TestDatabaseResetResult.Failure(
                "The test database reset failed. Review the safe log.");
        }
        finally
        {
            if (lockAcquired
                && serverConnection.State == ConnectionState.Open)
            {
                await ReleaseResetLockAsync(serverConnection);
            }
        }
    }

    private static async Task<bool> AcquireResetLockAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql =
            "SELECT GET_LOCK(@lock_name, 10);";
        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@lock_name",
            ResetLockName);
        object? result =
            await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(
                result,
                CultureInfo.InvariantCulture)
            == 1;
    }

    private static async Task RecreateDatabaseAsync(
        MySqlConnection connection,
        string databaseName,
        CancellationToken cancellationToken)
    {
        string quotedDatabaseName =
            $"`{databaseName.Replace(
                "`",
                "``",
                StringComparison.Ordinal)}`";

        await using var dropCommand = new MySqlCommand(
            $"DROP DATABASE IF EXISTS {quotedDatabaseName};",
            connection);
        await dropCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var createCommand = new MySqlCommand(
            $"""
            CREATE DATABASE {quotedDatabaseName}
                CHARACTER SET utf8mb4
                COLLATE utf8mb4_unicode_ci;
            """,
            connection);
        await createCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ReleaseResetLockAsync(
        MySqlConnection connection)
    {
        const string sql =
            "SELECT RELEASE_LOCK(@lock_name);";
        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@lock_name",
            ResetLockName);
        await command.ExecuteScalarAsync(CancellationToken.None);
    }
}

public sealed record TestDatabaseResetResult(
    bool Succeeded,
    string Message,
    BaselineVerificationResult? Verification)
{
    public static TestDatabaseResetResult Success(
        BaselineVerificationResult verification)
    {
        return new TestDatabaseResetResult(
            true,
            "The approved test database was recreated and migrated successfully.",
            verification);
    }

    public static TestDatabaseResetResult Failure(
        string message,
        BaselineVerificationResult? verification = null)
    {
        return new TestDatabaseResetResult(
            false,
            message,
            verification);
    }
}

internal static partial class TestDatabaseResetLog
{
    [LoggerMessage(
        EventId = 3100,
        Level = LogLevel.Warning,
        Message = "Test database reset safety guard blocked execution: {Reason}")]
    public static partial void SafetyGuardBlocked(
        ILogger logger,
        string reason);

    [LoggerMessage(
        EventId = 3101,
        Level = LogLevel.Warning,
        Message = "Test database reset confirmation did not match the target.")]
    public static partial void ConfirmationMismatch(
        ILogger logger);

    [LoggerMessage(
        EventId = 3102,
        Level = LogLevel.Information,
        Message =
            "Test database reset started on {ServerHost}; database {DatabaseName}; account {AccountName}.")]
    public static partial void ResetStarted(
        ILogger logger,
        string serverHost,
        string databaseName,
        string accountName);

    [LoggerMessage(
        EventId = 3103,
        Level = LogLevel.Information,
        Message =
            "Test database {DatabaseName} was recreated and migrated to version {Version}.")]
    public static partial void ResetSucceeded(
        ILogger logger,
        string databaseName,
        long? version);

    [LoggerMessage(
        EventId = 3104,
        Level = LogLevel.Error,
        Message =
            "Test database reset failed for {DatabaseName}. Error type: {ErrorType}.")]
    public static partial void ResetFailed(
        ILogger logger,
        string databaseName,
        string errorType);
}
