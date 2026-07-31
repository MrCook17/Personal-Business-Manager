using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PersonalBusinessManager.Infrastructure.Database.Migrations;

namespace PersonalBusinessManager.DatabaseMigrator;

internal static class Program
{
    public static async Task<int> Main(string[] arguments)
    {
        MigrationCommandParseResult parseResult =
            MigrationCommandOptions.Parse(arguments);

        if (parseResult.ShowHelp)
        {
            WriteUsage();
            return 0;
        }

        if (!parseResult.IsSuccessful
            || parseResult.Options is null)
        {
            Console.Error.WriteLine(parseResult.Error);
            WriteUsage();
            return 2;
        }

        string? migrationConnectionString =
            ResolveEnvironmentVariable(
                parseResult.Options
                    .ConnectionEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(
                migrationConnectionString))
        {
            Console.Error.WriteLine(
                "The selected migration connection environment "
                + "variable is absent or empty.");

            return 2;
        }

        var services = new ServiceCollection();

        services.AddLogging(logging => logging
            .SetMinimumLevel(LogLevel.Information)
            .AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffK ";
            }));

        services.AddDatabaseMigrations(
            migrationConnectionString);

        await using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        ILogger logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigrator");

        try
        {
            MigrationRunnerService runner =
                serviceProvider.GetRequiredService<
                    MigrationRunnerService>();
            BaselineSchemaVerifier verifier =
                serviceProvider.GetRequiredService<
                    BaselineSchemaVerifier>();
            BaselineRegistrationService baselineService =
                serviceProvider.GetRequiredService<
                    BaselineRegistrationService>();

            using var cancellationSource =
                new CancellationTokenSource();

            Console.CancelKeyPress += (_, eventArguments) =>
            {
                eventArguments.Cancel = true;
                cancellationSource.Cancel();
            };

            return parseResult.Options.Command switch
            {
                MigrationCommand.Status =>
                    await ReportStatusAsync(
                        runner,
                        logger,
                        cancellationSource.Token),

                MigrationCommand.VerifyBaseline =>
                    await VerifyAsync(
                        verifier,
                        logger,
                        baselineEligibility: true,
                        cancellationSource.Token),

                MigrationCommand.Migrate =>
                    await MigrateAsync(
                        runner,
                        logger,
                        parseResult.Options.Confirmation,
                        cancellationSource.Token),

                MigrationCommand.BaselineExisting =>
                    await BaselineExistingAsync(
                        baselineService,
                        logger,
                        parseResult.Options,
                        cancellationSource.Token),

                MigrationCommand.Verify =>
                    await VerifyAsync(
                        verifier,
                        logger,
                        baselineEligibility: false,
                        cancellationSource.Token),

                _ => 2,
            };
        }
        catch (OperationCanceledException)
        {
            DatabaseMigratorLog.CommandCancelled(logger);
            return 3;
        }
        catch (Exception exception)
        {
            DatabaseMigratorLog.CommandFailed(
                logger,
                exception.GetType().Name);

            return 1;
        }
    }

    private static async Task<int> ReportStatusAsync(
        MigrationRunnerService runner,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        DatabaseMigratorLog.StatusStarted(logger);

        MigrationStatus status =
            await runner.GetStatusAsync(cancellationToken);

        DatabaseMigratorLog.TargetIdentity(
            logger,
            status.ServerHost,
            status.DatabaseName,
            status.DatabaseAccount,
            status.ServerVersion);

        DatabaseMigratorLog.VersionSummary(
            logger,
            status.MigrationHistoryTableExists,
            status.ApplicationTableCount,
            status.HighestAppliedVersion,
            status.SchemaInformationVersion);

        if (logger.IsEnabled(LogLevel.Information))
        {
            if (status.AppliedVersions.Count == 0)
            {
                DatabaseMigratorLog.NoAppliedVersions(logger);
            }
            else
            {
                foreach (long appliedVersion
                    in status.AppliedVersions)
                {
                    DatabaseMigratorLog.AppliedVersion(
                        logger,
                        appliedVersion);
                }
            }

            if (status.PendingMigrations.Count == 0)
            {
                DatabaseMigratorLog.NoPendingVersions(logger);
            }
            else
            {
                foreach (MigrationDescriptor pendingMigration
                    in status.PendingMigrations)
                {
                    DatabaseMigratorLog.PendingVersion(
                        logger,
                        pendingMigration.Version,
                        pendingMigration.Description);
                }
            }
        }

        DatabaseMigratorLog.VersionAgreement(
            logger,
            status.HistoryAndSchemaInformationAgree);

        DatabaseMigratorLog.StatusSucceeded(logger);

        return 0;
    }

    private static async Task<int> MigrateAsync(
        MigrationRunnerService runner,
        ILogger logger,
        string? confirmation,
        CancellationToken cancellationToken)
    {
        MigrationExecutionResult result =
            await runner.MigrateAsync(
                confirmation,
                cancellationToken);

        if (result.Succeeded)
        {
            DatabaseMigratorLog.ResultSucceeded(
                logger,
                result.Message);

            return 0;
        }

        DatabaseMigratorLog.ResultFailed(
            logger,
            result.Message);

        return 1;
    }

    private static async Task<int> VerifyAsync(
        BaselineSchemaVerifier verifier,
        ILogger logger,
        bool baselineEligibility,
        CancellationToken cancellationToken)
    {
        DatabaseMigratorLog.VerificationStarted(
            logger,
            baselineEligibility
                ? "baseline eligibility"
                : "current schema");

        BaselineVerificationResult result =
            baselineEligibility
                ? await verifier.VerifyBaselineEligibilityAsync(
                    cancellationToken)
                : await verifier.VerifyCurrentAsync(
                    cancellationToken);

        ReportVerification(logger, result);

        if (result.Succeeded)
        {
            DatabaseMigratorLog.VerificationSucceeded(logger);
            return 0;
        }

        DatabaseMigratorLog.VerificationFailed(
            logger,
            result.Problems.Count);

        return 1;
    }

    private static async Task<int> BaselineExistingAsync(
        BaselineRegistrationService baselineService,
        ILogger logger,
        MigrationCommandOptions options,
        CancellationToken cancellationToken)
    {
        DatabaseMigratorLog.BaselineStarted(
            logger,
            options.TargetVersion
                ?? BaselineSchemaManifest.Version);

        BaselineRegistrationResult result =
            await baselineService.RegisterExistingAsync(
                options.TargetVersion
                    ?? BaselineSchemaManifest.Version,
                options.Confirmation,
                options.BackupPath,
                options.BackupSha256,
                cancellationToken);

        if (result.Verification is not null)
        {
            ReportVerification(
                logger,
                result.Verification);
        }

        if (result.Backup is not null)
        {
            DatabaseMigratorLog.BackupVerified(
                logger,
                result.Backup.FileName,
                result.Backup.SizeBytes,
                result.Backup.Sha256);
        }

        DatabaseMigratorLog.BuildIdentifier(
            logger,
            result.BuildIdentifier);

        if (logger.IsEnabled(LogLevel.Information))
        {
            foreach (long version in result.RegisteredVersions)
            {
                DatabaseMigratorLog.BaselineVersionRegistered(
                    logger,
                    version);
            }
        }

        if (logger.IsEnabled(LogLevel.Information)
            && result.BeforeData is not null
            && result.AfterData is not null)
        {
            bool dataEqual = string.Equals(
                result.BeforeData.FingerprintSha256,
                result.AfterData.FingerprintSha256,
                StringComparison.Ordinal);

            DatabaseMigratorLog.DataComparison(
                logger,
                result.BeforeData.FingerprintSha256,
                result.AfterData.FingerprintSha256,
                dataEqual);
        }

        if (result.Succeeded)
        {
            DatabaseMigratorLog.ResultSucceeded(
                logger,
                result.Message);
            return 0;
        }

        DatabaseMigratorLog.ResultFailed(
            logger,
            result.Message);
        return 1;
    }

    private static void ReportVerification(
        ILogger logger,
        BaselineVerificationResult result)
    {
        DatabaseMigratorLog.TargetIdentity(
            logger,
            result.ServerHost,
            result.DatabaseName,
            result.DatabaseAccount,
            result.ServerVersion);
        DatabaseMigratorLog.SchemaFingerprint(
            logger,
            result.Schema.FingerprintSha256,
            result.Schema.MetadataRecordCount,
            result.Schema.ApplicationTableCount,
            result.Schema.CheckConstraintCount);
        DatabaseMigratorLog.DataFingerprint(
            logger,
            result.Data.FingerprintSha256,
            result.Data.TableCount,
            result.Data.TotalRows);

        if (logger.IsEnabled(LogLevel.Warning))
        {
            foreach (string problem in result.Problems)
            {
                DatabaseMigratorLog.VerificationProblem(
                    logger,
                    problem);
            }
        }
    }

    private static string? ResolveEnvironmentVariable(
        string variableName)
    {
        string? processValue =
            Environment.GetEnvironmentVariable(
                variableName,
                EnvironmentVariableTarget.Process);

        if (!string.IsNullOrWhiteSpace(processValue)
            || !OperatingSystem.IsWindows())
        {
            return processValue;
        }

        return Environment.GetEnvironmentVariable(
            variableName,
            EnvironmentVariableTarget.User);
    }

    private static void WriteUsage()
    {
        Console.WriteLine(
            """
            Personal Business Manager database migrator

            Read-only status:
              dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- status --connection-env PBM_MIGRATION_CONNECTION_STRING

            Read-only baseline eligibility verification:
              dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- verify-baseline --connection-env PBM_MIGRATION_CONNECTION_STRING

            Apply pending migrations:
              dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- migrate --connection-env PBM_MIGRATION_CONNECTION_STRING --confirm "MIGRATE <database_name>"

            Register a verified existing version-13 schema:
              dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- baseline-existing --connection-env PBM_MIGRATION_CONNECTION_STRING --to 13 --backup-path "<verified-backup.sql>" --backup-sha256 "<sha256>" --confirm "BASELINE <database_name> TO 13"

            Read-only current-schema verification:
              dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- verify --connection-env PBM_MIGRATION_CONNECTION_STRING

            The connection value is read from the explicitly named environment
            variable. A raw connection string is not accepted on the command line.
            Normal WinForms startup never invokes this tool.
            """);
    }
}

