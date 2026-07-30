using System.Reflection;
using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public sealed class MigrationCatalog
{
    private readonly IReadOnlyList<MigrationDescriptor> _migrations;

    public MigrationCatalog()
        : this(typeof(MigrationCatalog).Assembly)
    {
    }

    public MigrationCatalog(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        List<MigrationDescriptor> migrations = assembly
            .GetTypes()
            .Select(type => type.GetCustomAttribute<MigrationAttribute>())
            .Where(attribute => attribute is not null)
            .Select(attribute => new MigrationDescriptor(
                attribute!.Version,
                string.IsNullOrWhiteSpace(attribute.Description)
                    ? $"Migration {attribute.Version}"
                    : attribute.Description))
            .OrderBy(migration => migration.Version)
            .ToList();

        long? duplicateVersion = migrations
            .GroupBy(migration => migration.Version)
            .Where(group => group.Count() > 1)
            .Select(group => (long?)group.Key)
            .FirstOrDefault();

        if (duplicateVersion is not null)
        {
            throw new InvalidOperationException(
                $"Migration version {duplicateVersion.Value} is registered more than once.");
        }

        _migrations = migrations;
    }

    public IReadOnlyList<MigrationDescriptor> Migrations => _migrations;
}
