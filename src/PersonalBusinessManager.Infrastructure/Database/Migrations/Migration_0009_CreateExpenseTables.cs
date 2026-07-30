using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    9,
    TransactionBehavior.None,
    "Create expense category and expense tables")]
public sealed class Migration0009CreateExpenseTables : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0009, 9);
    }

    public override void Down()
    {
        RejectDestructiveDown(9);
    }
}
