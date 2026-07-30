using Dapper;
using MySqlConnector;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public sealed class MigrationDatabaseInspector
{
    private const string MigrationHistoryTable = "schema_migrations";
    private const string SchemaInformationTable = "schema_information";

    private readonly string _connectionString;
    private readonly MigrationCatalog _catalog;

    public MigrationDatabaseInspector(
        string connectionString,
        MigrationCatalog catalog)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
        _catalog = catalog;
    }

    public async Task<MigrationStatus> InspectAsync(
        CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string identitySql =
            """
            SELECT
                @@hostname AS ServerHost,
                DATABASE() AS DatabaseName,
                VERSION() AS ServerVersion,
                CURRENT_USER() AS DatabaseAccount;
            """;

        DatabaseIdentity identity =
            await connection.QuerySingleAsync<DatabaseIdentity>(
                new CommandDefinition(
                    identitySql,
                    cancellationToken: cancellationToken));

        bool historyTableExists = await TableExistsAsync(
            connection,
            MigrationHistoryTable,
            cancellationToken);

        IReadOnlyList<long> appliedVersions =
            historyTableExists
                ? (await connection.QueryAsync<long>(
                    new CommandDefinition(
                        """
                        SELECT `version`
                        FROM `schema_migrations`
                        ORDER BY `version`;
                        """,
                        cancellationToken: cancellationToken))).AsList()
                : [];

        bool schemaInformationExists = await TableExistsAsync(
            connection,
            SchemaInformationTable,
            cancellationToken);

        long? schemaInformationVersion =
            schemaInformationExists
                ? await connection.QueryFirstOrDefaultAsync<long?>(
                    new CommandDefinition(
                        """
                        SELECT `schema_version`
                        FROM `schema_information`
                        WHERE `record_id` = 1
                        LIMIT 1;
                        """,
                        cancellationToken: cancellationToken))
                : null;

        const string applicationTableCountSql =
            """
            SELECT COUNT(*)
            FROM information_schema.tables
            WHERE table_schema = DATABASE()
              AND table_type = 'BASE TABLE'
              AND table_name <> @MigrationHistoryTable;
            """;

        int applicationTableCount =
            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    applicationTableCountSql,
                    new
                    {
                        MigrationHistoryTable,
                    },
                    cancellationToken: cancellationToken));

        HashSet<long> appliedVersionSet = appliedVersions.ToHashSet();

        IReadOnlyList<MigrationDescriptor> pendingMigrations =
            _catalog.Migrations
                .Where(migration =>
                    !appliedVersionSet.Contains(migration.Version))
                .ToList();

        return new MigrationStatus(
            identity.ServerHost,
            identity.DatabaseName,
            identity.ServerVersion,
            identity.DatabaseAccount,
            historyTableExists,
            applicationTableCount,
            _catalog.Migrations,
            appliedVersions,
            pendingMigrations,
            schemaInformationVersion);
    }

    private static async Task<bool> TableExistsAsync(
        MySqlConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT EXISTS(
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = DATABASE()
                  AND table_name = @TableName
                  AND table_type = 'BASE TABLE');
            """;

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new
                {
                    TableName = tableName,
                },
                cancellationToken: cancellationToken));
    }

    private sealed class DatabaseIdentity
    {
        public required string ServerHost { get; init; }

        public required string DatabaseName { get; init; }

        public required string ServerVersion { get; init; }

        public required string DatabaseAccount { get; init; }
    }
}
