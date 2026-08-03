using System.Reflection;
using PersonalBusinessManager.Core.Application.Filters;
using PersonalBusinessManager.Core.Application.Queries;

namespace PersonalBusinessManager.Core.Tests;

public sealed class ListPagingFoundationTests
{
    [Fact]
    public void PagingRequestUsesBoundedDefaultsAndLookAheadLimit()
    {
        var request = new PagingRequest();

        Assert.Equal(1, request.PageNumber);
        Assert.Equal(100, request.PageSize);
        Assert.Equal(0, request.Offset);
        Assert.Equal(101, request.QueryRowLimit);
        Assert.Equal(200, PagingRequest.MaximumPageSize);
    }

    [Theory]
    [InlineData(0, 100, "pageNumber")]
    [InlineData(1, 0, "pageSize")]
    [InlineData(1, 201, "pageSize")]
    public void PagingRequestRejectsUnboundedOrInvalidValues(
        int pageNumber,
        int pageSize,
        string expectedParameter)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<
            ArgumentOutOfRangeException>(() =>
                new PagingRequest(pageNumber, pageSize));

        Assert.Equal(expectedParameter, exception.ParamName);
    }

    [Fact]
    public void PagingRequestCalculatesOffsetAndAdjacentPages()
    {
        var request = new PagingRequest(3, 50);

        Assert.Equal(100, request.Offset);
        Assert.Equal(new PagingRequest(4, 50), request.NextPage());
        Assert.Equal(new PagingRequest(2, 50), request.PreviousPage());
        Assert.Equal(
            new PagingRequest(1, 50),
            new PagingRequest(1, 50).PreviousPage());
    }

    [Fact]
    public void BaseListFilterNormalisesSearchAndCarriesCommonState()
    {
        var filter = new TestListFilter
        {
            SearchText = "  harbour design  ",
            IncludeArchived = true,
            Paging = new PagingRequest(2, 50),
            SortDirection = SortDirection.Descending,
        };

        Assert.Equal("harbour design", filter.SearchText);
        Assert.True(filter.IncludeArchived);
        Assert.Equal(2, filter.Paging.PageNumber);
        Assert.Equal(SortDirection.Descending, filter.SortDirection);
        Assert.Null(ListFilter.NormaliseSearchText("   "));
    }

    [Fact]
    public void PagedResultExposesRangeTotalAndNavigationMetadata()
    {
        var result = new PagedResult<int>(
            Enumerable.Range(51, 50),
            new PagingRequest(2, 50),
            hasNextPage: true,
            totalItemCount: 125);

        Assert.Equal(50, result.Items.Count);
        Assert.Equal(51, result.FirstItemNumber);
        Assert.Equal(100, result.LastItemNumber);
        Assert.Equal(125, result.TotalItemCount);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PagedResultRejectsMoreThanTheRequestedPageSize()
    {
        var request = new PagingRequest(pageSize: 50);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new PagedResult<int>(
                Enumerable.Range(1, 51),
                request,
                hasNextPage: false));

        Assert.Equal("items", exception.ParamName);
    }

    [Fact]
    public void PagedQueryContractRequiresCancellationTokenLast()
    {
        MethodInfo method = typeof(IPagedListQuery<,>)
            .GetMethod(nameof(IPagedListQuery<ListFilter, object>.ExecuteAsync))
            ?? throw new InvalidOperationException(
                "Paged list query contract was not found.");
        ParameterInfo[] parameters = method.GetParameters();

        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(CancellationToken), parameters[^1].ParameterType);
        Assert.False(parameters[^1].HasDefaultValue);
    }

    private sealed record TestListFilter : ListFilter
    {
        public SortDirection SortDirection { get; init; }
    }
}
