using MySqlConnector;
using PersonalBusinessManager.Core.Application.Dtos;
using PersonalBusinessManager.Infrastructure.Database;
using PersonalBusinessManager.Infrastructure.Database.Repositories;

namespace PersonalBusinessManager.IntegrationTests;

[Collection(MariaDbTestGroup.Name)]
public sealed class MariaDbConstraintTests
{
    [MariaDbTestFact]
    public async Task DuplicateApplicationSettingKeyIsRejectedByUniqueConstraint()
    {
        ApplicationSettingRepository repository = CreateRepository();
        string settingKey = $"test.p208.unique.{Guid.NewGuid():N}";
        ApplicationSettingDto setting = CreateSetting(
            settingKey,
            updatedByUserId: null);

        try
        {
            await repository.InsertAsync(setting);

            MySqlException exception = await Assert.ThrowsAsync<
                MySqlException>(() =>
                    repository.InsertAsync(setting));

            Assert.Equal(1062, exception.Number);
        }
        finally
        {
            await repository.DeleteByKeyAsync(settingKey);
        }
    }

    [MariaDbTestFact]
    public async Task UnknownUpdatingUserIsRejectedByForeignKeyConstraint()
    {
        ApplicationSettingRepository repository = CreateRepository();
        string settingKey = $"test.p208.foreign_key.{Guid.NewGuid():N}";
        ApplicationSettingDto setting = CreateSetting(
            settingKey,
            ulong.MaxValue);

        try
        {
            MySqlException exception = await Assert.ThrowsAsync<
                MySqlException>(() =>
                    repository.InsertAsync(setting));

            Assert.Equal(1452, exception.Number);
        }
        finally
        {
            await repository.DeleteByKeyAsync(settingKey);
        }
    }

    private static ApplicationSettingRepository CreateRepository()
    {
        return new ApplicationSettingRepository(
            new MariaDbConnectionFactory(
                MariaDbTestEnvironment
                    .GetRequiredRuntimeConnectionString()));
    }

    private static ApplicationSettingDto CreateSetting(
        string settingKey,
        ulong? updatedByUserId)
    {
        return new ApplicationSettingDto
        {
            SettingKey = settingKey,
            SettingValue = "P2-08 integration test",
            ValueTypeCode = "string",
            IsSensitive = false,
            DateUpdatedUtc = DateTime.UtcNow,
            UpdatedByUserId = updatedByUserId,
        };
    }
}
