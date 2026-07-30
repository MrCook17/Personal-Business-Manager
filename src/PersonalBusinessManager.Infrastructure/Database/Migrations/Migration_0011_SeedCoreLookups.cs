using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    11,
    TransactionBehavior.None,
    "Seed core account types, expense category, and invoice sequences")]
public sealed class Migration0011SeedCoreLookups : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0011, 11);
    }

    public override void Down()
    {
        RejectDestructiveDown(11);
    }
}
