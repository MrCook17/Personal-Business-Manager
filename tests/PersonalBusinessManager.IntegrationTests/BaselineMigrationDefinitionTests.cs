using System.Text.RegularExpressions;
using PersonalBusinessManager.Infrastructure.Database.Migrations;

namespace PersonalBusinessManager.IntegrationTests;

public sealed partial class BaselineMigrationDefinitionTests
{
    private static readonly string[] SchemaMigrationSql =
    [
        BaselineMigrationSql.Migration0001,
        BaselineMigrationSql.Migration0002,
        BaselineMigrationSql.Migration0003,
        BaselineMigrationSql.Migration0004,
        BaselineMigrationSql.Migration0005,
        BaselineMigrationSql.Migration0006,
        BaselineMigrationSql.Migration0007,
        BaselineMigrationSql.Migration0008,
        BaselineMigrationSql.Migration0009,
        BaselineMigrationSql.Migration0010,
        BaselineMigrationSql.Migration0012,
    ];

    [Fact]
    public void CatalogContainsTheApprovedThirteenMigrationSequence()
    {
        var catalog = new MigrationCatalog();

        Assert.Equal(
            Enumerable.Range(1, 13).Select(version => (long)version),
            catalog.Migrations.Select(migration => migration.Version));
        Assert.All(
            catalog.Migrations,
            migration => Assert.False(
                string.IsNullOrWhiteSpace(migration.Description)));
    }

