using Dapper;
using MySqlConnector;
using PersonalBusinessManager.Core.Application.Contracts;
using PersonalBusinessManager.Core.Application.Dtos;

namespace PersonalBusinessManager.Infrastructure.Database.Repositories;

public sealed class ApplicationSettingRepository
    : IApplicationSettingRepository
{
    private const int CommandTimeoutSeconds = 30;

    private readonly MariaDbConnectionFactory _connectionFactory;

    public ApplicationSettingRepository(
        MariaDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ApplicationSettingDto?> GetByKeyAsync(
        string settingKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingKey);

        const string sql =
            """
            SELECT
                `record_id` AS RecordId,
                `setting_key` AS SettingKey,
                `setting_value` AS SettingValue,
                `value_type_code` AS ValueTypeCode,
                `is_sensitive` AS IsSensitive,
                `date_updated_utc` AS DateUpdatedUtc,
                `updated_by_user_id` AS UpdatedByUserId
            FROM `application_settings`
            WHERE `setting_key` = @SettingKey;
            """;

        await using MySqlConnection connection =
            _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return await connection
            .QuerySingleOrDefaultAsync<ApplicationSettingDto>(
                new CommandDefinition(
                    sql,
                    new { SettingKey = settingKey },
                    commandTimeout: CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));
    }

    public async Task<ulong> InsertAsync(
        ApplicationSettingDto setting,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(setting);
        ArgumentException.ThrowIfNullOrWhiteSpace(setting.SettingKey);

        const string sql =
            """
            INSERT INTO `application_settings` (
                `setting_key`,
                `setting_value`,
                `value_type_code`,
                `is_sensitive`,
                `date_updated_utc`,
                `updated_by_user_id`
            )
            VALUES (
                @SettingKey,
                @SettingValue,
                @ValueTypeCode,
                @IsSensitive,
                @DateUpdatedUtc,
                @UpdatedByUserId
            );
            """;

        await using MySqlConnection connection =
            _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                setting,
                commandTimeout: CommandTimeoutSeconds,
                cancellationToken: cancellationToken));

        const string insertedIdSql =
            "SELECT LAST_INSERT_ID();";

        return await connection.QuerySingleAsync<ulong>(
            new CommandDefinition(
                insertedIdSql,
                commandTimeout: CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(
        ApplicationSettingDto setting,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(setting);
        ArgumentException.ThrowIfNullOrWhiteSpace(setting.SettingKey);

        const string sql =
            """
            UPDATE `application_settings`
            SET
                `setting_value` = @SettingValue,
                `value_type_code` = @ValueTypeCode,
                `is_sensitive` = @IsSensitive,
                `date_updated_utc` = @DateUpdatedUtc,
                `updated_by_user_id` = @UpdatedByUserId
            WHERE `setting_key` = @SettingKey;
            """;

        await using MySqlConnection connection =
            _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        int affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                setting,
                commandTimeout: CommandTimeoutSeconds,
                cancellationToken: cancellationToken));

        return affectedRows == 1;
    }

    public async Task<bool> DeleteByKeyAsync(
        string settingKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingKey);

        const string sql =
            """
            DELETE FROM `application_settings`
            WHERE `setting_key` = @SettingKey;
            """;

        await using MySqlConnection connection =
            _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        int affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { SettingKey = settingKey },
                commandTimeout: CommandTimeoutSeconds,
                cancellationToken: cancellationToken));

        return affectedRows == 1;
    }
}
