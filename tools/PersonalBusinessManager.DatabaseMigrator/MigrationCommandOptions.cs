namespace PersonalBusinessManager.DatabaseMigrator;

public enum MigrationCommand
{
    Status,
    Migrate,
}

public sealed record MigrationCommandOptions(
    MigrationCommand Command,
    string ConnectionEnvironmentVariable,
    string? Confirmation)
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
            "migrate" => MigrationCommand.Migrate,
            _ => null,
        };

        if (command is null)
        {
            return MigrationCommandParseResult.Failure(
                $"Unknown command: {arguments[0]}");
        }

        string? connectionEnvironmentVariable = null;
        string? confirmation = null;

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

                case "--connection-env":
                case "--confirm":
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

        if (command == MigrationCommand.Status
            && confirmation is not null)
        {
            return MigrationCommandParseResult.Failure(
                "--confirm is only valid for the migrate command.");
        }

        if (command == MigrationCommand.Migrate
            && string.IsNullOrWhiteSpace(confirmation))
        {
            return MigrationCommandParseResult.Failure(
                "The migrate command requires an explicit --confirm value.");
        }

        return MigrationCommandParseResult.Success(
            new MigrationCommandOptions(
                command.Value,
                connectionEnvironmentVariable,
                confirmation));
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
