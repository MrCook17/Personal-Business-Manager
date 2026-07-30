using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    6,
    TransactionBehavior.None,
    "Create financial snapshots, applications, and contributions")]
public sealed class Migration0006CreateFinancialActivityTables : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0006, 6);
    }

    public override void Down()
    {
        RejectDestructiveDown(6);
    }
}
