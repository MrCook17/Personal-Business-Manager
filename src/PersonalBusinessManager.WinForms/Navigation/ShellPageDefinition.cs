namespace PersonalBusinessManager.WinForms.Navigation;

public sealed class ShellPageDefinition
{
    private readonly Func<CancellationToken, ValueTask<UserControl>>
        _pageFactory;

    public ShellPageDefinition(
        string key,
        string title,
        string breadcrumb,
        Func<CancellationToken, ValueTask<UserControl>> pageFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(breadcrumb);
        ArgumentNullException.ThrowIfNull(pageFactory);

        Key = key.Trim();
        Title = title.Trim();
        Breadcrumb = breadcrumb.Trim();
        _pageFactory = pageFactory;
    }

    public string Key { get; }

    public string Title { get; }

    public string Breadcrumb { get; }

    public ValueTask<UserControl> CreatePageAsync(
        CancellationToken cancellationToken)
    {
        return _pageFactory(cancellationToken);
    }

    public static ShellPageDefinition FromSynchronousFactory(
        string key,
        string title,
        string breadcrumb,
        Func<UserControl> pageFactory)
    {
        ArgumentNullException.ThrowIfNull(pageFactory);

        return new ShellPageDefinition(
            key,
            title,
            breadcrumb,
            _ => ValueTask.FromResult(pageFactory()));
    }
}

public interface IShellNavigationStatefulPage
{
    object? CaptureNavigationState();

    void RestoreNavigationState(object? state);
}

public interface IShellNavigationGuard
{
    ValueTask<bool> CanNavigateAwayAsync(
        CancellationToken cancellationToken);
}
