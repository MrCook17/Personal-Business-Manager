using System.ComponentModel;
using PersonalBusinessManager.Core.Application.Queries;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public enum PagedListState
{
    Ready,
    Loading,
    Empty,
    Error,
}

public sealed class ListLoadFailedEventArgs(Exception exception)
    : EventArgs
{
    public Exception Exception { get; } =
        exception ?? throw new ArgumentNullException(nameof(exception));
}

[DesignerCategory("Code")]
public sealed class PagedListView : UserControl, IThemeAwareControl
{
    private readonly Lock _loadSync = new();
    private readonly Panel _contentHost = new();
    private readonly DarkDataGridView _grid = new();
    private readonly EmptyStatePanel _statePanel = new();
    private readonly LoadingOverlay _loadingOverlay = new();
    private readonly PagingControl _paging = new();
    private CancellationTokenSource? _activeLoad;
    private long _loadRequestId;
    private bool _disposed;
    private PagedListState _state;

    public PagedListView()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        Dock = DockStyle.Fill;
        Margin = Padding.Empty;
        Padding = Padding.Empty;
        TabStop = false;
        AccessibleRole = AccessibleRole.Grouping;
        AccessibleName = "Paged record list";

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                UiDimensions.PagingFooterHeight));

        _contentHost.Dock = DockStyle.Fill;
        _contentHost.Margin = Padding.Empty;
        _contentHost.Padding = Padding.Empty;
        _grid.Dock = DockStyle.Fill;
        _grid.Margin = Padding.Empty;
        _statePanel.Dock = DockStyle.Fill;
        _statePanel.Visible = false;
        _statePanel.PrimaryActionClicked += (_, _) =>
        {
            if (State == PagedListState.Error)
            {
                RetryRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                EmptyPrimaryActionRequested?.Invoke(
                    this,
                    EventArgs.Empty);
            }
        };
        _statePanel.SecondaryActionClicked += (_, _) =>
            SecondaryActionRequested?.Invoke(this, EventArgs.Empty);
        _loadingOverlay.MessageText = "Loading records…";
        _loadingOverlay.CanCancel = true;
        _loadingOverlay.CancelRequested += (_, _) =>
        {
            CancelLoading();
            CancelRequested?.Invoke(this, EventArgs.Empty);
        };
        _paging.PageRequested += (_, eventArgs) =>
            PageRequested?.Invoke(this, eventArgs);

        _contentHost.Controls.Add(_grid);
        _contentHost.Controls.Add(_statePanel);
        _contentHost.Controls.Add(_loadingOverlay);
        layout.Controls.Add(_contentHost, 0, 0);
        layout.Controls.Add(_paging, 0, 1);
        Controls.Add(layout);

        ShowEmpty();
        ApplyTheme();
    }

    public event EventHandler<PagingRequestEventArgs>? PageRequested;

    public event EventHandler? RetryRequested;

    public event EventHandler? EmptyPrimaryActionRequested;

    public event EventHandler? SecondaryActionRequested;

    public event EventHandler? CancelRequested;

    public event EventHandler<ListLoadFailedEventArgs>? LoadFailed;

    [Browsable(false)]
    public DarkDataGridView Grid => _grid;

    [Browsable(false)]
    public PagingControl Paging => _paging;

    [Browsable(false)]
    public PagedListState State => _state;

    [Browsable(false)]
    public bool IsLoading => _loadingOverlay.IsActive;

    public void BindResult<T>(PagedResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        _grid.DataSource = result.Items.ToArray();
        _paging.ApplyResult(result);

        if (result.Items.Count == 0)
        {
            ShowEmpty();
            return;
        }

        SetState(PagedListState.Ready);
    }

    public void ShowLoading(
        string message = "Loading records…",
        bool canCancel = true)
    {
        _loadingOverlay.MessageText = message;
        _loadingOverlay.CanCancel = canCancel;
        _statePanel.Visible = false;
        _grid.Visible = false;
        _paging.Enabled = false;
        _loadingOverlay.IsActive = true;
        _state = PagedListState.Loading;
        AccessibleDescription = message;
    }

    public void ShowEmpty(
        string heading = "No records found",
        string description =
            "There are no records for the current filters.",
        string primaryActionText = "",
        string secondaryActionText = "Clear filters")
    {
        _statePanel.StateKind = ContentStateKind.Empty;
        _statePanel.HeadingText = heading;
        _statePanel.DescriptionText = description;
        _statePanel.TechnicalReference = string.Empty;
        _statePanel.PrimaryActionText = primaryActionText;
        _statePanel.SecondaryActionText = secondaryActionText;
        SetState(PagedListState.Empty);
    }

    public void ShowError(
        string description = "Try this operation again.",
        string technicalReference = "")
    {
        _statePanel.StateKind = ContentStateKind.Error;
        _statePanel.HeadingText = "Records could not be loaded";
        _statePanel.DescriptionText = description;
        _statePanel.TechnicalReference = technicalReference;
        _statePanel.PrimaryActionText = "Retry";
        _statePanel.SecondaryActionText = string.Empty;
        SetState(PagedListState.Error);
    }

    public async Task<bool> LoadAsync<T>(
        Func<CancellationToken, Task<PagedResult<T>>> loadAsync,
        string loadingMessage = "Loading records…",
        string errorDescription = "Try this operation again.",
        string technicalReference = "",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loadAsync);

        CancellationTokenSource request;
        long requestId;
        PagedListState previousState;
        lock (_loadSync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _activeLoad?.Cancel();
            request = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);
            _activeLoad = request;
            requestId = checked(++_loadRequestId);
            previousState = _state;
        }

        ShowLoading(loadingMessage, canCancel: true);

        try
        {
            PagedResult<T> result = await loadAsync(request.Token);
            if (!IsCurrentRequest(request, requestId))
            {
                return false;
            }

            BindResult(result);
            return true;
        }
        catch (OperationCanceledException)
            when (request.IsCancellationRequested)
        {
            if (OwnsRequest(request, requestId))
            {
                RestoreAfterCancellation(previousState);
            }

            return false;
        }
        catch (Exception exception)
        {
            if (!IsCurrentRequest(request, requestId))
            {
                return false;
            }

            ShowError(errorDescription, technicalReference);
            LoadFailed?.Invoke(
                this,
                new ListLoadFailedEventArgs(exception));
            return false;
        }
        finally
        {
            lock (_loadSync)
            {
                if (ReferenceEquals(_activeLoad, request))
                {
                    _activeLoad = null;
                }
            }

            request.Dispose();
        }
    }

    public void CancelLoading()
    {
        lock (_loadSync)
        {
            _activeLoad?.Cancel();
        }
    }

    public void ApplyTheme()
    {
        ControlStyler.StylePanel(this, ThemeSurface.Panel);
        ControlStyler.StylePanel(_contentHost, ThemeSurface.Panel);
        _grid.ApplyTheme();
        _statePanel.ApplyTheme();
        _loadingOverlay.ApplyTheme();
        _paging.ApplyTheme();
        Invalidate(true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_loadSync)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _activeLoad?.Cancel();
                }
            }
        }

        base.Dispose(disposing);
    }

    private bool IsCurrentRequest(
        CancellationTokenSource request,
        long requestId)
    {
        return OwnsRequest(request, requestId)
            && !request.IsCancellationRequested;
    }

    private bool OwnsRequest(
        CancellationTokenSource request,
        long requestId)
    {
        lock (_loadSync)
        {
            return !_disposed
                && ReferenceEquals(_activeLoad, request)
                && _loadRequestId == requestId;
        }
    }

    private void SetState(PagedListState state)
    {
        _loadingOverlay.IsActive = false;
        _paging.Enabled = true;
        _statePanel.Visible = state is PagedListState.Empty
            or PagedListState.Error;
        _grid.Visible = state == PagedListState.Ready;
        if (_statePanel.Visible)
        {
            _statePanel.BringToFront();
        }

        _state = state;
        AccessibleDescription = state switch
        {
            PagedListState.Ready => "Records loaded.",
            PagedListState.Empty => _statePanel.HeadingText,
            PagedListState.Error => _statePanel.HeadingText,
            _ => AccessibleDescription,
        };
    }

    private void RestoreAfterCancellation(PagedListState previousState)
    {
        if (previousState == PagedListState.Loading)
        {
            ShowEmpty();
            return;
        }

        SetState(previousState);
    }
}
