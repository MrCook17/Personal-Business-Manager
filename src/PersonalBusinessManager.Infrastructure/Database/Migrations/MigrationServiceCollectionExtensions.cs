using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public static class MigrationServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseMigrations(
        this IServiceCollection services,
        string migrationConnectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            migrationConnectionString);

        var versionTable = new SchemaMigrationVersionTable();
        var catalog = new MigrationCatalog();

        services.AddSingleton(catalog);
        services.AddSingleton(
            provider => new MigrationDatabaseInspector(
                migrationConnectionString,
                provider.GetRequiredService<MigrationCatalog>()));

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddMySql()
                .WithGlobalConnectionString(
                    migrationConnectionString)
                .WithVersionTable(versionTable)
                .ScanIn(typeof(MigrationCatalog).Assembly)
                .For.Migrations());

        services.AddTransient<MigrationRunnerService>();

        return services;
    }
}
