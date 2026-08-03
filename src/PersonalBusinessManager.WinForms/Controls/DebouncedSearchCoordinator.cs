namespace PersonalBusinessManager.WinForms.Controls;

public sealed class DebouncedSearchCoordinator : IDisposable
{
    public const int DefaultDelayMilliseconds = 300;
    public const int MinimumDelayMilliseconds = 250;
    public const int MaximumDelayMilliseconds = 400;

    private readonly Lock _sync = new();
    private readonly TimeSpan _delay;
    private CancellationTokenSource? _activeRequest;
    private bool _disposed;

    public DebouncedSearchCoordinator(
        int delayMilliseconds = DefaultDelayMilliseconds)
    {
        if (delayMilliseconds < MinimumDelayMilliseconds
            || delayMilliseconds > MaximumDelayMilliseconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delayMilliseconds),
                delayMilliseconds,
                $"Search debounce must be between "
                    + $"{MinimumDelayMilliseconds} and "
                    + $"{MaximumDelayMilliseconds} milliseconds.");
        }

        _delay = TimeSpan.FromMilliseconds(delayMilliseconds);
    }

    public TimeSpan Delay => _delay;

    public async Task<bool> QueueAsync(
        Func<CancellationToken, Task> searchAsync,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchAsync);

        CancellationTokenSource request;
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _activeRequest?.Cancel();
            request = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);
            _activeRequest = request;
        }

        try
        {
            await Task.Delay(_delay, request.Token);
            await searchAsync(request.Token);

            lock (_sync)
            {
                return !_disposed
                    && ReferenceEquals(_activeRequest, request)
                    && !request.IsCancellationRequested;
            }
        }
        catch (OperationCanceledException)
            when (request.IsCancellationRequested)
        {
            return false;
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_activeRequest, request))
                {
                    _activeRequest = null;
                }
            }

            request.Dispose();
        }
    }

    public void Cancel()
    {
        lock (_sync)
        {
            _activeRequest?.Cancel();
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _activeRequest?.Cancel();
        }
    }
}
