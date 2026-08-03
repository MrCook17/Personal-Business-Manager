using System.Data;
using Dapper;
using PersonalBusinessManager.Core.Application.Filters;
using PersonalBusinessManager.Core.Application.Queries;

namespace PersonalBusinessManager.Infrastructure.Database.Queries;

public static class ListQueryConventions
{
    public const int DefaultCommandTimeoutSeconds = 30;

    public static CommandDefinition CreateCommand(
        string commandText,
        object? parameters,
        CancellationToken cancellationToken,
        int commandTimeoutSeconds = DefaultCommandTimeoutSeconds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandText);

        if (commandTimeoutSeconds < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(commandTimeoutSeconds),
                commandTimeoutSeconds,
                "Command timeout must be at least one second.");
        }

        return new CommandDefinition(
            commandText,
            parameters,
            commandTimeout: commandTimeoutSeconds,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
    }

    public static PagedResult<T> CreateKeysetPage<T>(
        IEnumerable<T> queryRows,
        PagingRequest request,
        long? totalItemCount = null,
        bool? hasPreviousPage = null)
    {
        ArgumentNullException.ThrowIfNull(queryRows);
        ArgumentNullException.ThrowIfNull(request);

        T[] lookAheadRows = queryRows
            .Take(request.QueryRowLimit)
            .ToArray();
        bool hasNextPage = lookAheadRows.Length > request.PageSize;

        return new PagedResult<T>(
            lookAheadRows.Take(request.PageSize),
            request,
            hasNextPage,
            totalItemCount,
            hasPreviousPage);
    }

    public static string ToSqlKeyword(this SortDirection direction) =>
        direction switch
        {
            SortDirection.Ascending => "ASC",
            SortDirection.Descending => "DESC",
            _ => throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "Unknown sort direction."),
        };
}
