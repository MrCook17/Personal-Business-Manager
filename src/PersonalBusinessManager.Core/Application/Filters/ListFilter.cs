namespace PersonalBusinessManager.Core.Application.Filters;

public abstract record ListFilter
{
    private string? _searchText;
    private PagingRequest _paging = new();

    public string? SearchText
    {
        get => _searchText;
        init => _searchText = NormaliseSearchText(value);
    }

    public bool IncludeArchived { get; init; }

    public PagingRequest Paging
    {
        get => _paging;
        init => _paging = value
            ?? throw new ArgumentNullException(nameof(Paging));
    }

    public static string? NormaliseSearchText(string? value)
    {
        string? normalised = value?.Trim();
        return string.IsNullOrWhiteSpace(normalised)
            ? null
            : normalised;
    }
}
