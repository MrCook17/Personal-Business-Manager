using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    1,
    TransactionBehavior.None,
    "Create users and password recovery codes")]
public sealed class Migration0001CreateUsersAndSecurity : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0001, 1);
    }

    public override void Down()
    {
        RejectDestructiveDown(1);
    }
}
