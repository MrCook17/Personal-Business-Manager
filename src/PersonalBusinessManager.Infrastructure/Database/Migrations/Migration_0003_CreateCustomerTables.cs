using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    3,
    TransactionBehavior.None,
    "Create customer, contact, and address tables")]
public sealed class Migration0003CreateCustomerTables : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0003, 3);
    }

    public override void Down()
    {
        RejectDestructiveDown(3);
    }
}
