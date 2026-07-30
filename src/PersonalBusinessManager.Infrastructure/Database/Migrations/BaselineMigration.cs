using FluentMigrator;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public abstract class BaselineMigration : Migration
{
    protected void Apply(string sql, int schemaVersion)
    {
        Execute.Sql(BaselineMigrationSql.ConfigureSession);
        Execute.Sql(sql);

        if (schemaVersion >= 2)
        {
            Execute.Sql(
                $"""
                UPDATE `schema_information`
                SET
                    `schema_version` = {schemaVersion},
                    `date_updated_utc` = UTC_TIMESTAMP(6)
                WHERE `record_id` = 1;
                """);
        }
    }

    protected static void RejectDestructiveDown(int version)
    {
        throw new NotSupportedException(
            $"Migration {version:D4} cannot be reversed safely because doing so would delete schema or seeded business configuration. Restore a tested backup or rebuild a disposable database instead.");
    }
}
