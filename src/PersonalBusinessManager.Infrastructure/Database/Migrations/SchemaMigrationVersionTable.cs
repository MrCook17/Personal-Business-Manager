using FluentMigrator.Runner.VersionTableInfo;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[VersionTableMetaData]
public sealed class SchemaMigrationVersionTable : IVersionTableMetaData
{
    public bool OwnsSchema => false;

    public string SchemaName => string.Empty;

    public string TableName => "schema_migrations";

    public string ColumnName => "version";

    public string DescriptionColumnName => "description";

    public string AppliedOnColumnName => "applied_on_utc";

    public string UniqueIndexName => "uq_schema_migrations_version";

    public bool CreateWithPrimaryKey => true;
}
