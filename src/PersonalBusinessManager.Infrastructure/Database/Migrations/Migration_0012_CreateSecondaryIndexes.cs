using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

[Migration(
    12,
    TransactionBehavior.None,
    "Create secondary indexes")]
public sealed class Migration0012CreateSecondaryIndexes : BaselineMigration
{
    public override void Up()
    {
        Apply(BaselineMigrationSql.Migration0012, 12);
    }

    public override void Down()
    {
        Execute.Sql(BaselineMigrationSql.ConfigureSession);
        Execute.Sql(BaselineMigrationSql.Migration0012Down);
        Execute.Sql(
            """
            UPDATE `schema_information`
            SET
                `schema_version` = 11,
                `date_updated_utc` = UTC_TIMESTAMP(6)
            WHERE `record_id` = 1;
            """);
    }
}
