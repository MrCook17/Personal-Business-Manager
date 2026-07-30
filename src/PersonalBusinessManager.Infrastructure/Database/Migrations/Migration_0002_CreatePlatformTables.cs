using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    2,
    TransactionBehavior.None,
    "Create platform settings, schema, audit, and backup tables")]
public sealed class Migration0002CreatePlatformTables : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0002, 2);
    }

    public override void Down()
    {
        RejectDestructiveDown(2);
    }
}
