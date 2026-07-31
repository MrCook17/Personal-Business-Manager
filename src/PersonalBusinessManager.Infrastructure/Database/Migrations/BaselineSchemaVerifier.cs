using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using MySqlConnector;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public sealed class BaselineSchemaVerifier
{
    private const string MigrationHistoryTable = "schema_migrations";

    private static readonly IReadOnlyDictionary<
        string,
        RequiredAccountType> RequiredAccountTypes =
        new Dictionary<string, RequiredAccountType>(
            StringComparer.Ordinal)
        {
            ["current_account"] = new("asset", false),
            ["savings_account"] = new("asset", false),
            ["regular_saver"] = new("asset", false),
            ["fixed_rate_saver"] = new("asset", false),
            ["cash_isa"] = new("asset", true),
            ["stocks_shares_isa"] = new("asset", true),
            ["lifetime_isa"] = new("asset", true),
            ["investment_account"] = new("asset", false),
            ["pension"] = new("asset", true),
            ["cash"] = new("asset", false),
            ["other_asset"] = new("asset", false),
            ["credit_card"] = new("liability", false),
            ["overdraft"] = new("liability", false),
            ["personal_loan"] = new("liability", false),
            ["student_loan"] = new("liability", false),
            ["mortgage"] = new("liability", false),
            ["other_liability"] = new("liability", false),
        };

    private static readonly IReadOnlyDictionary<string, string>
        RequiredSettings =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["locale"] = "string",
            ["default_currency_code"] = "string",
            ["default_country_code"] = "string",
            ["theme"] = "string",
            ["default_hourly_rate"] = "decimal",
            ["default_payment_terms_days"] = "integer",
            ["business_vat_registered"] = "boolean",
            ["vat_registration_number"] = "string",
            ["default_vat_rate"] = "decimal",
            ["prices_include_vat_by_default"] = "boolean",
            ["default_time_rounding_rule"] = "string",
            ["forgotten_timer_warning_minutes"] = "integer",
            ["inactivity_lock_minutes"] = "integer",
            ["tax_reserve_percentage"] = "decimal",
            ["automatic_backup_on_first_launch"] = "boolean",
            ["backup_retention_daily_count"] = "integer",
            ["backup_retention_weekly_count"] = "integer",
            ["backup_retention_monthly_count"] = "integer",
        };

    private readonly string _connectionString;
    private readonly MigrationCatalog _catalog;

    public BaselineSchemaVerifier(
        string connectionString,
        MigrationCatalog catalog)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(catalog);

        _connectionString = connectionString;
        _catalog = catalog;
    }

    public Task<BaselineVerificationResult>
        VerifyBaselineEligibilityAsync(
            CancellationToken cancellationToken)
    {
        return VerifyAsync(
            VerificationMode.BaselineEligibility,
            cancellationToken);
    }

    public Task<BaselineVerificationResult> VerifyCurrentAsync(
        CancellationToken cancellationToken)
    {
        return VerifyAsync(
            VerificationMode.Current,
            cancellationToken);
    }

    public async Task<BaselineDataSnapshot> CaptureDataSnapshotAsync(
        CancellationToken cancellationToken)
    {
        await using var connection =
            new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await CaptureDataSnapshotAsync(
            connection,
            cancellationToken);
    }

    private async Task<BaselineVerificationResult> VerifyAsync(
        VerificationMode mode,
        CancellationToken cancellationToken)
    {
        await using var connection =
            new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        DatabaseIdentity identity =
            await ReadIdentityAsync(
                connection,
                cancellationToken);
        BaselineSchemaSnapshot schema =
            await CaptureSchemaSnapshotAsync(
                connection,
                cancellationToken);
        BaselineDataSnapshot data =
            await CaptureDataSnapshotAsync(
                connection,
                cancellationToken);
        var problems = new List<string>();

        problems.AddRange(GetManifestProblems(schema));
        await VerifyVersionStateAsync(
            connection,
            mode,
            problems,
            cancellationToken);
        await VerifyRequiredAndObsoleteColumnsAsync(
            connection,
            problems,
            cancellationToken);
        await VerifySeedsAsync(
            connection,
            problems,
            cancellationToken);
        await VerifyForeignKeysAsync(
            connection,
            problems,
            cancellationToken);
        await VerifyCheckConstraintsAsync(
            connection,
            problems,
            cancellationToken);

        return new BaselineVerificationResult(
            problems.Count == 0,
            identity.ServerHost,
            identity.DatabaseName,
            identity.ServerVersion,
            identity.DatabaseAccount,
            schema,
            data,
            problems);
    }

    private static async Task<DatabaseIdentity> ReadIdentityAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                @@hostname AS ServerHost,
                DATABASE() AS DatabaseName,
                VERSION() AS ServerVersion,
                CURRENT_USER() AS DatabaseAccount;
            """;

        return await connection.QuerySingleAsync<DatabaseIdentity>(
            new CommandDefinition(
                sql,
                cancellationToken: cancellationToken));
    }

    private static async Task<BaselineSchemaSnapshot>
        CaptureSchemaSnapshotAsync(
            MySqlConnection connection,
            CancellationToken cancellationToken)
    {
        const string tablesSql =
            """
            SELECT
                table_name AS TableName,
                engine AS Engine,
                table_collation AS TableCollation
            FROM information_schema.tables
            WHERE table_schema = DATABASE()
              AND table_type = 'BASE TABLE'
              AND table_name <> 'schema_migrations';
            """;
        const string columnsSql =
            """
            SELECT
                table_name AS TableName,
                ordinal_position AS OrdinalPosition,
                column_name AS ColumnName,
                column_type AS ColumnType,
                is_nullable AS IsNullable,
                column_default AS ColumnDefault,
                extra AS Extra,
                character_set_name AS CharacterSetName,
                collation_name AS CollationName,
                generation_expression AS GenerationExpression
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
              AND table_name <> 'schema_migrations';
            """;
        const string constraintsSql =
            """
            SELECT
                table_name AS TableName,
                constraint_name AS ConstraintName,
                constraint_type AS ConstraintType
            FROM information_schema.table_constraints
            WHERE constraint_schema = DATABASE()
              AND table_name <> 'schema_migrations';
            """;
        const string checksSql =
            """
            SELECT
                table_name AS TableName,
                constraint_name AS ConstraintName,
                check_clause AS CheckClause
            FROM information_schema.check_constraints
            WHERE constraint_schema = DATABASE()
              AND table_name <> 'schema_migrations';
            """;
        const string foreignKeysSql =
            """
            SELECT
                kcu.table_name AS TableName,
                kcu.constraint_name AS ConstraintName,
                kcu.ordinal_position AS OrdinalPosition,
                kcu.column_name AS ColumnName,
                kcu.referenced_table_name AS ReferencedTableName,
                kcu.referenced_column_name AS ReferencedColumnName,
                rc.update_rule AS UpdateRule,
                rc.delete_rule AS DeleteRule
            FROM information_schema.key_column_usage AS kcu
            INNER JOIN information_schema.referential_constraints AS rc
                ON rc.constraint_schema = kcu.constraint_schema
               AND rc.constraint_name = kcu.constraint_name
            WHERE kcu.constraint_schema = DATABASE()
              AND kcu.table_name <> 'schema_migrations'
              AND kcu.referenced_table_name IS NOT NULL;
            """;
        const string indexesSql =
            """
            SELECT
                table_name AS TableName,
                index_name AS IndexName,
                non_unique AS NonUnique,
                seq_in_index AS SequenceInIndex,
                column_name AS ColumnName,
                collation AS Collation,
                sub_part AS SubPart,
                index_type AS IndexType
            FROM information_schema.statistics
            WHERE table_schema = DATABASE()
              AND table_name <> 'schema_migrations';
            """;

        List<TableMetadata> tables =
            (await connection.QueryAsync<TableMetadata>(
                new CommandDefinition(
                    tablesSql,
                    cancellationToken: cancellationToken))).AsList();
        List<ColumnMetadata> columns =
            (await connection.QueryAsync<ColumnMetadata>(
                new CommandDefinition(
                    columnsSql,
                    cancellationToken: cancellationToken))).AsList();
        List<ConstraintMetadata> constraints =
            (await connection.QueryAsync<ConstraintMetadata>(
                new CommandDefinition(
                    constraintsSql,
                    cancellationToken: cancellationToken))).AsList();
        List<CheckMetadata> checks =
            (await connection.QueryAsync<CheckMetadata>(
                new CommandDefinition(
                    checksSql,
                    cancellationToken: cancellationToken))).AsList();
        List<ForeignKeyMetadata> foreignKeys =
            (await connection.QueryAsync<ForeignKeyMetadata>(
                new CommandDefinition(
                    foreignKeysSql,
                    cancellationToken: cancellationToken))).AsList();
        List<IndexMetadata> indexes =
            (await connection.QueryAsync<IndexMetadata>(
                new CommandDefinition(
                    indexesSql,
                    cancellationToken: cancellationToken))).AsList();

        var records = new List<string>(
            tables.Count
            + columns.Count
            + constraints.Count
            + checks.Count
            + foreignKeys.Count
            + indexes.Count);

        records.AddRange(tables.Select(table => CanonicalRecord(
            "table",
            table.TableName,
            table.Engine,
            table.TableCollation)));
        records.AddRange(columns.Select(column => CanonicalRecord(
            "column",
            column.TableName,
            column.OrdinalPosition.ToString(
                "D4",
                CultureInfo.InvariantCulture),
            column.ColumnName,
            column.ColumnType,
            column.IsNullable,
            column.ColumnDefault,
            column.Extra,
            column.CharacterSetName,
            column.CollationName,
            column.GenerationExpression)));
        records.AddRange(constraints.Select(constraint =>
            CanonicalRecord(
                "constraint",
                constraint.TableName,
                constraint.ConstraintName,
                constraint.ConstraintType)));
        records.AddRange(checks.Select(check => CanonicalRecord(
            "check",
            check.TableName,
            check.ConstraintName,
            check.CheckClause)));
        records.AddRange(foreignKeys.Select(foreignKey =>
            CanonicalRecord(
                "foreign_key",
                foreignKey.TableName,
                foreignKey.ConstraintName,
                foreignKey.OrdinalPosition.ToString(
                    "D4",
                    CultureInfo.InvariantCulture),
                foreignKey.ColumnName,
                foreignKey.ReferencedTableName,
                foreignKey.ReferencedColumnName,
                foreignKey.UpdateRule,
                foreignKey.DeleteRule)));
        records.AddRange(indexes.Select(index => CanonicalRecord(
            "index",
            index.TableName,
            index.IndexName,
            index.NonUnique.ToString(
                CultureInfo.InvariantCulture),
            index.SequenceInIndex.ToString(
                "D4",
                CultureInfo.InvariantCulture),
            index.ColumnName,
            index.Collation,
            index.SubPart?.ToString(
                CultureInfo.InvariantCulture),
            index.IndexType)));

        string fingerprint = ComputeFingerprint(records);

        return new BaselineSchemaSnapshot(
            fingerprint,
            tables.Count,
            columns.Count,
            constraints.Count,
            checks.Count,
            foreignKeys.Count,
            indexes.Count,
            records.Count);
    }

    private static async Task<BaselineDataSnapshot>
        CaptureDataSnapshotAsync(
            MySqlConnection connection,
            CancellationToken cancellationToken)
    {
        const string tableNamesSql =
            """
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = DATABASE()
              AND table_type = 'BASE TABLE'
              AND table_name <> 'schema_migrations'
            ORDER BY table_name;
            """;

        List<string> tableNames =
            (await connection.QueryAsync<string>(
                new CommandDefinition(
                    tableNamesSql,
                    cancellationToken: cancellationToken))).AsList();
        var rowCounts =
            new SortedDictionary<string, long>(
                StringComparer.Ordinal);

        foreach (string tableName in tableNames)
        {
            string sql =
                $"SELECT COUNT(*) FROM {QuoteIdentifier(tableName)};";
            long rowCount =
                await connection.ExecuteScalarAsync<long>(
                    new CommandDefinition(
                        sql,
                        cancellationToken: cancellationToken));
            rowCounts.Add(tableName, rowCount);
        }

        const string totalsSql =
            """
            SELECT
                COALESCE((SELECT SUM(`net_total`) FROM `invoices`), 0)
                    AS InvoiceNetTotal,
                COALESCE((SELECT SUM(`vat_total`) FROM `invoices`), 0)
                    AS InvoiceVatTotal,
                COALESCE((SELECT SUM(`gross_total`) FROM `invoices`), 0)
                    AS InvoiceGrossTotal,
                COALESCE((SELECT SUM(`amount_paid`) FROM `invoices`), 0)
                    AS InvoiceAmountPaid,
                COALESCE((SELECT SUM(`outstanding_amount`) FROM `invoices`), 0)
                    AS InvoiceOutstandingAmount,
                COALESCE((SELECT SUM(`amount`) FROM `invoice_payments`), 0)
                    AS InvoicePaymentTotal,
                COALESCE((SELECT SUM(
                    CASE WHEN `is_reversed` = 0 THEN `amount` ELSE 0 END)
                    FROM `invoice_payments`), 0)
                    AS ActiveInvoicePaymentTotal,
                COALESCE((SELECT SUM(`net_amount`) FROM `expenses`), 0)
                    AS ExpenseNetTotal,
                COALESCE((SELECT SUM(`vat_amount`) FROM `expenses`), 0)
                    AS ExpenseVatTotal,
                COALESCE((SELECT SUM(`gross_amount`) FROM `expenses`), 0)
                    AS ExpenseGrossTotal,
                COALESCE((SELECT SUM(`current_balance`)
                    FROM `financial_accounts`), 0)
                    AS AccountCurrentBalanceTotal,
                COALESCE((SELECT SUM(`balance_amount`)
                    FROM `financial_account_balance_snapshots`), 0)
                    AS BalanceSnapshotTotal,
                COALESCE((SELECT SUM(`raw_duration_seconds`)
                    FROM `time_entries`), 0)
                    AS RawDurationSecondsTotal,
                COALESCE((SELECT SUM(`rounded_duration_seconds`)
                    FROM `time_entries`), 0)
                    AS RoundedDurationSecondsTotal;
            """;

        FinancialTotals totals =
            await connection.QuerySingleAsync<FinancialTotals>(
                new CommandDefinition(
                    totalsSql,
                    cancellationToken: cancellationToken));
        var financialTotals =
            new SortedDictionary<string, decimal>(
                StringComparer.Ordinal)
            {
                ["account_current_balance_total"] =
                    totals.AccountCurrentBalanceTotal,
                ["active_invoice_payment_total"] =
                    totals.ActiveInvoicePaymentTotal,
                ["balance_snapshot_total"] =
                    totals.BalanceSnapshotTotal,
                ["expense_gross_total"] =
                    totals.ExpenseGrossTotal,
                ["expense_net_total"] =
                    totals.ExpenseNetTotal,
                ["expense_vat_total"] =
                    totals.ExpenseVatTotal,
                ["invoice_amount_paid"] =
                    totals.InvoiceAmountPaid,
                ["invoice_gross_total"] =
                    totals.InvoiceGrossTotal,
                ["invoice_net_total"] =
                    totals.InvoiceNetTotal,
                ["invoice_outstanding_amount"] =
                    totals.InvoiceOutstandingAmount,
                ["invoice_payment_total"] =
                    totals.InvoicePaymentTotal,
                ["invoice_vat_total"] =
                    totals.InvoiceVatTotal,
                ["raw_duration_seconds_total"] =
                    totals.RawDurationSecondsTotal,
                ["rounded_duration_seconds_total"] =
                    totals.RoundedDurationSecondsTotal,
            };
        var canonicalRecords = new List<string>();

        canonicalRecords.AddRange(rowCounts.Select(pair =>
            CanonicalRecord(
                "row_count",
                pair.Key,
                pair.Value.ToString(
                    CultureInfo.InvariantCulture))));
        canonicalRecords.AddRange(financialTotals.Select(pair =>
            CanonicalRecord(
                "financial_total",
                pair.Key,
                pair.Value.ToString(
                    "0.############################",
                    CultureInfo.InvariantCulture))));

        return new BaselineDataSnapshot(
            ComputeFingerprint(canonicalRecords),
            rowCounts.Count,
            rowCounts.Values.Sum(),
            rowCounts,
            financialTotals);
    }

    internal static IReadOnlyList<string> GetManifestProblems(
        BaselineSchemaSnapshot schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        var problems = new List<string>();

        CompareCount(
            "application table",
            BaselineSchemaManifest.ApplicationTableCount,
            schema.ApplicationTableCount,
            problems);
        CompareCount(
            "column",
            BaselineSchemaManifest.ColumnCount,
            schema.ColumnCount,
            problems);
        CompareCount(
            "constraint",
            BaselineSchemaManifest.ConstraintCount,
            schema.ConstraintCount,
            problems);
        CompareCount(
            "check constraint",
            BaselineSchemaManifest.CheckConstraintCount,
            schema.CheckConstraintCount,
            problems);
        CompareCount(
            "foreign-key column",
            BaselineSchemaManifest.ForeignKeyColumnCount,
            schema.ForeignKeyColumnCount,
            problems);
        CompareCount(
            "index column",
            BaselineSchemaManifest.IndexColumnCount,
            schema.IndexColumnCount,
            problems);
        CompareCount(
            "metadata record",
            BaselineSchemaManifest.MetadataRecordCount,
            schema.MetadataRecordCount,
            problems);

        if (!string.Equals(
                schema.FingerprintSha256,
                BaselineSchemaManifest.FingerprintSha256,
                StringComparison.Ordinal))
        {
            problems.Add(
                "The normalized application-schema fingerprint does not match the approved version-13 manifest.");
        }

        return problems;
    }

    private async Task VerifyVersionStateAsync(
        MySqlConnection connection,
        VerificationMode mode,
        List<string> problems,
        CancellationToken cancellationToken)
    {
        bool historyExists = await TableExistsAsync(
            connection,
            MigrationHistoryTable,
            cancellationToken);
        List<long> appliedVersions = historyExists
            ? (await connection.QueryAsync<long>(
                new CommandDefinition(
                    """
                    SELECT `version`
                    FROM `schema_migrations`
                    ORDER BY `version`;
                    """,
                    cancellationToken: cancellationToken))).AsList()
            : [];

        if (mode == VerificationMode.BaselineEligibility)
        {
            if (appliedVersions.Count > 0)
            {
                problems.Add(
                    "Migration history is not empty; baseline registration is a one-time operation.");
            }
        }
        else
        {
            long[] expectedVersions = _catalog.Migrations
                .Where(migration =>
                    migration.Version
                    <= BaselineSchemaManifest.Version)
                .Select(migration => migration.Version)
                .ToArray();

            if (!appliedVersions.SequenceEqual(expectedVersions))
            {
                problems.Add(
                    "Migration history does not contain exactly the approved baseline versions 1 through 13.");
            }
        }

        const string schemaInformationSql =
            """
            SELECT
                COUNT(*) AS RowCount,
                COALESCE(MAX(`schema_version`), 0)
                    AS SchemaVersion
            FROM `schema_information`;
            """;
        SchemaInformationState schemaInformation =
            await connection.QuerySingleAsync<SchemaInformationState>(
                new CommandDefinition(
                    schemaInformationSql,
                    cancellationToken: cancellationToken));

        if (schemaInformation.RowCount != 1)
        {
            problems.Add(
                "schema_information must contain exactly one row.");
        }
        else if (mode == VerificationMode.BaselineEligibility
            && (schemaInformation.SchemaVersion < 1
                || schemaInformation.SchemaVersion
                    > BaselineSchemaManifest.Version))
        {
            problems.Add(
                "schema_information contains a version that is not eligible for version-13 baseline registration.");
        }
        else if (mode == VerificationMode.Current
            && schemaInformation.SchemaVersion
                != BaselineSchemaManifest.Version)
        {
            problems.Add(
                "schema_information.schema_version is not 13.");
        }
    }

    private static async Task VerifyRequiredAndObsoleteColumnsAsync(
        MySqlConnection connection,
        List<string> problems,
        CancellationToken cancellationToken)
    {
        var requirements = new[]
        {
            new ColumnRequirement(
                "time_entries",
                "rounded_duration_seconds",
                true,
                false),
            new ColumnRequirement(
                "audit_records",
                "user_id",
                true,
                true),
            new ColumnRequirement(
                "expenses",
                "payment_method_code",
                true,
                false),
            new ColumnRequirement(
                "time_entries",
                "rounded_duration_minutes",
                false,
                false),
            new ColumnRequirement(
                "invoice_time_entries",
                "billed_minutes",
                false,
                false),
            new ColumnRequirement(
                "tasks",
                "recurrence_definition_id",
                false,
                false),
        };

        const string sql =
            """
            SELECT
                COUNT(*) AS RowCount,
                COALESCE(MAX(`is_nullable`), '') AS IsNullable
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
              AND table_name = @TableName
              AND column_name = @ColumnName;
            """;

        foreach (ColumnRequirement requirement in requirements)
        {
            ColumnState state =
                await connection.QuerySingleAsync<ColumnState>(
                    new CommandDefinition(
                        sql,
                        new
                        {
                            requirement.TableName,
                            requirement.ColumnName,
                        },
                        cancellationToken: cancellationToken));

            if (requirement.MustExist && state.RowCount != 1)
            {
                problems.Add(
                    $"Required column {requirement.TableName}.{requirement.ColumnName} is missing.");
            }
            else if (!requirement.MustExist && state.RowCount != 0)
            {
                problems.Add(
                    $"Obsolete column {requirement.TableName}.{requirement.ColumnName} is present.");
            }
            else if (requirement.MustExist
                && requirement.MustBeNullable
                && !string.Equals(
                    state.IsNullable,
                    "YES",
                    StringComparison.Ordinal))
            {
                problems.Add(
                    $"Required column {requirement.TableName}.{requirement.ColumnName} is not nullable.");
            }
        }
    }

    private static async Task VerifySeedsAsync(
        MySqlConnection connection,
        List<string> problems,
        CancellationToken cancellationToken)
    {
        List<AccountTypeSeed> accountTypes =
            (await connection.QueryAsync<AccountTypeSeed>(
                new CommandDefinition(
                    """
                    SELECT
                        `account_type_code` AS AccountTypeCode,
                        `classification_code` AS ClassificationCode,
                        `is_tax_wrapper` AS IsTaxWrapper
                    FROM `financial_account_types`;
                    """,
                    cancellationToken: cancellationToken))).AsList();
        Dictionary<string, AccountTypeSeed> accountTypesByCode =
            accountTypes.ToDictionary(
                accountType => accountType.AccountTypeCode,
                StringComparer.Ordinal);

        foreach ((string code, RequiredAccountType required)
            in RequiredAccountTypes)
        {
            if (!accountTypesByCode.TryGetValue(
                    code,
                    out AccountTypeSeed? actual))
            {
                problems.Add(
                    $"Required financial-account type {code} is missing.");
                continue;
            }

            if (!string.Equals(
                    actual.ClassificationCode,
                    required.ClassificationCode,
                    StringComparison.Ordinal)
                || actual.IsTaxWrapper
                    != required.IsTaxWrapper)
            {
                problems.Add(
                    $"Financial-account type {code} does not match the approved classification.");
            }
        }

        int uncategorisedCount =
            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    """
                    SELECT COUNT(*)
                    FROM `expense_categories`
                    WHERE `category_name` = 'Uncategorised';
                    """,
                    cancellationToken: cancellationToken));

        if (uncategorisedCount != 1)
        {
            problems.Add(
                "The required Uncategorised expense category is missing or duplicated.");
        }

        List<SequenceSeed> sequences =
            (await connection.QueryAsync<SequenceSeed>(
                new CommandDefinition(
                    """
                    SELECT
                        `sequence_code` AS SequenceCode,
                        `number_prefix` AS NumberPrefix,
                        `next_number` AS NextNumber
                    FROM `invoice_number_sequences`
                    WHERE `sequence_code` IN ('invoice', 'credit_note')
                      AND `sequence_year` = 0;
                    """,
                    cancellationToken: cancellationToken))).AsList();

        foreach (string code in new[] { "invoice", "credit_note" })
        {
            SequenceSeed? sequence = sequences.SingleOrDefault(
                candidate => string.Equals(
                    candidate.SequenceCode,
                    code,
                    StringComparison.Ordinal));

            if (sequence is null)
            {
                problems.Add(
                    $"Required invoice sequence {code} is missing.");
            }
            else if (string.IsNullOrWhiteSpace(
                    sequence.NumberPrefix)
                || sequence.NextNumber < 1)
            {
                problems.Add(
                    $"Invoice sequence {code} has an invalid prefix or next number.");
            }
        }

        List<ApplicationSettingSeed> settings =
            (await connection.QueryAsync<ApplicationSettingSeed>(
                new CommandDefinition(
                    """
                    SELECT
                        `setting_key` AS SettingKey,
                        `setting_value` AS SettingValue,
                        `value_type_code` AS ValueTypeCode,
                        `is_sensitive` AS IsSensitive
                    FROM `application_settings`;
                    """,
                    cancellationToken: cancellationToken))).AsList();
        Dictionary<string, ApplicationSettingSeed> settingsByKey =
            settings.ToDictionary(
                setting => setting.SettingKey,
                StringComparer.Ordinal);

        foreach ((string key, string expectedType)
            in RequiredSettings)
        {
            if (!settingsByKey.TryGetValue(
                    key,
                    out ApplicationSettingSeed? setting))
            {
                problems.Add(
                    $"Required application setting {key} is missing.");
                continue;
            }

            if (!string.Equals(
                    setting.ValueTypeCode,
                    expectedType,
                    StringComparison.Ordinal)
                || setting.IsSensitive)
            {
                problems.Add(
                    $"Application setting {key} has an invalid type or sensitivity flag.");
                continue;
            }

            if (!SettingValueIsValid(
                    setting.SettingValue,
                    expectedType))
            {
                problems.Add(
                    $"Application setting {key} has a value incompatible with its approved type.");
            }
        }
    }

    private static async Task VerifyForeignKeysAsync(
        MySqlConnection connection,
        List<string> problems,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                kcu.table_name AS TableName,
                kcu.constraint_name AS ConstraintName,
                kcu.ordinal_position AS OrdinalPosition,
                kcu.column_name AS ColumnName,
                kcu.referenced_table_name AS ReferencedTableName,
                kcu.referenced_column_name AS ReferencedColumnName,
                rc.update_rule AS UpdateRule,
                rc.delete_rule AS DeleteRule
            FROM information_schema.key_column_usage AS kcu
            INNER JOIN information_schema.referential_constraints AS rc
                ON rc.constraint_schema = kcu.constraint_schema
               AND rc.constraint_name = kcu.constraint_name
            WHERE kcu.constraint_schema = DATABASE()
              AND kcu.table_name <> 'schema_migrations'
              AND kcu.referenced_table_name IS NOT NULL
            ORDER BY
                kcu.table_name,
                kcu.constraint_name,
                kcu.ordinal_position;
            """;
        List<ForeignKeyMetadata> foreignKeys =
            (await connection.QueryAsync<ForeignKeyMetadata>(
                new CommandDefinition(
                    sql,
                    cancellationToken: cancellationToken))).AsList();

        foreach (IGrouping<string, ForeignKeyMetadata> group
            in foreignKeys.GroupBy(
                foreignKey =>
                    foreignKey.TableName
                    + "\u001f"
                    + foreignKey.ConstraintName,
                StringComparer.Ordinal))
        {
            ForeignKeyMetadata first = group.First();
            string join = string.Join(
                " AND ",
                group.Select(pair =>
                    $"child.{QuoteIdentifier(pair.ColumnName)} = parent.{QuoteIdentifier(pair.ReferencedColumnName)}"));
            string nonNull = string.Join(
                " AND ",
                group.Select(pair =>
                    $"child.{QuoteIdentifier(pair.ColumnName)} IS NOT NULL"));
            string query =
                $"""
                SELECT COUNT(*)
                FROM {QuoteIdentifier(first.TableName)} AS child
                LEFT JOIN {QuoteIdentifier(first.ReferencedTableName)}
                    AS parent ON {join}
                WHERE {nonNull}
                  AND parent.{QuoteIdentifier(first.ReferencedColumnName)}
                      IS NULL;
                """;
            long orphanCount =
                await connection.ExecuteScalarAsync<long>(
                    new CommandDefinition(
                        query,
                        cancellationToken: cancellationToken));

            if (orphanCount > 0)
            {
                problems.Add(
                    $"Foreign key {first.ConstraintName} has {orphanCount} orphaned row(s).");
            }
        }
    }

    private static async Task VerifyCheckConstraintsAsync(
        MySqlConnection connection,
        List<string> problems,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                table_name AS TableName,
                constraint_name AS ConstraintName,
                check_clause AS CheckClause
            FROM information_schema.check_constraints
            WHERE constraint_schema = DATABASE()
              AND table_name <> 'schema_migrations'
            ORDER BY table_name, constraint_name;
            """;
        List<CheckMetadata> checks =
            (await connection.QueryAsync<CheckMetadata>(
                new CommandDefinition(
                    sql,
                    cancellationToken: cancellationToken))).AsList();

        foreach (CheckMetadata check in checks)
        {
            string query =
                $"""
                SELECT COUNT(*)
                FROM {QuoteIdentifier(check.TableName)}
                WHERE NOT ({check.CheckClause})
                  AND ({check.CheckClause}) IS NOT NULL;
                """;
            long violationCount =
                await connection.ExecuteScalarAsync<long>(
                    new CommandDefinition(
                        query,
                        cancellationToken: cancellationToken));

            if (violationCount > 0)
            {
                problems.Add(
                    $"Check constraint {check.ConstraintName} has {violationCount} violating row(s).");
            }
        }
    }

    private static bool SettingValueIsValid(
        string? value,
        string valueType)
    {
        if (value is null)
        {
            return valueType == "string";
        }

        return valueType switch
        {
            "boolean" => value is "true" or "false",
            "integer" => long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out _),
            "decimal" => decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out _),
            "string" => true,
            _ => false,
        };
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

    private static void CompareCount(
        string name,
        int expected,
        int actual,
        List<string> problems)
    {
        if (expected != actual)
        {
            problems.Add(
                $"Expected {expected} {name} record(s), but found {actual}.");
        }
    }

    private static string CanonicalRecord(
        params object?[] values)
    {
        return string.Join(
            '|',
            values.Select(NormalizeValue));
    }

    private static string NormalizeValue(object? value)
    {
        if (value is null || value is DBNull)
        {
            return "<null>";
        }

        string text = Convert.ToString(
                value,
                CultureInfo.InvariantCulture)
            ?? string.Empty;
        text = Regex.Replace(
            text,
            @"\s+",
            " ",
            RegexOptions.CultureInvariant);

        return text
            .Trim()
            .Replace("`", string.Empty, StringComparison.Ordinal)
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .ToLowerInvariant();
    }

    private static string ComputeFingerprint(
        IEnumerable<string> records)
    {
        string canonical = string.Join(
            '\n',
            records.Order(StringComparer.Ordinal));
        byte[] hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(canonical));

        return Convert.ToHexString(hash)
            .ToLowerInvariant();
    }

    private static string QuoteIdentifier(string identifier)
    {
        return $"`{identifier.Replace(
            "`",
            "``",
            StringComparison.Ordinal)}`";
    }

    private enum VerificationMode
    {
        BaselineEligibility,
        Current,
    }

    private sealed record RequiredAccountType(
        string ClassificationCode,
        bool IsTaxWrapper);

    private sealed record ColumnRequirement(
        string TableName,
        string ColumnName,
        bool MustExist,
        bool MustBeNullable);

    private sealed class DatabaseIdentity
    {
        public required string ServerHost { get; init; }

        public required string DatabaseName { get; init; }

        public required string ServerVersion { get; init; }

        public required string DatabaseAccount { get; init; }
    }

    private sealed class TableMetadata
    {
        public required string TableName { get; init; }

        public string? Engine { get; init; }

        public string? TableCollation { get; init; }
    }

    private sealed class ColumnMetadata
    {
        public required string TableName { get; init; }

        public int OrdinalPosition { get; init; }

        public required string ColumnName { get; init; }

        public required string ColumnType { get; init; }

        public required string IsNullable { get; init; }

        public string? ColumnDefault { get; init; }

        public string? Extra { get; init; }

        public string? CharacterSetName { get; init; }

        public string? CollationName { get; init; }

        public string? GenerationExpression { get; init; }
    }

    private sealed class ConstraintMetadata
    {
        public required string TableName { get; init; }

        public required string ConstraintName { get; init; }

        public required string ConstraintType { get; init; }
    }

    private sealed class CheckMetadata
    {
        public required string TableName { get; init; }

        public required string ConstraintName { get; init; }

        public required string CheckClause { get; init; }
    }

    private sealed class ForeignKeyMetadata
    {
        public required string TableName { get; init; }

        public required string ConstraintName { get; init; }

        public int OrdinalPosition { get; init; }

        public required string ColumnName { get; init; }

        public required string ReferencedTableName { get; init; }

        public required string ReferencedColumnName { get; init; }

        public required string UpdateRule { get; init; }

        public required string DeleteRule { get; init; }
    }

    private sealed class IndexMetadata
    {
        public required string TableName { get; init; }

        public required string IndexName { get; init; }

        public int NonUnique { get; init; }

        public int SequenceInIndex { get; init; }

        public string? ColumnName { get; init; }

        public string? Collation { get; init; }

        public long? SubPart { get; init; }

        public required string IndexType { get; init; }
    }

    private sealed class SchemaInformationState
    {
        public int RowCount { get; init; }

        public long SchemaVersion { get; init; }
    }

    private sealed class ColumnState
    {
        public int RowCount { get; init; }

        public required string IsNullable { get; init; }
    }

    private sealed class AccountTypeSeed
    {
        public required string AccountTypeCode { get; init; }

        public required string ClassificationCode { get; init; }

        public bool IsTaxWrapper { get; init; }
    }

    private sealed class SequenceSeed
    {
        public required string SequenceCode { get; init; }

        public required string NumberPrefix { get; init; }

        public long NextNumber { get; init; }
    }

    private sealed class ApplicationSettingSeed
    {
        public required string SettingKey { get; init; }

        public string? SettingValue { get; init; }

        public required string ValueTypeCode { get; init; }

        public bool IsSensitive { get; init; }
    }

    private sealed class FinancialTotals
    {
        public decimal InvoiceNetTotal { get; init; }

        public decimal InvoiceVatTotal { get; init; }

        public decimal InvoiceGrossTotal { get; init; }

        public decimal InvoiceAmountPaid { get; init; }

        public decimal InvoiceOutstandingAmount { get; init; }

        public decimal InvoicePaymentTotal { get; init; }

        public decimal ActiveInvoicePaymentTotal { get; init; }

        public decimal ExpenseNetTotal { get; init; }

        public decimal ExpenseVatTotal { get; init; }

        public decimal ExpenseGrossTotal { get; init; }

        public decimal AccountCurrentBalanceTotal { get; init; }

        public decimal BalanceSnapshotTotal { get; init; }

        public decimal RawDurationSecondsTotal { get; init; }

        public decimal RoundedDurationSecondsTotal { get; init; }
    }
}
