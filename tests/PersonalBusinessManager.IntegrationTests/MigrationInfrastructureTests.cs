using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using PersonalBusinessManager.DatabaseMigrator;
using PersonalBusinessManager.Infrastructure.Database.Migrations;

namespace PersonalBusinessManager.IntegrationTests;

public sealed class MigrationInfrastructureTests
{
    [Fact]
    public void VersionTableUsesApprovedSnakeCaseNames()
    {
        var versionTable = new SchemaMigrationVersionTable();

        Assert.Equal("schema_migrations", versionTable.TableName);
        Assert.Equal("version", versionTable.ColumnName);
        Assert.Equal(
            "applied_on_utc",
            versionTable.AppliedOnColumnName);
        Assert.Equal(
            "description",
            versionTable.DescriptionColumnName);
        Assert.Equal(
            "uq_schema_migrations_version",
            versionTable.UniqueIndexName);
        Assert.True(versionTable.CreateWithPrimaryKey);
        Assert.False(versionTable.OwnsSchema);
    }

    [Fact]
    public void CatalogReportsMigrationsInVersionOrder()
    {
        var catalog = new MigrationCatalog(
            Assembly.GetExecutingAssembly());

        Assert.Collection(
            catalog.Migrations,
            migration =>
            {
                Assert.Equal(41, migration.Version);
                Assert.Equal(
                    "Earlier test migration",
                    migration.Description);
            },
            migration =>
            {
                Assert.Equal(42, migration.Version);
                Assert.Equal(
                    "Later test migration",
                    migration.Description);
            });
    }

    [Fact]
    public void ServiceRegistrationAddsRunnerWithoutConnecting()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDatabaseMigrations(
            "Server=localhost;Database=pbm_test;"
            + "User ID=migration_test");

        using ServiceProvider provider =
            services.BuildServiceProvider();

        Assert.NotNull(
            provider.GetRequiredService<IMigrationRunner>());

        Assert.NotNull(
            provider.GetRequiredService<
                MigrationRunnerService>());
        Assert.NotNull(
            provider.GetRequiredService<
                BaselineSchemaVerifier>());
        Assert.NotNull(
            provider.GetRequiredService<
                BaselineRegistrationService>());
    }

    [Fact]
    public void SafetyGuardBlocksExistingUnversionedSchema()
    {
        MigrationStatus status = CreateStatus(
            applicationTableCount: 31,
            appliedVersions: []);

        string? blockReason =
            MigrationSafetyGuard.GetBlockReason(status);

        Assert.NotNull(blockReason);
        Assert.Contains(
            "existing unversioned schema",
            blockReason,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SafetyGuardAllowsEmptyDatabase()
    {
        MigrationStatus status = CreateStatus(
            applicationTableCount: 0,
            appliedVersions: []);

        Assert.Null(
            MigrationSafetyGuard.GetBlockReason(status));
    }

    [Fact]
    public void SafetyGuardBlocksGappedHistory()
    {
        MigrationStatus status = CreateStatus(
            applicationTableCount: 2,
            appliedVersions: [1, 3]);

        string? blockReason =
            MigrationSafetyGuard.GetBlockReason(status);

        Assert.NotNull(blockReason);
        Assert.Contains(
            "contiguous sequence",
            blockReason,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SafetyGuardBlocksHistoryUnknownToAssembly()
    {
        MigrationStatus status = CreateStatus(
            applicationTableCount: 2,
            appliedVersions: [1]);

        string? blockReason =
            MigrationSafetyGuard.GetBlockReason(status);

        Assert.NotNull(blockReason);
        Assert.Contains(
            "not present in the migration assembly",
            blockReason,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SafetyGuardAllowsKnownContiguousHistory()
    {
        MigrationStatus status = CreateStatus(
            applicationTableCount: 2,
            appliedVersions: [1],
            availableMigrations:
            [
                new MigrationDescriptor(
                    1,
                    "Known migration"),
            ]);

        Assert.Null(
            MigrationSafetyGuard.GetBlockReason(status));
    }

    private static MigrationStatus CreateStatus(
        int applicationTableCount,
        IReadOnlyList<long> appliedVersions,
        IReadOnlyList<MigrationDescriptor>? availableMigrations =
            null)
    {
        return new MigrationStatus(
            "localhost",
            "pbm_test",
            "test",
            "migration_user@localhost",
            appliedVersions.Count > 0,
            applicationTableCount,
            availableMigrations ?? [],
            appliedVersions,
            [],
            null);
    }

    [Migration(42, "Later test migration")]
    private sealed class LaterTestMigration : Migration
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }
    }

    [Migration(41, "Earlier test migration")]
    private sealed class EarlierTestMigration : Migration
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }
    }
}
