using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    7,
    TransactionBehavior.None,
    "Create invoice sequences, invoices, and invoice lines")]
public sealed class Migration0007CreateInvoiceTables : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0007, 7);
    }

    public override void Down()
    {
        RejectDestructiveDown(7);
    }
}
