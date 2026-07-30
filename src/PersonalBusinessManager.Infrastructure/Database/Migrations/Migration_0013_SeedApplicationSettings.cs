using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    13,
    TransactionBehavior.None,
    "Seed application settings and finalize schema version")]
public sealed class Migration0013SeedApplicationSettings : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0013, 13);
    }

    public override void Down()
    {
        RejectDestructiveDown(13);
    }
}
