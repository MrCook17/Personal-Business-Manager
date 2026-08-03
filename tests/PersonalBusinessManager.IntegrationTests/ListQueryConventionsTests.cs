using Dapper;
using MySqlConnector;
using PersonalBusinessManager.Core.Application.Filters;
using PersonalBusinessManager.Infrastructure.Database;
using PersonalBusinessManager.Infrastructure.Database.Queries;

namespace PersonalBusinessManager.IntegrationTests;

[Collection(MariaDbTestGroup.Name)]
public sealed class ListQueryConventionsTests
{
    [Fact]
    public void CommandFactoryAppliesTimeoutAndCancellationConventions()
    {
        using var cancellationSource = new CancellationTokenSource();

        CommandDefinition command = ListQueryConventions.CreateCommand(
            "SELECT 1;",
            parameters: null,
            cancellationSource.Token);

        Assert.Equal("SELECT 1;", command.CommandText);
        Assert.Equal(
            ListQueryConventions.DefaultCommandTimeoutSeconds,
            command.CommandTimeout);
        Assert.Equal(cancellationSource.Token, command.CancellationToken);
        Assert.Equal(System.Data.CommandType.Text, command.CommandType);
    }

    [Theory]
    [InlineData(SortDirection.Ascending, "ASC")]
    [InlineData(SortDirection.Descending, "DESC")]
    public void SortDirectionMapsToWhitelistedSqlKeyword(
        SortDirection direction,
        string expected)
    {
        Assert.Equal(expected, direction.ToSqlKeyword());
    }

    [Fact]
    public void KeysetMaterialisationReadsOnlyOneLookAheadRow()
    {
        var request = new PagingRequest(pageSize: 50);
        int enumerated = 0;
        IEnumerable<int> CountedRows()
        {
            for (int value = 200; value >= 1; value--)
            {
                enumerated++;
                yield return value;
            }
        }

        var result = ListQueryConventions.CreateKeysetPage(
            CountedRows(),
            request);

        Assert.Equal(50, result.Items.Count);
        Assert.True(result.HasNextPage);
        Assert.Equal(51, enumerated);
    }

    [Fact]
    public void DemoKeysetQueryPagesWithoutDuplicatesOrUnlimitedRows()
    {
        int[] source = Enumerable.Range(1, 500)
            .OrderDescending()
            .ToArray();
        var request = new PagingRequest(pageSize: 50);

        var first = ListQueryConventions.CreateKeysetPage(
            source
                .Where(recordId => recordId < 501)
                .Take(request.QueryRowLimit),
            request);
        int cursor = first.Items[^1];
        var second = ListQueryConventions.CreateKeysetPage(
            source
                .Where(recordId => recordId < cursor)
                .Take(request.QueryRowLimit),
            request.NextPage());

        Assert.Equal(50, first.Items.Count);
        Assert.Equal(50, second.Items.Count);
        Assert.Empty(first.Items.Intersect(second.Items));
        Assert.Equal(500, first.Items[0]);
        Assert.Equal(451, cursor);
        Assert.Equal(450, second.Items[0]);
    }

    [MariaDbTestFact]
    public async Task MariaDbDemoQueryUsesDeterministicKeysetAndBoundedLimit()
    {
        var request = new PagingRequest(pageSize: 50);
        var factory = new MariaDbConnectionFactory(
            MariaDbTestEnvironment.GetRequiredRuntimeConnectionString());
        await using MySqlConnection connection = factory.CreateConnection();
        await connection.OpenAsync();
        using var timeout = new CancellationTokenSource(
            TimeSpan.FromSeconds(20));

        const string sql =
            """
            WITH RECURSIVE demo_records AS
            (
                SELECT 1 AS record_id
                UNION ALL
                SELECT record_id + 1
                FROM demo_records
                WHERE record_id < 250
            )
            SELECT
                record_id AS RecordId,
                CONCAT('Record ', record_id) AS DisplayName
            FROM demo_records
            WHERE record_id < @AfterRecordId
            ORDER BY record_id DESC
            LIMIT @QueryRowLimit;
            """;
        CommandDefinition command = ListQueryConventions.CreateCommand(
            sql,
            new
            {
                AfterRecordId = 251,
                request.QueryRowLimit,
            },
            timeout.Token);

        IEnumerable<DemoListProjection> rows =
            await connection.QueryAsync<DemoListProjection>(command);
        var page = ListQueryConventions.CreateKeysetPage(rows, request);

        Assert.Equal(50, page.Items.Count);
        Assert.True(page.HasNextPage);
        Assert.Equal(250, page.Items[0].RecordId);
        Assert.Equal(201, page.Items[^1].RecordId);
        Assert.Equal("Record 250", page.Items[0].DisplayName);
    }

    private sealed class DemoListProjection
    {
        public int RecordId { get; init; }

        public string DisplayName { get; init; } = string.Empty;
    }
}
