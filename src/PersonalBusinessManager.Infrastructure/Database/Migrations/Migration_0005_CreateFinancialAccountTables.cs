using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    5,
    TransactionBehavior.None,
    "Create financial account type and account tables")]
public sealed class Migration0005CreateFinancialAccountTables : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0005, 5);
    }

    public override void Down()
    {
        RejectDestructiveDown(5);
    }
}
