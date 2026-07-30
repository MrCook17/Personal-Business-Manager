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

                MigrationCommand.Migrate =>
                    await MigrateAsync(
                        runner,
                        logger,
                        parseResult.Options.Confirmation,
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

            Apply pending migrations:
              dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- migrate --connection-env PBM_MIGRATION_CONNECTION_STRING --confirm "MIGRATE <database_name>"

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
}
