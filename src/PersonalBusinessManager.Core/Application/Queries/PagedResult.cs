using PersonalBusinessManager.Core.Application.Filters;

namespace PersonalBusinessManager.Core.Application.Queries;

public sealed class PagedResult<T>
{
    public PagedResult(
        IEnumerable<T> items,
        PagingRequest request,
        bool hasNextPage,
        long? totalItemCount = null,
        bool? hasPreviousPage = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(request);

        T[] materialisedItems = items.ToArray();
        if (materialisedItems.Length > request.PageSize)
        {
            throw new ArgumentException(
                "A page cannot contain more items than its requested page size.",
                nameof(items));
        }

        if (totalItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalItemCount),
                totalItemCount,
                "Total item count cannot be negative.");
        }

        Items = Array.AsReadOnly(materialisedItems);
        Request = request;
        TotalItemCount = totalItemCount;
        HasPreviousPage = hasPreviousPage
            ?? request.PageNumber > PagingRequest.DefaultPageNumber;
        HasNextPage = totalItemCount.HasValue
            ? LastItemNumber < totalItemCount.Value
            : hasNextPage;
    }

    public IReadOnlyList<T> Items { get; }

    public PagingRequest Request { get; }

    public int PageNumber => Request.PageNumber;

    public int PageSize => Request.PageSize;

    public long? TotalItemCount { get; }

    public bool HasPreviousPage { get; }

    public bool HasNextPage { get; }

    public long FirstItemNumber =>
        Items.Count == 0 ? 0 : Request.Offset + 1;

    public long LastItemNumber => Request.Offset + Items.Count;

}
