using MySqlConnector;

namespace PersonalBusinessManager.Infrastructure.Database;

public sealed class MariaDbConnectionFactory
{
    private readonly string _connectionString;

    public MariaDbConnectionFactory(string? connectionString)
    {
        _connectionString = connectionString?.Trim() ?? string.Empty;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

    public MySqlConnection CreateConnection()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "The MariaDB connection string has not been configured.");
        }

        return new MySqlConnection(_connectionString);
    }
}