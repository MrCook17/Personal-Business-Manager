using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    8,
    TransactionBehavior.None,
    "Create invoice time-entry and payment link tables")]
public sealed class Migration0008CreateInvoiceLinkTables : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0008, 8);
    }

    public override void Down()
    {
        RejectDestructiveDown(8);
    }
}
