namespace PersonalBusinessManager.DatabaseMigrator;

public enum MigrationCommand
{
    Status,
    VerifyBaseline,
    Migrate,
    ResetTestDatabase,
    BaselineExisting,
    Verify,
}

public sealed record MigrationCommandOptions(
    MigrationCommand Command,
    string ConnectionEnvironmentVariable,
    string? Confirmation,
    int? TargetVersion,
    string? BackupPath,
    string? BackupSha256)
{
    private static readonly HashSet<string>
        ApprovedConnectionEnvironmentVariables =
        [
            "PBM_MIGRATION_CONNECTION_STRING",
            "PBM_TEST_MIGRATION_CONNECTION_STRING",
        ];

    public static MigrationCommandParseResult Parse(
        IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        if (arguments.Count == 0)
        {
            return MigrationCommandParseResult.Failure(
                "An explicit command is required.");
        }

        if (arguments[0] is "--help" or "-h" or "help")
        {
            return MigrationCommandParseResult.Help();
        }

        MigrationCommand? command = arguments[0] switch
        {
            "status" => MigrationCommand.Status,
            "verify-baseline" =>
                MigrationCommand.VerifyBaseline,
            "migrate" => MigrationCommand.Migrate,
            "reset-test" =>
                MigrationCommand.ResetTestDatabase,
            "baseline-existing" =>
                MigrationCommand.BaselineExisting,
            "verify" => MigrationCommand.Verify,
            _ => null,
        };

        if (command is null)
        {
            return MigrationCommandParseResult.Failure(
                $"Unknown command: {arguments[0]}");
        }

        string? connectionEnvironmentVariable = null;
        string? confirmation = null;
        int? targetVersion = null;
        string? backupPath = null;
        string? backupSha256 = null;

        for (int index = 1; index < arguments.Count; index += 2)
        {
            if (index + 1 >= arguments.Count)
            {
                return MigrationCommandParseResult.Failure(
                    $"A value is required for option {arguments[index]}.");
            }

            string option = arguments[index];
            string value = arguments[index + 1];

            switch (option)
            {
                case "--connection-env"
                    when connectionEnvironmentVariable is null:
                    connectionEnvironmentVariable = value;
                    break;

                case "--confirm" when confirmation is null:
                    confirmation = value;
                    break;

                case "--to" when targetVersion is null:
                    if (!int.TryParse(
                            value,
                            out int parsedVersion))
                    {
                        return MigrationCommandParseResult.Failure(
                            "--to must be an integer migration version.");
                    }

                    targetVersion = parsedVersion;
                    break;

                case "--backup-path" when backupPath is null:
                    backupPath = value;
                    break;

                case "--backup-sha256"
                    when backupSha256 is null:
                    backupSha256 = value;
                    break;

                case "--connection-env":
                case "--confirm":
                case "--to":
                case "--backup-path":
                case "--backup-sha256":
                    return MigrationCommandParseResult.Failure(
                        $"Option {option} may only be supplied once.");

                default:
                    return MigrationCommandParseResult.Failure(
                        $"Unknown option: {option}");
            }
        }

        if (string.IsNullOrWhiteSpace(
                connectionEnvironmentVariable))
        {
            return MigrationCommandParseResult.Failure(
                "An explicit --connection-env option is required.");
        }

        if (!ApprovedConnectionEnvironmentVariables.Contains(
                connectionEnvironmentVariable))
        {
            return MigrationCommandParseResult.Failure(
                "--connection-env must name "
                + "PBM_MIGRATION_CONNECTION_STRING or "
                + "PBM_TEST_MIGRATION_CONNECTION_STRING.");
        }

        if ((command is MigrationCommand.Status
                or MigrationCommand.VerifyBaseline
                or MigrationCommand.Verify)
            && (confirmation is not null
                || targetVersion is not null
                || backupPath is not null
                || backupSha256 is not null))
        {
            return MigrationCommandParseResult.Failure(
                "The selected read-only command accepts only --connection-env.");
        }

        if ((command is MigrationCommand.Migrate
                or MigrationCommand.ResetTestDatabase)
            && string.IsNullOrWhiteSpace(confirmation))
        {
            return MigrationCommandParseResult.Failure(
                $"The {arguments[0]} command requires an explicit --confirm value.");
        }

        if ((command is MigrationCommand.Migrate
                or MigrationCommand.ResetTestDatabase)
            && (targetVersion is not null
                || backupPath is not null
                || backupSha256 is not null))
        {
            return MigrationCommandParseResult.Failure(
                "Backup and baseline options are only valid for baseline-existing.");
        }

        if (command == MigrationCommand.ResetTestDatabase
            && !string.Equals(
                connectionEnvironmentVariable,
                TestDatabaseSafetyGuard
                    .MigrationConnectionEnvironmentVariable,
                StringComparison.Ordinal))
        {
            return MigrationCommandParseResult.Failure(
                "reset-test requires --connection-env "
                + TestDatabaseSafetyGuard
                    .MigrationConnectionEnvironmentVariable
                + ".");
        }

        if (command == MigrationCommand.BaselineExisting)
        {
            if (string.IsNullOrWhiteSpace(confirmation))
            {
                return MigrationCommandParseResult.Failure(
                    "baseline-existing requires an explicit --confirm value.");
            }

            if (targetVersion != 13)
            {
                return MigrationCommandParseResult.Failure(
                    "baseline-existing requires --to 13.");
            }

            if (string.IsNullOrWhiteSpace(backupPath))
            {
                return MigrationCommandParseResult.Failure(
                    "baseline-existing requires --backup-path.");
            }

            if (string.IsNullOrWhiteSpace(backupSha256))
            {
                return MigrationCommandParseResult.Failure(
                    "baseline-existing requires --backup-sha256.");
            }
        }

        return MigrationCommandParseResult.Success(
            new MigrationCommandOptions(
                command.Value,
                connectionEnvironmentVariable,
                confirmation,
                targetVersion,
                backupPath,
                backupSha256));
    }
}

public sealed record MigrationCommandParseResult(
    bool IsSuccessful,
    bool ShowHelp,
    MigrationCommandOptions? Options,
    string? Error)
{
    public static MigrationCommandParseResult Success(
        MigrationCommandOptions options)
    {
        return new MigrationCommandParseResult(
            true,
            false,
            options,
            null);
    }

    public static MigrationCommandParseResult Failure(
        string error)
    {
        return new MigrationCommandParseResult(
            false,
            false,
            null,
            error);
    }

    public static MigrationCommandParseResult Help()
    {
        return new MigrationCommandParseResult(
            false,
            true,
            null,
            null);
    }
}
