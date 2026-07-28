using Microsoft.Extensions.Logging;
using MySqlConnector;
using PersonalBusinessManager.Core.Application.Contracts;

namespace PersonalBusinessManager.Infrastructure.Database;

public sealed class DatabaseHealthService : IDatabaseHealthService
{
    private readonly MariaDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseHealthService> _logger;

    public DatabaseHealthService(
        MariaDbConnectionFactory connectionFactory,
        ILogger<DatabaseHealthService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<DatabaseHealthResult> CheckAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_connectionFactory.IsConfigured)
        {
            return new DatabaseHealthResult(
                false,
                "Connection string not configured");
        }

        try
        {
            await using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            return new DatabaseHealthResult(
                true,
                $"Connected: {connection.ServerVersion}");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return new DatabaseHealthResult(
                false,
                "Connection check timed out");
        }
        catch (MySqlException exception)
        {
            _logger.LogError(
                exception,
                "The MariaDB connection check failed.");

            return new DatabaseHealthResult(
                false,
                "Database unavailable");
        }
    }
}