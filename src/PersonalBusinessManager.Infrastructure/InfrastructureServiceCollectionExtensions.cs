using Microsoft.Extensions.DependencyInjection;
using PersonalBusinessManager.Core.Application.Contracts;
using PersonalBusinessManager.Infrastructure.Database;
using PersonalBusinessManager.Infrastructure.Database.Repositories;

namespace PersonalBusinessManager.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string? connectionString)
    {
        services.AddSingleton(
            new MariaDbConnectionFactory(connectionString));

        services.AddTransient<
            IDatabaseHealthService,
            DatabaseHealthService>();
        services.AddTransient<
            IApplicationSettingRepository,
            ApplicationSettingRepository>();

        return services;
    }
}