    [Fact]
    public void MigrationSchemaExactlyMatchesApprovedBootstrapDefinitions()
    {
        string approvedBootstrap = ReadApprovedBootstrap();

        string[] expected = ExtractStatements(
                approvedBootstrap,
                "CREATE TABLE",
                "CREATE INDEX")
            .Select(NormalizeDefinition)
            .Order(StringComparer.Ordinal)
            .ToArray();

        string[] actual = SchemaMigrationSql
            .SelectMany(sql => ExtractStatements(
                sql,
                "CREATE TABLE",
                "CREATE INDEX"))
            .Select(NormalizeDefinition)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MigrationSchemaPreservesApprovedObjectCountsAndConventions()
    {
        string combined = string.Join(
            Environment.NewLine,
            SchemaMigrationSql);
        string[] tables = ExtractStatements(
            combined,
            "CREATE TABLE");
        string[] indexes = ExtractStatements(
            combined,
            "CREATE INDEX");

        Assert.Equal(31, tables.Length);
        Assert.Equal(59, indexes.Length);
        Assert.Equal(31, PrimaryKeyRegex().Count(combined));
        Assert.Equal(18, UniqueConstraintRegex().Count(combined));
        Assert.Equal(56, ForeignKeyRegex().Count(combined));
        Assert.Equal(116, CheckConstraintRegex().Count(combined));

        Assert.All(
            tables,
            table =>
            {
                Assert.Contains(
                    "`record_id`",
                    table,
                    StringComparison.Ordinal);
                Assert.Contains(
                    "PRIMARY KEY (`record_id`)",
                    table,
                    StringComparison.Ordinal);
                Assert.Contains(
                    "ENGINE=InnoDB",
                    table,
                    StringComparison.Ordinal);
                Assert.Contains(
                    "DEFAULT CHARACTER SET=utf8mb4",
                    table,
                    StringComparison.Ordinal);
                Assert.Contains(
                    "COLLATE=utf8mb4_unicode_ci",
                    table,
                    StringComparison.Ordinal);
            });

        Assert.All(
            ExtractObjectNames(tables, CreateTableNameRegex()),
            AssertLowerSnakeCase);
        Assert.All(
            ExtractObjectNames(indexes, CreateIndexNameRegex()),
            AssertLowerSnakeCase);

        Assert.DoesNotContain(
            "IF NOT EXISTS",
            combined,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SeedsMatchApprovedBootstrapAndRemainIdempotent()
    {
        string approvedBootstrap = ReadApprovedBootstrap();

        Dictionary<string, string> expected =
            ExtractStatements(approvedBootstrap, "INSERT INTO")
                .ToDictionary(
                    GetInsertTableName,
                    NormalizeDefinition,
                    StringComparer.Ordinal);

        Dictionary<string, string> actual =
            ExtractStatements(
                    BaselineMigrationSql.Migration0011
                    + Environment.NewLine
                    + BaselineMigrationSql.Migration0013,
                    "INSERT INTO")
                .ToDictionary(
                    GetInsertTableName,
                    NormalizeDefinition,
                    StringComparer.Ordinal);

        Assert.Equal(
            expected.Keys
                .Where(key => key != "schema_information")
                .Order(StringComparer.Ordinal),
            actual.Keys.Order(StringComparer.Ordinal));

        foreach ((string tableName, string statement) in actual)
        {
            Assert.Equal(expected[tableName], statement);
            Assert.Contains(
                "ON DUPLICATE KEY UPDATE",
                statement,
                StringComparison.Ordinal);
        }

        Assert.Contains(
            "VALUES (1, '1.0.0', '1.0.0', 2, NULL, UTC_TIMESTAMP(6))",
            BaselineMigrationSql.Migration0002,
            StringComparison.Ordinal);
        Assert.Contains(
            "`schema_version` = VALUES(`schema_version`)",
            BaselineMigrationSql.Migration0002,
            StringComparison.Ordinal);
    }

    [Fact]
    public void MigrationSqlContainsNoAccountOrCredentialStatements()
    {
        string combined = string.Join(
            Environment.NewLine,
            SchemaMigrationSql.Append(
                BaselineMigrationSql.Migration0011)
                .Append(BaselineMigrationSql.Migration0013));

        Assert.DoesNotContain(
            "CREATE USER",
            combined,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "IDENTIFIED BY",
            combined,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "GRANT ",
            combined,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SecondaryIndexRollbackDropsEveryCreatedIndexInReverseOrder()
    {
        string[] createdNames = ExtractObjectNames(
            ExtractStatements(
                BaselineMigrationSql.Migration0012,
                "CREATE INDEX"),
            CreateIndexNameRegex());
        string[] droppedNames = ExtractObjectNames(
            ExtractStatements(
                BaselineMigrationSql.Migration0012Down,
                "DROP INDEX"),
            DropIndexNameRegex());

        Assert.Equal(
            createdNames.Reverse(),
            droppedNames);
    }

    private static string ReadApprovedBootstrap()
    {
        DirectoryInfo? directory =
            new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string path = Path.Combine(
                directory.FullName,
                "docs",
                "personal_business_management_application_schema.sql");

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "Could not locate the approved bootstrap schema.");
    }

    private static string[] ExtractStatements(
        string sql,
        params string[] prefixes)
    {
        string[] lines = sql.Replace(
                "\r\n",
                "\n",
                StringComparison.Ordinal)
            .Split('\n');
        var statements = new List<string>();

        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index].TrimStart();

            if (!prefixes.Any(prefix =>
                    line.StartsWith(
                        prefix,
                        StringComparison.Ordinal)))
            {
                continue;
            }

            var statementLines =
                new List<string> { lines[index] };

            while (!statementLines[^1]
                .TrimEnd()
                .EndsWith(';'))
            {
                index++;

                if (index >= lines.Length)
                {
                    throw new InvalidDataException(
                        "A SQL definition is unterminated.");
                }

                statementLines.Add(lines[index]);
            }

            statements.Add(string.Join(
                "\n",
                statementLines));
        }

        return [.. statements];
    }

    private static string NormalizeDefinition(string statement)
    {
        return statement
            .Replace(
                "CREATE TABLE IF NOT EXISTS",
                "CREATE TABLE",
                StringComparison.Ordinal)
            .Replace(
                "CREATE INDEX IF NOT EXISTS",
                "CREATE INDEX",
                StringComparison.Ordinal)
            .Replace(
                "\r\n",
                "\n",
                StringComparison.Ordinal)
            .Trim();
    }

    private static string[] ExtractObjectNames(
        IEnumerable<string> statements,
        Regex regex)
    {
        return statements
            .Select(statement =>
            {
                Match match = regex.Match(statement);

                Assert.True(
                    match.Success,
                    $"Could not read a database object name from: {statement}");

                return match.Groups[1].Value;
            })
            .ToArray();
    }

    private static string GetInsertTableName(string statement)
    {
        Match match = InsertTableNameRegex().Match(statement);

        Assert.True(
            match.Success,
            $"Could not read an INSERT table name from: {statement}");

        return match.Groups[1].Value;
    }

    private static void AssertLowerSnakeCase(string name)
    {
        Assert.Matches(
            "^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$",
            name);
    }

    [GeneratedRegex(@"CONSTRAINT `pk_")]
    private static partial Regex PrimaryKeyRegex();

    [GeneratedRegex(@"CONSTRAINT `uq_")]
    private static partial Regex UniqueConstraintRegex();

    [GeneratedRegex(@"CONSTRAINT `fk_")]
    private static partial Regex ForeignKeyRegex();

    [GeneratedRegex(@"CONSTRAINT `chk_")]
    private static partial Regex CheckConstraintRegex();

    [GeneratedRegex(@"^CREATE TABLE `([^`]+)`")]
    private static partial Regex CreateTableNameRegex();

    [GeneratedRegex(@"^CREATE INDEX `([^`]+)`")]
    private static partial Regex CreateIndexNameRegex();

    [GeneratedRegex(@"^DROP INDEX `([^`]+)`")]
    private static partial Regex DropIndexNameRegex();

    [GeneratedRegex(@"^INSERT INTO `([^`]+)`")]
    private static partial Regex InsertTableNameRegex();
}
