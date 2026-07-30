using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    4,
    TransactionBehavior.None,
    "Create jobs, timers, time entries, and tasks")]
public sealed class Migration0004CreateJobAndTimeTables : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0004, 4);
    }

    public override void Down()
    {
        RejectDestructiveDown(4);
    }
}
