using Microsoft.Extensions.DependencyInjection;
using PersonalBusinessManager.Core.Application.Contracts;
using PersonalBusinessManager.Core.Application.Dtos;
using PersonalBusinessManager.Infrastructure;
using PersonalBusinessManager.Infrastructure.Database;
using PersonalBusinessManager.Infrastructure.Database.Repositories;

namespace PersonalBusinessManager.IntegrationTests;

public sealed class ApplicationSettingRepositoryTests
{
    [Fact]
    public void InfrastructureRegistersApplicationSettingRepository()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddInfrastructure(
            "Server=localhost;Database=pbm_repository_test;"
            + "User ID=repository_test");

        using ServiceProvider provider =
            services.BuildServiceProvider();

        ApplicationSettingRepository repository =
            Assert.IsType<ApplicationSettingRepository>(
                provider.GetRequiredService<
                    IApplicationSettingRepository>());
        ApplicationSettingRepository secondRepository =
            Assert.IsType<ApplicationSettingRepository>(
                provider.GetRequiredService<
                    IApplicationSettingRepository>());

        Assert.NotSame(repository, secondRepository);
    }

    [MariaDbTestFact]
    public async Task GetByKeyAsyncReadsSeededApplicationSetting()
    {
        ApplicationSettingRepository repository = CreateRepository();
        using var timeout = new CancellationTokenSource(
            TimeSpan.FromSeconds(20));

        ApplicationSettingDto? setting =
            await repository.GetByKeyAsync(
                "locale",
                timeout.Token);

        Assert.NotNull(setting);
        Assert.True(setting.RecordId > 0);
        Assert.Equal("locale", setting.SettingKey);
        Assert.Equal("en-GB", setting.SettingValue);
        Assert.Equal("string", setting.ValueTypeCode);
        Assert.False(setting.IsSensitive);
        Assert.Null(setting.UpdatedByUserId);
    }

    [MariaDbTestFact]
    public async Task InsertUpdateAndDeleteRoundTripCleansUpTestSetting()
    {
        ApplicationSettingRepository repository = CreateRepository();
        string settingKey =
            $"p2_06_repository_test_{Guid.NewGuid():N}";
        using var timeout = new CancellationTokenSource(
            TimeSpan.FromSeconds(20));

        try
        {
            var insertedSetting = new ApplicationSettingDto
            {
                SettingKey = settingKey,
                SettingValue = "before",
                ValueTypeCode = "string",
                IsSensitive = false,
                DateUpdatedUtc = DateTime.UtcNow,
            };

            ulong recordId = await repository.InsertAsync(
                insertedSetting,
                timeout.Token);

            Assert.True(recordId > 0);

            ApplicationSettingDto? afterInsert =
                await repository.GetByKeyAsync(
                    settingKey,
                    timeout.Token);

            Assert.NotNull(afterInsert);
            Assert.Equal(recordId, afterInsert.RecordId);
            Assert.Equal("before", afterInsert.SettingValue);

            ApplicationSettingDto updatedSetting =
                afterInsert with
                {
                    SettingValue = "after ' parameterised",
                    DateUpdatedUtc = DateTime.UtcNow,
                };

            bool updated = await repository.UpdateAsync(
                updatedSetting,
                timeout.Token);

            Assert.True(updated);

            ApplicationSettingDto? afterUpdate =
                await repository.GetByKeyAsync(
                    settingKey,
                    timeout.Token);

            Assert.NotNull(afterUpdate);
            Assert.Equal(recordId, afterUpdate.RecordId);
            Assert.Equal(
                "after ' parameterised",
                afterUpdate.SettingValue);
        }
        finally
        {
            await repository.DeleteByKeyAsync(
                settingKey,
                CancellationToken.None);
        }

        Assert.Null(
            await repository.GetByKeyAsync(
                settingKey,
                timeout.Token));
    }

    private static ApplicationSettingRepository CreateRepository()
    {
        string connectionString =
            MariaDbTestEnvironment
                .GetRequiredRuntimeConnectionString();

        return new ApplicationSettingRepository(
            new MariaDbConnectionFactory(connectionString));
    }
}