internal static partial class DatabaseMigratorLog
{
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Warning,
        Message = "Migration command was cancelled.")]
    public static partial void CommandCancelled(
        ILogger logger);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Error,
        Message =
            "Migration command failed. Error type: {ErrorType}.")]
    public static partial void CommandFailed(
        ILogger logger,
        string errorType);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Read-only migration status check started.")]
    public static partial void StatusStarted(
        ILogger logger);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Information,
        Message =
            "Target server {ServerHost}; database {DatabaseName}; account {DatabaseAccount}; server version {ServerVersion}.")]
    public static partial void TargetIdentity(
        ILogger logger,
        string serverHost,
        string databaseName,
        string databaseAccount,
        string serverVersion);

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Information,
        Message =
            "Migration history table present: {HistoryPresent}; application tables: {ApplicationTableCount}; highest applied version: {HighestAppliedVersion}; schema-information version: {SchemaInformationVersion}.")]
    public static partial void VersionSummary(
        ILogger logger,
        bool historyPresent,
        int applicationTableCount,
        long? highestAppliedVersion,
        long? schemaInformationVersion);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Information,
        Message = "No applied migration versions were found.")]
    public static partial void NoAppliedVersions(
        ILogger logger);

    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Information,
        Message = "Applied migration version: {AppliedVersion}.")]
    public static partial void AppliedVersion(
        ILogger logger,
        long appliedVersion);

    [LoggerMessage(
        EventId = 3007,
        Level = LogLevel.Information,
        Message = "No pending migration versions were found.")]
    public static partial void NoPendingVersions(
        ILogger logger);

    [LoggerMessage(
        EventId = 3008,
        Level = LogLevel.Information,
        Message =
            "Pending migration version {PendingVersion}: {Description}.")]
    public static partial void PendingVersion(
        ILogger logger,
        long pendingVersion,
        string description);

    [LoggerMessage(
        EventId = 3009,
        Level = LogLevel.Information,
        Message =
            "History and schema-information versions agree: {VersionsAgree}.")]
    public static partial void VersionAgreement(
        ILogger logger,
        bool versionsAgree);

    [LoggerMessage(
        EventId = 3010,
        Level = LogLevel.Information,
        Message = "Read-only migration status check succeeded.")]
    public static partial void StatusSucceeded(
        ILogger logger);

    [LoggerMessage(
        EventId = 3011,
        Level = LogLevel.Information,
        Message = "{ResultMessage}")]
    public static partial void ResultSucceeded(
        ILogger logger,
        string resultMessage);

    [LoggerMessage(
        EventId = 3012,
        Level = LogLevel.Error,
        Message = "{ResultMessage}")]
    public static partial void ResultFailed(
        ILogger logger,
        string resultMessage);

    [LoggerMessage(
        EventId = 3013,
        Level = LogLevel.Information,
        Message = "Read-only {VerificationKind} verification started.")]
    public static partial void VerificationStarted(
        ILogger logger,
        string verificationKind);

    [LoggerMessage(
        EventId = 3014,
        Level = LogLevel.Information,
        Message =
            "Application-schema fingerprint {Fingerprint}; metadata records: {MetadataRecordCount}; application tables: {ApplicationTableCount}; checks: {CheckConstraintCount}.")]
    public static partial void SchemaFingerprint(
        ILogger logger,
        string fingerprint,
        int metadataRecordCount,
        int applicationTableCount,
        int checkConstraintCount);

    [LoggerMessage(
        EventId = 3015,
        Level = LogLevel.Information,
        Message =
            "Application-data summary fingerprint {Fingerprint}; tables: {TableCount}; total rows: {TotalRows}.")]
    public static partial void DataFingerprint(
        ILogger logger,
        string fingerprint,
        int tableCount,
        long totalRows);

    [LoggerMessage(
        EventId = 3016,
        Level = LogLevel.Warning,
        Message = "Verification problem: {Problem}")]
    public static partial void VerificationProblem(
        ILogger logger,
        string problem);

    [LoggerMessage(
        EventId = 3017,
        Level = LogLevel.Information,
        Message = "Schema verification succeeded.")]
    public static partial void VerificationSucceeded(
        ILogger logger);

    [LoggerMessage(
        EventId = 3018,
        Level = LogLevel.Error,
        Message =
            "Schema verification failed with {ProblemCount} problem(s).")]
    public static partial void VerificationFailed(
        ILogger logger,
        int problemCount);

    [LoggerMessage(
        EventId = 3019,
        Level = LogLevel.Information,
        Message =
            "Controlled baseline registration started for target version {TargetVersion}.")]
    public static partial void BaselineStarted(
        ILogger logger,
        int targetVersion);

    [LoggerMessage(
        EventId = 3020,
        Level = LogLevel.Information,
        Message =
            "Verified backup file {BackupFileName}; size: {SizeBytes} bytes; SHA-256: {Sha256}.")]
    public static partial void BackupVerified(
        ILogger logger,
        string backupFileName,
        long sizeBytes,
        string sha256);

    [LoggerMessage(
        EventId = 3021,
        Level = LogLevel.Information,
        Message =
            "Migration assembly build identifier: {BuildIdentifier}.")]
    public static partial void BuildIdentifier(
        ILogger logger,
        string buildIdentifier);

    [LoggerMessage(
        EventId = 3022,
        Level = LogLevel.Information,
        Message =
            "Registered existing baseline migration version {Version}; no Up method was executed.")]
    public static partial void BaselineVersionRegistered(
        ILogger logger,
        long version);

    [LoggerMessage(
        EventId = 3023,
        Level = LogLevel.Information,
        SkipEnabledCheck = true,
        Message =
            "Before data fingerprint: {BeforeFingerprint}; after data fingerprint: {AfterFingerprint}; equal: {Equal}.")]
    public static partial void DataComparison(
        ILogger logger,
        string beforeFingerprint,
        string afterFingerprint,
        bool equal);
}
