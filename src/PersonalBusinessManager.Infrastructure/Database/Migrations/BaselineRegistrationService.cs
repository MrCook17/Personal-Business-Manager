using System.Reflection;
using Dapper;
using MySqlConnector;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public sealed class BaselineRegistrationService
{
    private const string MigrationHistoryTable = "schema_migrations";

    private static readonly string[] ApprovedHistoryColumns =
    [
        "version",
        "applied_on_utc",
        "description",
    ];

    private readonly string _connectionString;
    private readonly MigrationCatalog _catalog;
    private readonly BaselineSchemaVerifier _verifier;

    public BaselineRegistrationService(
        string connectionString,
        MigrationCatalog catalog,
        BaselineSchemaVerifier verifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(verifier);

        _connectionString = connectionString;
        _catalog = catalog;
        _verifier = verifier;
    }

    public async Task<BaselineRegistrationResult>
        RegisterExistingAsync(
            int targetVersion,
            string? confirmation,
            string? backupPath,
            string? backupSha256,
            CancellationToken cancellationToken)
    {
        string buildIdentifier = GetBuildIdentifier();

        if (targetVersion != BaselineSchemaManifest.Version)
        {
            return Failure(
                "The baseline target must be exactly version 13.",
                buildIdentifier);
        }

        IReadOnlyList<MigrationDescriptor> baselineMigrations =
            SelectBaselineMigrations(_catalog);

        if (!baselineMigrations
            .Select(migration => migration.Version)
            .SequenceEqual(
                Enumerable.Range(
                        1,
                        BaselineSchemaManifest.Version)
                    .Select(version => (long)version)))
        {
            return Failure(
                "The migration assembly does not contain exactly baseline versions 1 through 13.",
                buildIdentifier);
        }

        BaselineVerificationResult preflight =
            await _verifier.VerifyBaselineEligibilityAsync(
                cancellationToken);

        if (!preflight.Succeeded)
        {
            return new BaselineRegistrationResult(
                false,
                "Baseline eligibility verification failed. No baseline action was performed.",
                preflight,
                null,
                [],
                preflight.Data,
                null,
                buildIdentifier);
        }

        string requiredConfirmation =
            $"BASELINE {preflight.DatabaseName} TO 13";

        if (!string.Equals(
                confirmation,
                requiredConfirmation,
                StringComparison.Ordinal))
        {
            return new BaselineRegistrationResult(
                false,
                $"Confirmation must be exactly: {requiredConfirmation}",
                preflight,
                null,
                [],
                preflight.Data,
                null,
                buildIdentifier);
        }

        (VerifiedBackupEvidence? backup, string? backupError) =
            await BackupEvidenceVerifier.VerifyAsync(
                backupPath,
                backupSha256,
                cancellationToken);

        if (backup is null)
        {
            return new BaselineRegistrationResult(
                false,
                backupError
                    ?? "Backup evidence verification failed.",
                preflight,
                null,
                [],
                preflight.Data,
                null,
                buildIdentifier);
        }

        await using var connection =
            new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        string lockName =
            $"personal_business_manager:baseline:{preflight.DatabaseName}";
        int lockAcquired =
            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "SELECT GET_LOCK(@LockName, 0);",
                    new
                    {
                        LockName = lockName,
                    },
                    cancellationToken: cancellationToken));

        if (lockAcquired != 1)
        {
            return new BaselineRegistrationResult(
                false,
                "Another baseline operation holds the target-database lock.",
                preflight,
                backup,
                [],
                preflight.Data,
                null,
                buildIdentifier);
        }

        var registeredVersions = new List<long>(
            baselineMigrations.Count);

        try
        {
            BaselineVerificationResult lockedPreflight =
                await _verifier.VerifyBaselineEligibilityAsync(
                    cancellationToken);

            if (!lockedPreflight.Succeeded
                || !string.Equals(
                    preflight.Schema.FingerprintSha256,
                    lockedPreflight.Schema.FingerprintSha256,
                    StringComparison.Ordinal)
                || !string.Equals(
                    preflight.Data.FingerprintSha256,
                    lockedPreflight.Data.FingerprintSha256,
                    StringComparison.Ordinal))
            {
                return new BaselineRegistrationResult(
                    false,
                    "The target changed between preflight and the locked write phase. No baseline action was performed.",
                    lockedPreflight,
                    backup,
                    [],
                    preflight.Data,
                    lockedPreflight.Data,
                    buildIdentifier);
            }

            bool historyExists = await TableExistsAsync(
                connection,
                MigrationHistoryTable,
                cancellationToken);

            if (historyExists)
            {
                long historyRows =
                    await connection.ExecuteScalarAsync<long>(
                        new CommandDefinition(
                            """
                            SELECT COUNT(*)
                            FROM `schema_migrations`;
                            """,
                            cancellationToken: cancellationToken));

                if (historyRows > 0)
                {
                    return new BaselineRegistrationResult(
                        false,
                        "Database already contains migration history. No baseline action was performed.",
                        preflight,
                        backup,
                        [],
                        preflight.Data,
                        null,
                        buildIdentifier);
                }

                if (!await EmptyHistoryTableHasApprovedShapeAsync(
                        connection,
                        cancellationToken))
                {
                    return new BaselineRegistrationResult(
                        false,
                        "The existing empty migration-history table does not have the approved shape.",
                        preflight,
                        backup,
                        [],
                        preflight.Data,
                        null,
                        buildIdentifier);
                }
            }
            else
            {
                await CreateHistoryTableAsync(
                    connection,
                    cancellationToken);
            }

            await using MySqlTransaction transaction =
                await connection.BeginTransactionAsync(
                    cancellationToken);

            try
            {
                const string insertSql =
                    """
                    INSERT INTO `schema_migrations` (
                        `version`,
                        `applied_on_utc`,
                        `description`)
                    VALUES (
                        @Version,
                        @AppliedOnUtc,
                        @Description);
                    """;
                DateTime appliedOnUtc = DateTime.UtcNow;

                foreach (MigrationDescriptor migration
                    in baselineMigrations)
                {
                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            insertSql,
                            new
                            {
                                migration.Version,
                                AppliedOnUtc = appliedOnUtc,
                                migration.Description,
                            },
                            transaction,
                            cancellationToken:
                                cancellationToken));
                    registeredVersions.Add(migration.Version);
                }

                int updatedRows =
                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            """
                            UPDATE `schema_information`
                            SET
                                `schema_version` = 13,
                                `date_updated_utc` =
                                    UTC_TIMESTAMP(6)
                            WHERE `record_id` = 1;
                            """,
                            transaction: transaction,
                            cancellationToken:
                                cancellationToken));

                if (updatedRows != 1)
                {
                    throw new InvalidOperationException(
                        "The schema-information singleton could not be updated.");
                }

                await transaction.CommitAsync(
                    cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(
                    cancellationToken);
                throw;
            }

            BaselineVerificationResult postRegistration =
                await _verifier.VerifyCurrentAsync(
                    cancellationToken);

            if (!postRegistration.Succeeded
                || !string.Equals(
                    preflight.Schema.FingerprintSha256,
                    postRegistration.Schema.FingerprintSha256,
                    StringComparison.Ordinal)
                || !string.Equals(
                    preflight.Data.FingerprintSha256,
                    postRegistration.Data.FingerprintSha256,
                    StringComparison.Ordinal))
            {
                return new BaselineRegistrationResult(
                    false,
                    "Post-baseline verification failed. Stop writes and restore the verified backup.",
                    postRegistration,
                    backup,
                    registeredVersions,
                    preflight.Data,
                    postRegistration.Data,
                    buildIdentifier);
            }

            int verifiedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    UPDATE `schema_information`
                    SET
                        `last_verified_utc` =
                            UTC_TIMESTAMP(6),
                        `date_updated_utc` =
                            UTC_TIMESTAMP(6)
                    WHERE `record_id` = 1
                      AND `schema_version` = 13;
                    """,
                    cancellationToken: cancellationToken));

            if (verifiedRows != 1)
            {
                return new BaselineRegistrationResult(
                    false,
                    "Baseline metadata was registered, but the final verification timestamp could not be recorded. Restore the verified backup.",
                    postRegistration,
                    backup,
                    registeredVersions,
                    preflight.Data,
                    postRegistration.Data,
                    buildIdentifier);
            }

            BaselineVerificationResult finalVerification =
                await _verifier.VerifyCurrentAsync(
                    cancellationToken);

            if (!finalVerification.Succeeded
                || !string.Equals(
                    preflight.Data.FingerprintSha256,
                    finalVerification.Data.FingerprintSha256,
                    StringComparison.Ordinal))
            {
                return new BaselineRegistrationResult(
                    false,
                    "Final baseline verification failed. Stop writes and restore the verified backup.",
                    finalVerification,
                    backup,
                    registeredVersions,
                    preflight.Data,
                    finalVerification.Data,
                    buildIdentifier);
            }

            return new BaselineRegistrationResult(
                true,
                "The existing version-13 schema was registered without executing migration Up methods.",
                finalVerification,
                backup,
                registeredVersions,
                preflight.Data,
                finalVerification.Data,
                buildIdentifier);
        }
        catch (Exception exception)
            when (exception is MySqlException
                or InvalidOperationException)
        {
            return new BaselineRegistrationResult(
                false,
                "Baseline registration failed after the write phase started. Stop writes, inspect migration history, and restore the verified backup if any metadata was committed.",
                preflight,
                backup,
                registeredVersions,
                preflight.Data,
                null,
                buildIdentifier);
        }
        finally
        {
            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "SELECT RELEASE_LOCK(@LockName);",
                    new
                    {
                        LockName = lockName,
                    },
                    cancellationToken:
                        CancellationToken.None));
        }
    }

    internal static IReadOnlyList<MigrationDescriptor>
        SelectBaselineMigrations(MigrationCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return SelectBaselineMigrations(catalog.Migrations);
    }

    internal static IReadOnlyList<MigrationDescriptor>
        SelectBaselineMigrations(
            IEnumerable<MigrationDescriptor> migrations)
    {
        ArgumentNullException.ThrowIfNull(migrations);

        return migrations
            .Where(migration =>
                migration.Version
                <= BaselineSchemaManifest.Version)
            .OrderBy(migration => migration.Version)
            .ToArray();
    }

    private static BaselineRegistrationResult Failure(
        string message,
        string buildIdentifier)
    {
        return new BaselineRegistrationResult(
            false,
            message,
            null,
            null,
            [],
            null,
            null,
            buildIdentifier);
    }

    private static async Task CreateHistoryTableAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                CREATE TABLE `schema_migrations` (
                    `version` BIGINT NOT NULL,
                    `applied_on_utc` DATETIME NULL,
                    `description` VARCHAR(1024) NULL,
                    CONSTRAINT `uq_schema_migrations_version`
                        PRIMARY KEY (`version`)
                ) ENGINE=InnoDB;
                """,
                cancellationToken: cancellationToken));

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                CREATE UNIQUE INDEX
                    `uq_schema_migrations_version`
                ON `schema_migrations` (`version` ASC);
                """,
                cancellationToken: cancellationToken));
    }

    private static async Task<bool>
        EmptyHistoryTableHasApprovedShapeAsync(
            MySqlConnection connection,
            CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                `column_name`
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
              AND table_name = 'schema_migrations'
            ORDER BY `ordinal_position`;
            """;
        List<string> columns =
            (await connection.QueryAsync<string>(
                new CommandDefinition(
                    sql,
                    cancellationToken: cancellationToken))).AsList();

        return columns.SequenceEqual(
            ApprovedHistoryColumns);
    }

    private static async Task<bool> TableExistsAsync(
        MySqlConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT EXISTS(
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                      AND table_name = @TableName
                      AND table_type = 'BASE TABLE');
                """,
                new
                {
                    TableName = tableName,
                },
                cancellationToken: cancellationToken));
    }

    private static string GetBuildIdentifier()
    {
        Assembly assembly = typeof(MigrationCatalog).Assembly;

        return assembly
            .GetCustomAttribute<
                AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "unknown";
    }
}
