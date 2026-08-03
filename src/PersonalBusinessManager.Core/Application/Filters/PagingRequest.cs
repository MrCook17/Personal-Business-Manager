namespace PersonalBusinessManager.Core.Application.Filters;

public sealed record PagingRequest
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 100;
    public const int MaximumPageSize = 200;

    public PagingRequest(
        int pageNumber = DefaultPageNumber,
        int pageSize = DefaultPageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "Page number must be at least one.");
        }

        if (pageSize < 1 || pageSize > MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                $"Page size must be between 1 and {MaximumPageSize}.");
        }

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }

    public int PageSize { get; }

    public long Offset =>
        checked((long)(PageNumber - 1) * PageSize);

    public int QueryRowLimit => checked(PageSize + 1);

    public PagingRequest NextPage() =>
        new(checked(PageNumber + 1), PageSize);

    public PagingRequest PreviousPage() =>
        new(Math.Max(DefaultPageNumber, PageNumber - 1), PageSize);
}
