using Dapper;
using Microsoft.Extensions.Logging.Abstractions;
using MySqlConnector;
using PersonalBusinessManager.Infrastructure.Database;

namespace PersonalBusinessManager.IntegrationTests;

[Collection(MariaDbTestGroup.Name)]
public sealed class MariaDbFoundationTests
{
    [Fact]
    public void ConnectionFactoryRejectsMissingConfiguration()
    {
        var factory = new MariaDbConnectionFactory(null);

        InvalidOperationException exception = Assert.Throws<
            InvalidOperationException>(factory.CreateConnection);

        Assert.Contains(
            "not been configured",
            exception.Message,
            StringComparison.Ordinal);
    }

    [MariaDbTestFact]
    public async Task ConnectionFactoryOpensTheApprovedRuntimeDatabase()
    {
        string connectionString = MariaDbTestEnvironment
            .GetRequiredRuntimeConnectionString();
        var factory = new MariaDbConnectionFactory(
            connectionString);

        await using MySqlConnection connection =
            factory.CreateConnection();
        await connection.OpenAsync();

        var identity = await connection.QuerySingleAsync<
            DatabaseIdentity>(
            """
            SELECT
                DATABASE() AS DatabaseName,
                CURRENT_USER() AS AccountName;
            """);

        Assert.Equal(
            "personal_business_manager_test",
            identity.DatabaseName);
        Assert.Equal(
            "personal_business_test_app@localhost",
            identity.AccountName);
    }

    [Fact]
    public async Task DatabaseHealthReportsMissingConfiguration()
    {
        var service = new DatabaseHealthService(
            new MariaDbConnectionFactory(null),
            NullLogger<DatabaseHealthService>.Instance);

        var result = await service.CheckAsync();

        Assert.False(result.IsAvailable);
        Assert.Equal(
            "Connection string not configured",
            result.Message);
    }

    [MariaDbTestFact]
    public async Task DatabaseHealthReportsTheAvailableMariaDbServer()
    {
        var service = new DatabaseHealthService(
            new MariaDbConnectionFactory(
                MariaDbTestEnvironment
                    .GetRequiredRuntimeConnectionString()),
            NullLogger<DatabaseHealthService>.Instance);

        var result = await service.CheckAsync();

        Assert.True(result.IsAvailable);
        Assert.StartsWith(
            "Connected: ",
            result.Message,
            StringComparison.Ordinal);
        Assert.Contains(
            "MariaDB",
            result.Message,
            StringComparison.Ordinal);
    }

    private sealed class DatabaseIdentity
    {
        public required string DatabaseName { get; init; }

        public required string AccountName { get; init; }
    }
}
