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
            _logger.LogWarning(
                "Database connection check was skipped because " +
                "the connection string is not configured.");

            return new DatabaseHealthResult(
                false,
                "Connection string not configured");
        }

        try
        {
            await using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            string serverVersion = connection.ServerVersion;

            _logger.LogInformation(
                "Database connection succeeded. Server version: {ServerVersion}.",
                serverVersion);

            return new DatabaseHealthResult(
                true,
                $"Connected: {serverVersion}");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Database connection check was cancelled or timed out.");

            return new DatabaseHealthResult(
                false,
                "Connection check timed out");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "Database connection failed. Error type: {ErrorType}.",
                exception.GetType().Name);

            return new DatabaseHealthResult(
                false,
                "Database unavailable");
        }
    }
}
