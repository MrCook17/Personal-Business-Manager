using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    10,
    TransactionBehavior.None,
    "Create attachments and attachment link tables")]
public sealed class Migration0010CreateAttachmentTables : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0010, 10);
    }

    public override void Down()
    {
        RejectDestructiveDown(10);
    }
}
