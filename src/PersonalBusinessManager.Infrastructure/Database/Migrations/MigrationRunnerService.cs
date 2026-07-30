using FluentMigrator.Runner;
using Microsoft.Extensions.Logging;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public sealed class MigrationRunnerService
{
    private readonly IMigrationRunner _runner;
    private readonly MigrationDatabaseInspector _inspector;
    private readonly ILogger<MigrationRunnerService> _logger;

    public MigrationRunnerService(
        IMigrationRunner runner,
        MigrationDatabaseInspector inspector,
        ILogger<MigrationRunnerService> logger)
    {
        _runner = runner;
        _inspector = inspector;
        _logger = logger;
    }

    public Task<MigrationStatus> GetStatusAsync(
        CancellationToken cancellationToken)
    {
        return _inspector.InspectAsync(cancellationToken);
    }

    public async Task<MigrationExecutionResult> MigrateAsync(
        string? confirmation,
        CancellationToken cancellationToken)
    {
        MigrationStatus before;

        try
        {
            before = await _inspector.InspectAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            MigrationRunnerLog.PreflightFailed(
                _logger,
                exception.GetType().Name);

            return new MigrationExecutionResult(
                false,
                "Migration preflight failed. No migrations were executed.");
        }

        string requiredConfirmation =
            $"MIGRATE {before.DatabaseName}";

        if (!string.Equals(
                confirmation,
                requiredConfirmation,
                StringComparison.Ordinal))
        {
            MigrationRunnerLog.ConfirmationMismatch(_logger);

            return new MigrationExecutionResult(
                false,
                $"Confirmation must be exactly: {requiredConfirmation}",
                before);
        }

        string? blockReason =
            MigrationSafetyGuard.GetBlockReason(before);

        if (blockReason is not null)
        {
            MigrationRunnerLog.SafetyPreflightBlocked(
                _logger,
                before.DatabaseName);

            return new MigrationExecutionResult(
                false,
                blockReason,
                before);
        }

        if (before.PendingMigrations.Count == 0)
        {
            MigrationRunnerLog.NoPendingMigrations(
                _logger,
                before.DatabaseName);

            return new MigrationExecutionResult(
                true,
                "No pending migrations were found.",
                before);
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            MigrationRunnerLog.ExecutionStarted(
                _logger,
                before.DatabaseName,
                before.PendingMigrations.Count);

            foreach (MigrationDescriptor pendingMigration
                in before.PendingMigrations)
            {
                MigrationRunnerLog.PendingVersion(
                    _logger,
                    pendingMigration.Version);
            }
        }

        try
        {
            _runner.MigrateUp();

            MigrationStatus after =
                await _inspector.InspectAsync(cancellationToken);

            if (after.PendingMigrations.Count > 0)
            {
                MigrationRunnerLog.PendingMigrationsRemain(
                    _logger,
                    after.DatabaseName);

                return new MigrationExecutionResult(
                    false,
                    "Migration execution completed with pending migrations remaining.",
                    after);
            }

            MigrationRunnerLog.ExecutionSucceeded(
                _logger,
                after.DatabaseName,
                after.HighestAppliedVersion);

            return new MigrationExecutionResult(
                true,
                "All pending migrations were applied successfully.",
                after);
        }
        catch (Exception exception)
        {
            MigrationRunnerLog.ExecutionFailed(
                _logger,
                before.DatabaseName,
                exception.GetType().Name);

            return new MigrationExecutionResult(
                false,
                "Migration execution failed. Review the safe migration log.",
                before);
        }
    }
}

internal static partial class MigrationRunnerLog
{
    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Error,
        Message =
            "Migration preflight failed. Error type: {ErrorType}.")]
    public static partial void PreflightFailed(
        ILogger logger,
        string errorType);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message =
            "Migration confirmation did not match the target database.")]
    public static partial void ConfirmationMismatch(
        ILogger logger);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message =
            "Migration safety preflight blocked execution for database {DatabaseName}.")]
    public static partial void SafetyPreflightBlocked(
        ILogger logger,
        string databaseName);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Information,
        Message =
            "Migration check completed for database {DatabaseName}; no pending migrations were found.")]
    public static partial void NoPendingMigrations(
        ILogger logger,
        string databaseName);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Information,
        Message =
            "Migration execution started for database {DatabaseName}. Pending migration count: {PendingCount}.")]
    public static partial void ExecutionStarted(
        ILogger logger,
        string databaseName,
        int pendingCount);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Information,
        Message = "Pending migration version: {PendingVersion}.")]
    public static partial void PendingVersion(
        ILogger logger,
        long pendingVersion);

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Error,
        Message =
            "Migration execution did not apply every pending migration for database {DatabaseName}.")]
    public static partial void PendingMigrationsRemain(
        ILogger logger,
        string databaseName);

    [LoggerMessage(
        EventId = 2007,
        Level = LogLevel.Information,
        Message =
            "Migration execution succeeded for database {DatabaseName}. Highest applied version: {HighestAppliedVersion}.")]
    public static partial void ExecutionSucceeded(
        ILogger logger,
        string databaseName,
        long? highestAppliedVersion);

    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Error,
        Message =
            "Migration execution failed for database {DatabaseName}. Error type: {ErrorType}.")]
    public static partial void ExecutionFailed(
        ILogger logger,
        string databaseName,
        string errorType);
}
