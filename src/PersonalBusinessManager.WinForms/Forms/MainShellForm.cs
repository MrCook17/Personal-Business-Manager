using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PersonalBusinessManager.Core.Application.Contracts;
using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Navigation;
using PersonalBusinessManager.WinForms.Pages;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Forms;

public sealed class MainShellForm : Form
{
    private static readonly Action<ILogger, string, Exception?>
        LogNavigationFailure = LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2110, nameof(LogNavigationFailure)),
            "Shell navigation to {RouteKey} failed.");

    private static readonly Action<ILogger, Exception?>
        LogDatabaseHealthFailure = LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(2111, nameof(LogDatabaseHealthFailure)),
            "The shell database-health check failed.");

    private readonly IDatabaseHealthService _databaseHealthService;
    private readonly ILogger<MainShellForm> _logger;
    private readonly Dictionary<string, ShellPageDefinition>
        _pageDefinitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NavigationEntry>
        _navigationEntries = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, object?> _navigationState =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SectionEntry> _sectionEntries = [];
    private readonly ToolTip _navigationToolTip = new();
    private readonly TableLayoutPanel _contentRegion = new();
    private readonly Panel _contentPanel = new();
    private readonly Panel _pageHost = new();
    private readonly Panel _timerStrip = new();
    private readonly Label _pageTitleLabel = new();
    private readonly Label _breadcrumbLabel = new();
    private readonly Label _databaseStatusLabel = new();
    private readonly Label _applicationNameLabel = new();
    private readonly Label _phaseLabel = new();
    private readonly DarkButton _sidebarToggleButton = new();
    private readonly DarkButton _notificationSummaryButton = new();
    private readonly BackupStatusIndicator _backupStatus = new();
    private readonly CurrentUserMenu _currentUserMenu = new();
    private readonly NotificationArea _notificationArea = new();
    private readonly LoadingOverlay _loadingOverlay = new();

    private TableLayoutPanel? _rootLayout;
    private ColumnStyle? _sidebarColumnStyle;
    private FlowLayoutPanel? _sidebarMenu;
    private DarkButton? _selectedNavigationButton;
    private CancellationTokenSource? _pageLoadCancellation;
    private UserControl? _activePage;
    private string? _activeRouteKey;
    private long _navigationRequestId;
    private bool _sidebarCollapsed;
    private bool _userPrefersCollapsedSidebar;
    private bool _responsiveCollapseRequired;
    private bool _layoutBuilt;
    private bool _healthCheckStarted;
    private bool _isDisposed;

    public MainShellForm(
        IDatabaseHealthService databaseHealthService,
        ILogger<MainShellForm>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(databaseHealthService);
        _databaseHealthService = databaseHealthService;
        _logger = logger ?? NullLogger<MainShellForm>.Instance;

        Text = "Personal Business Manager";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(
            UiDimensions.MinimumWindowWidth,
            UiDimensions.MinimumWindowHeight);
        WindowState = FormWindowState.Maximized;
        DoubleBuffered = true;
        KeyPreview = true;

        ControlStyler.StyleForm(this);
        BuildLayout();
        _layoutBuilt = true;
        _ = NavigateAsync("dashboard").GetAwaiter().GetResult();
        ThemeManager.Apply(this);
        ApplyResponsiveLayout();

        Shown += MainShellForm_Shown;
    }

    public event EventHandler? NavigationCompleted;

    public event EventHandler? SidebarStateChanged;

    public IReadOnlyList<string> NavigationKeys =>
        _navigationEntries.Keys.ToArray();

    public string? ActiveRouteKey => _activeRouteKey;

    public UserControl? ActivePage => _activePage;

    public bool IsSidebarCollapsed => _sidebarCollapsed;

    public bool IsPageLoading => _loadingOverlay.IsActive;

    public bool IsTimerStripVisible => _timerStrip.Visible;

    public int SidebarWidth => _sidebarColumnStyle is null
        ? 0
        : (int)_sidebarColumnStyle.Width;

    public int ContentHorizontalPadding =>
        _contentPanel.Padding.Left;

    public bool IsResponsiveCollapseRequired =>
        _responsiveCollapseRequired;

    public int ActiveNotificationCount =>
        _notificationArea.ActiveNotificationCount;

    public BackupStatusSnapshot BackupStatus =>
        _backupStatus.Snapshot;

    public CurrentUserMenu CurrentUserMenu => _currentUserMenu;

    public string CurrentPageTitle => _pageTitleLabel.Text;

    public string CurrentBreadcrumb => _breadcrumbLabel.Text;

    public void RegisterPageDefinition(
        ShellPageDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _pageDefinitions[definition.Key] = definition;
    }

    public async Task<bool> NavigateAsync(
        string routeKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeKey);
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (!_pageDefinitions.TryGetValue(
                routeKey,
                out ShellPageDefinition? definition))
        {
            throw new KeyNotFoundException(
                $"No shell page is registered for route '{routeKey}'.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        bool canLeave;

        try
        {
            canLeave = await CanLeaveActivePageAsync(
                cancellationToken);
        }
        catch (Exception exception)
        {
            LogNavigationFailure(
                _logger,
                _activeRouteKey ?? "navigation-guard",
                exception);
            ShowNotification(
                new ShellNotification(
                    "The current page could not confirm whether navigation is safe.",
                    ShellNotificationSeverity.Error));
            return false;
        }

        if (!canLeave)
        {
            ShowNotification(
                new ShellNotification(
                    "Finish or discard the current changes before navigating.",
                    ShellNotificationSeverity.Warning));
            return false;
        }

        CancellationTokenSource linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);
        CancellationTokenSource? obsoleteCancellation =
            _pageLoadCancellation;
        _pageLoadCancellation = linkedCancellation;
        obsoleteCancellation?.Cancel();
        obsoleteCancellation?.Dispose();

        long requestId = ++_navigationRequestId;
        UserControl? loadedPage = null;
        SetLoadingState(
            true,
            $"{definition.Title} is loading…");

        try
        {
            loadedPage = await definition.CreatePageAsync(
                linkedCancellation.Token);
            ArgumentNullException.ThrowIfNull(loadedPage);
            linkedCancellation.Token.ThrowIfCancellationRequested();

            if (requestId != _navigationRequestId)
            {
                loadedPage.Dispose();
                return false;
            }

            CaptureActivePageState();
            PreparePage(loadedPage, definition.Title);
            ReplaceActivePage(loadedPage);
            loadedPage = null;
            RestorePageState(definition.Key);
            ApplySuccessfulNavigation(definition);
            MoveFocusIntoActivePage();
            NavigationCompleted?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (OperationCanceledException)
            when (linkedCancellation.IsCancellationRequested)
        {
            loadedPage?.Dispose();
            return false;
        }
        catch (Exception exception)
        {
            loadedPage?.Dispose();
            LogNavigationFailure(
                _logger,
                definition.Key,
                exception);

            if (requestId == _navigationRequestId)
            {
                ShowPageLoadFailure(definition, requestId);
            }

            return false;
        }
        finally
        {
            if (requestId == _navigationRequestId)
            {
                SetLoadingState(false, string.Empty);
                _pageLoadCancellation = null;
            }

            linkedCancellation.Dispose();
        }
    }

    public void CancelPageLoading()
    {
        _pageLoadCancellation?.Cancel();
    }

    public void SetSidebarCollapsed(bool collapsed)
    {
        _userPrefersCollapsedSidebar = collapsed;
        ApplyResponsiveLayout();
    }

    public Guid ShowNotification(ShellNotification notification)
    {
        Guid notificationId =
            _notificationArea.ShowNotification(notification);
        PositionNotificationArea();
        UpdateNotificationSummary();
        return notificationId;
    }

    public bool DismissNotification(Guid notificationId)
    {
        bool dismissed = _notificationArea
            .DismissNotification(notificationId);
        UpdateNotificationSummary();
        return dismissed;
    }

    public void UpdateBackupStatus(BackupStatusSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _backupStatus.Snapshot = snapshot;
    }

    protected override bool ProcessCmdKey(
        ref Message msg,
        Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.B)
            && !_responsiveCollapseRequired)
        {
            _userPrefersCollapsedSidebar =
                !_userPrefersCollapsedSidebar;
            ApplyResponsiveLayout();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);

        if (_layoutBuilt)
        {
            ApplyResponsiveLayout();
            PositionNotificationArea();
        }
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        ApplyResponsiveLayout();
        PerformLayout();
        Invalidate(true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _isDisposed = true;
            Shown -= MainShellForm_Shown;
            _pageLoadCancellation?.Cancel();
            _pageLoadCancellation?.Dispose();
            _pageLoadCancellation = null;
            _notificationArea.DismissAll();
            _navigationToolTip.Dispose();
        }

        base.Dispose(disposing);
    }

    private void BuildLayout()
    {
        _rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(
            _rootLayout,
            ThemeSurface.Application);

        _sidebarColumnStyle = new ColumnStyle(
            SizeType.Absolute,
            UiDimensions.ExpandedSidebarWidth);
        _rootLayout.ColumnStyles.Add(_sidebarColumnStyle);
        _rootLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(BuildSidebar(), 0, 0);
        _rootLayout.Controls.Add(BuildMainArea(), 1, 0);
        Controls.Add(_rootLayout);
    }

    private TableLayoutPanel BuildSidebar()
    {
        var sidebar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
        };
        ControlStyler.StylePanel(sidebar, ThemeSurface.Sidebar);
        sidebar.RowStyles.Add(new RowStyle(
            SizeType.Absolute,
            UiDimensions.HeaderHeight));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sidebar.RowStyles.Add(new RowStyle(
            SizeType.Absolute,
            UiDimensions.TimerStripHeight));

        _applicationNameLabel.Dock = DockStyle.Fill;
        _applicationNameLabel.Text = "PBM";
        _applicationNameLabel.TextAlign = ContentAlignment.MiddleLeft;
        _applicationNameLabel.Padding = new Padding(
            UiSpacing.Space16,
            0,
            0,
            0);
        _applicationNameLabel.Margin = Padding.Empty;
        ControlStyler.StyleLabel(
            _applicationNameLabel,
            ThemeTextRole.DialogHeading);

        _sidebarMenu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(UiSpacing.Space16),
            Margin = Padding.Empty,
        };
        ControlStyler.StylePanel(
            _sidebarMenu,
            ThemeSurface.Sidebar);

        AddNavigationButton(
            "dashboard",
            "Dashboard",
            "D",
            "Home / Dashboard",
            static () => new DashboardPage());

        AddSectionHeading("WORK");
        AddPlaceholderNavigation(
            "customers",
            "Customers",
            "C",
            "Work / Customers",
            "Customer management will be implemented in Phase 4.");
        AddPlaceholderNavigation(
            "jobs",
            "Jobs",
            "J",
            "Work / Jobs",
            "Job management will be implemented in Phase 4.");
        AddPlaceholderNavigation(
            "time",
            "Time",
            "Tm",
            "Work / Time",
            "Persistent time tracking will be implemented in Phase 5.");
        AddPlaceholderNavigation(
            "tasks",
            "Tasks",
            "Ts",
            "Work / Tasks",
            "Task management will be implemented in Phase 6.");

        AddSectionHeading("BUSINESS FINANCE");
        AddPlaceholderNavigation(
            "invoices",
            "Invoices",
            "I",
            "Business finance / Invoices",
            "Invoice management will be implemented in Phase 8.");
        AddPlaceholderNavigation(
            "expenses",
            "Expenses",
            "E",
            "Business finance / Expenses",
            "Expense management will be implemented in Phase 9.");
        AddPlaceholderNavigation(
            "business-reports",
            "Business Reports",
            "BR",
            "Business finance / Reports",
            "Business reporting will be implemented in Phase 10.");

        AddSectionHeading("PERSONAL FINANCE");
        AddPlaceholderNavigation(
            "accounts",
            "Accounts",
            "A",
            "Personal finance / Accounts",
            "Financial account tracking will be implemented in Phase 7.");
        AddPlaceholderNavigation(
            "applications",
            "Applications",
            "Ap",
            "Personal finance / Applications",
            "Financial account applications will be implemented in Phase 7.");
        AddPlaceholderNavigation(
            "personal-reports",
            "Personal Reports",
            "PR",
            "Personal finance / Reports",
            "Personal finance reporting will be implemented in Phase 10.");

        AddSectionHeading("SYSTEM");
        AddPlaceholderNavigation(
            "audit-history",
            "Audit History",
            "AH",
            "System / Audit history",
            "The audit foundation will be implemented in Phase 3.");
        AddPlaceholderNavigation(
            "backups",
            "Backups",
            "B",
            "System / Backups",
            "Backup and restore will be implemented in Phase 11.");
        AddPlaceholderNavigation(
            "settings",
            "Settings",
            "S",
            "System / Settings",
            "Application settings will be implemented in Phase 3.");

        _phaseLabel.Dock = DockStyle.Fill;
        _phaseLabel.Text = "Phase 2 application shell";
        _phaseLabel.TextAlign = ContentAlignment.MiddleCenter;
        _phaseLabel.Margin = Padding.Empty;
        ControlStyler.StyleLabel(
            _phaseLabel,
            ThemeTextRole.Caption,
            ThemePalette.MutedText);

        sidebar.Controls.Add(_applicationNameLabel, 0, 0);
        sidebar.Controls.Add(_sidebarMenu, 0, 1);
        sidebar.Controls.Add(_phaseLabel, 0, 2);
        return sidebar;
    }

    private TableLayoutPanel BuildMainArea()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(
            mainLayout,
            ThemeSurface.Application);
        mainLayout.RowStyles.Add(new RowStyle(
            SizeType.Absolute,
            UiDimensions.HeaderHeight));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(
            SizeType.Absolute,
            UiDimensions.TimerStripHeight));

        mainLayout.Controls.Add(BuildHeader(), 0, 0);
        BuildContentRegion();
        mainLayout.Controls.Add(_contentRegion, 0, 1);
        BuildTimerStrip();
        mainLayout.Controls.Add(_timerStrip, 0, 2);
        return mainLayout;
    }

    private TableLayoutPanel BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(
                UiSpacing.Space16,
                UiSpacing.Space4,
                UiSpacing.Space16,
                UiSpacing.Space4),
            Margin = Padding.Empty,
        };
        ControlStyler.StylePanel(header, ThemeSurface.Header);
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _sidebarToggleButton.Text = "Collapse";
        _sidebarToggleButton.Variant = ButtonVariant.Ghost;
        _sidebarToggleButton.SizeVariant = ControlSize.Compact;
        _sidebarToggleButton.Anchor = AnchorStyles.Left;
        _sidebarToggleButton.Margin = new Padding(
            0,
            0,
            UiSpacing.Space8,
            0);
        _sidebarToggleButton.AccessibleName = "Toggle sidebar";
        _sidebarToggleButton.Click += (_, _) =>
        {
            if (_responsiveCollapseRequired)
            {
                ShowNotification(
                    new ShellNotification(
                        "The sidebar remains compact at this window width.",
                        ShellNotificationSeverity.Information));
                return;
            }

            _userPrefersCollapsedSidebar = !_sidebarCollapsed;
            ApplyResponsiveLayout();
        };

        var titlePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
        };
        ControlStyler.StylePanel(titlePanel, ThemeSurface.Header);
        titlePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        titlePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _pageTitleLabel.AutoSize = true;
        _pageTitleLabel.Text = "Dashboard";
        _pageTitleLabel.Margin = Padding.Empty;
        _pageTitleLabel.AutoEllipsis = true;
        ControlStyler.StyleLabel(
            _pageTitleLabel,
            ThemeTextRole.PageHeading);
        _breadcrumbLabel.AutoSize = true;
        _breadcrumbLabel.Text = "Home / Dashboard";
        _breadcrumbLabel.Margin = Padding.Empty;
        _breadcrumbLabel.AutoEllipsis = true;
        ControlStyler.StyleLabel(
            _breadcrumbLabel,
            ThemeTextRole.Small,
            ThemePalette.SecondaryText);
        titlePanel.Controls.Add(_pageTitleLabel, 0, 0);
        titlePanel.Controls.Add(_breadcrumbLabel, 0, 1);

        var statusPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(statusPanel, ThemeSurface.Header);

        _currentUserMenu.Margin = Padding.Empty;
        _backupStatus.Margin = new Padding(
            0,
            0,
            UiSpacing.Space8,
            0);
        _backupStatus.Click += async (_, _) =>
            _ = await NavigateAsync("backups");

        _notificationSummaryButton.Text = "Notices 0";
        _notificationSummaryButton.Variant = ButtonVariant.Ghost;
        _notificationSummaryButton.SizeVariant = ControlSize.Compact;
        _notificationSummaryButton.Margin = new Padding(
            0,
            0,
            UiSpacing.Space8,
            0);
        _notificationSummaryButton.AccessibleName =
            "Notifications, none";
        _notificationSummaryButton.Click += (_, _) =>
            _notificationArea.FocusLatest();

        _databaseStatusLabel.AutoSize = false;
        _databaseStatusLabel.Width =
            UiDimensions.HeaderStatusControlWidth;
        _databaseStatusLabel.Height =
            UiDimensions.StandardControlHeight;
        _databaseStatusLabel.Text = "Database: checking";
        _databaseStatusLabel.TextAlign =
            ContentAlignment.MiddleRight;
        _databaseStatusLabel.Margin = new Padding(
            0,
            0,
            UiSpacing.Space8,
            0);
        _databaseStatusLabel.AutoEllipsis = true;
        ControlStyler.StyleLabel(
            _databaseStatusLabel,
            ThemeTextRole.Small,
            ThemePalette.Warning);

        statusPanel.Controls.Add(_currentUserMenu);
        statusPanel.Controls.Add(_backupStatus);
        statusPanel.Controls.Add(_notificationSummaryButton);
        statusPanel.Controls.Add(_databaseStatusLabel);
        header.Controls.Add(_sidebarToggleButton, 0, 0);
        header.Controls.Add(titlePanel, 1, 0);
        header.Controls.Add(statusPanel, 2, 0);
        return header;
    }

    private void BuildContentRegion()
    {
        _contentRegion.Dock = DockStyle.Fill;
        _contentRegion.ColumnCount = 1;
        _contentRegion.RowCount = 2;
        _contentRegion.Margin = Padding.Empty;
        _contentRegion.Padding = Padding.Empty;
        _contentRegion.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));
        _contentRegion.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));
        _contentRegion.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));
        ControlStyler.StylePanel(
            _contentRegion,
            ThemeSurface.Application);

        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.Margin = Padding.Empty;
        _contentPanel.Padding = new Padding(UiSpacing.Space24);
        ControlStyler.StylePanel(
            _contentPanel,
            ThemeSurface.Application);
        _pageHost.Dock = DockStyle.Fill;
        _pageHost.Margin = Padding.Empty;
        _pageHost.Padding = Padding.Empty;
        ControlStyler.StylePanel(
            _pageHost,
            ThemeSurface.Application);

        _loadingOverlay.MessageText = "Loading page…";
        _loadingOverlay.CanCancel = true;
        _loadingOverlay.CancelRequested += (_, _) =>
            CancelPageLoading();

        _notificationArea.Visible = false;
        _notificationArea.Anchor =
            AnchorStyles.Top | AnchorStyles.Right;
        _notificationArea.Margin = new Padding(
            0,
            UiSpacing.Space16,
            UiSpacing.Space16,
            UiSpacing.Space8);
        _notificationArea.NotificationsChanged += (_, _) =>
        {
            UpdateNotificationSummary();
            PositionNotificationArea();
        };

        _contentPanel.Controls.Add(_pageHost);
        _contentPanel.Controls.Add(_loadingOverlay);
        _contentRegion.Controls.Add(_notificationArea, 0, 0);
        _contentRegion.Controls.Add(_contentPanel, 0, 1);
    }

    private void BuildTimerStrip()
    {
        _timerStrip.Dock = DockStyle.Fill;
        _timerStrip.Padding = new Padding(
            UiSpacing.Space24,
            0,
            UiSpacing.Space24,
            0);
        _timerStrip.Margin = Padding.Empty;
        _timerStrip.TabStop = false;
        _timerStrip.AccessibleRole = AccessibleRole.Grouping;
        _timerStrip.AccessibleName = "Persistent timer strip";
        ControlStyler.StylePanel(
            _timerStrip,
            ThemeSurface.Header);

        var timerLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Timer: no active timer — available from Phase 5",
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty,
        };
        ControlStyler.StyleLabel(
            timerLabel,
            ThemeTextRole.Label,
            ThemePalette.SecondaryText);
        _timerStrip.Controls.Add(timerLabel);
    }

    private void AddPlaceholderNavigation(
        string key,
        string title,
        string compactText,
        string breadcrumb,
        string description)
    {
        AddNavigationButton(
            key,
            title,
            compactText,
            breadcrumb,
            () => new PlaceholderPage(title, description));
    }

    private void AddNavigationButton(
        string key,
        string title,
        string compactText,
        string breadcrumb,
        Func<UserControl> pageFactory)
    {
        if (_sidebarMenu is null)
        {
            throw new InvalidOperationException(
                "The sidebar menu has not been created.");
        }

        var definition = ShellPageDefinition
            .FromSynchronousFactory(
                key,
                title,
                breadcrumb,
                pageFactory);
        RegisterPageDefinition(definition);

        var button = new DarkButton
        {
            Text = title,
            Margin = Padding.Empty,
            IsNavigationItem = true,
            AccessibleName = title,
            AccessibleDescription = $"Navigate to {breadcrumb}.",
            Tag = key,
        };
        button.Click += async (_, _) =>
            _ = await NavigateAsync(key);
        button.KeyDown += NavigationButton_KeyDown;
        _navigationToolTip.SetToolTip(button, title);
        _sidebarMenu.Controls.Add(button);
        _navigationEntries.Add(
            key,
            new NavigationEntry(
                button,
                title,
                compactText));
    }

    private void AddSectionHeading(string text)
    {
        if (_sidebarMenu is null)
        {
            throw new InvalidOperationException(
                "The sidebar menu has not been created.");
        }

        var heading = new Label
        {
            Width = UiDimensions.ExpandedSidebarWidth
                - (UiSpacing.Space16 * 2),
            Height = UiDimensions.CompactControlHeight,
            Margin = new Padding(
                0,
                UiSpacing.Space16,
                0,
                0),
            Text = text,
            TextAlign = ContentAlignment.BottomLeft,
            AccessibleName = $"{text} section",
        };
        ControlStyler.StyleLabel(
            heading,
            ThemeTextRole.Small,
            ThemePalette.MutedText);
        _sidebarMenu.Controls.Add(heading);
        _sectionEntries.Add(new SectionEntry(heading, text));
    }

    private async ValueTask<bool> CanLeaveActivePageAsync(
        CancellationToken cancellationToken)
    {
        if (_activePage is not IShellNavigationGuard guard)
        {
            return true;
        }

        try
        {
            return await guard.CanNavigateAwayAsync(
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private static void PreparePage(UserControl page, string title)
    {
        page.Dock = DockStyle.Fill;
        page.Margin = Padding.Empty;
        page.TabStop = true;
        page.AccessibleName = string.IsNullOrWhiteSpace(
            page.AccessibleName)
            ? $"{title} page"
            : page.AccessibleName;
        ThemeManager.ApplyControlTree(page);
    }

    private void ReplaceActivePage(UserControl page)
    {
        UserControl? outgoingPage = _activePage;
        _pageHost.Controls.Clear();
        _activePage = page;
        _pageHost.Controls.Add(page);
        outgoingPage?.Dispose();
    }

    private void CaptureActivePageState()
    {
        if (_activePage is IShellNavigationStatefulPage stateful
            && !string.IsNullOrWhiteSpace(_activeRouteKey))
        {
            _navigationState[_activeRouteKey] =
                stateful.CaptureNavigationState();
        }
    }

    private void RestorePageState(string routeKey)
    {
        if (_activePage is IShellNavigationStatefulPage stateful
            && _navigationState.TryGetValue(routeKey, out object? state))
        {
            stateful.RestoreNavigationState(state);
        }
    }

    private void ApplySuccessfulNavigation(
        ShellPageDefinition definition)
    {
        _activeRouteKey = definition.Key;
        _pageTitleLabel.Text = definition.Title;
        _breadcrumbLabel.Text = definition.Breadcrumb;

        if (_navigationEntries.TryGetValue(
                definition.Key,
                out NavigationEntry? entry))
        {
            SelectNavigationButton(entry.Button);
        }
    }

    private void ShowPageLoadFailure(
        ShellPageDefinition definition,
        long requestId)
    {
        CaptureActivePageState();
        var error = new EmptyStatePanel
        {
            Dock = DockStyle.Fill,
            StateKind = ContentStateKind.Error,
            HeadingText = $"{definition.Title} could not be loaded",
            DescriptionText =
                "The page did not load. Try the operation again.",
            TechnicalReference = $"Reference: PAGE-{requestId:D6}",
            PrimaryActionText = "Retry",
            SecondaryActionText = string.Empty,
        };
        error.PrimaryActionClicked += async (_, _) =>
            _ = await NavigateAsync(definition.Key);
        PreparePage(error, definition.Title);
        ReplaceActivePage(error);
        ApplySuccessfulNavigation(definition);
        ShowNotification(
            new ShellNotification(
                $"{definition.Title} could not be loaded.",
                ShellNotificationSeverity.Error,
                "Retry",
                () => _ = NavigateAsync(definition.Key)));
        MoveFocusIntoActivePage();
    }

    private void MoveFocusIntoActivePage()
    {
        if (_activePage is null)
        {
            return;
        }

        Control? firstSelectable = Descendants(_activePage)
            .FirstOrDefault(control =>
                control.Visible
                && control.Enabled
                && control.TabStop
                && control.CanSelect);

        if (firstSelectable is not null)
        {
            _ = firstSelectable.Focus();
        }
        else
        {
            _ = _activePage.Focus();
        }
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;

            foreach (Control descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private void SelectNavigationButton(DarkButton selectedButton)
    {
        if (_selectedNavigationButton is not null)
        {
            _selectedNavigationButton.IsSelected = false;
        }

        selectedButton.IsSelected = true;
        _selectedNavigationButton = selectedButton;
    }

    private void SetLoadingState(bool isLoading, string message)
    {
        _loadingOverlay.MessageText = isLoading
            ? message
            : "Loading page…";
        _pageHost.Visible = !isLoading;
        _loadingOverlay.IsActive = isLoading;

        if (isLoading)
        {
            _loadingOverlay.BringToFront();
            _notificationArea.BringToFront();
        }
    }

    private void ApplyResponsiveLayout()
    {
        if (!_layoutBuilt
            || _rootLayout is null
            || _sidebarColumnStyle is null
            || _sidebarMenu is null)
        {
            return;
        }

        _responsiveCollapseRequired =
            ClientSize.Width < UiDimensions.ResponsiveWidth;
        bool collapsed = _responsiveCollapseRequired
            || _userPrefersCollapsedSidebar;
        bool changed = collapsed != _sidebarCollapsed;
        _sidebarCollapsed = collapsed;

        _sidebarColumnStyle.Width = collapsed
            ? UiDimensions.CollapsedSidebarWidth
            : UiDimensions.ExpandedSidebarWidth;
        _sidebarMenu.Padding = new Padding(
            collapsed
                ? UiSpacing.Space8
                : UiSpacing.Space16);
        _applicationNameLabel.Text = "PBM";
        _applicationNameLabel.TextAlign = collapsed
            ? ContentAlignment.MiddleCenter
            : ContentAlignment.MiddleLeft;
        _applicationNameLabel.Padding = collapsed
            ? Padding.Empty
            : new Padding(UiSpacing.Space16, 0, 0, 0);
        _phaseLabel.Text = collapsed ? "P2" : "Phase 2 application shell";

        foreach (NavigationEntry entry in _navigationEntries.Values)
        {
            entry.Button.IsCompactNavigation = collapsed;
            entry.Button.Text = collapsed
                ? entry.CompactText
                : entry.FullText;
            entry.Button.AccessibleName = entry.FullText;
            _navigationToolTip.SetToolTip(
                entry.Button,
                entry.FullText);
        }

        foreach (SectionEntry entry in _sectionEntries)
        {
            entry.Label.Width = collapsed
                ? UiDimensions.CollapsedSidebarWidth
                    - (UiSpacing.Space8 * 2)
                : UiDimensions.ExpandedSidebarWidth
                    - (UiSpacing.Space16 * 2);
            entry.Label.Text = collapsed ? "—" : entry.FullText;
            entry.Label.TextAlign = collapsed
                ? ContentAlignment.BottomCenter
                : ContentAlignment.BottomLeft;
        }

        _sidebarToggleButton.Enabled = true;
        _sidebarToggleButton.Text = _responsiveCollapseRequired
            ? "Compact"
            : collapsed
                ? "Expand"
                : "Collapse";
        _sidebarToggleButton.AccessibleDescription =
            _responsiveCollapseRequired
                ? "The sidebar remains compact below the responsive width."
                : "Collapse or expand the permanent navigation sidebar.";
        _contentPanel.Padding = new Padding(
            collapsed ? UiSpacing.Space16 : UiSpacing.Space24,
            UiSpacing.Space24,
            collapsed ? UiSpacing.Space16 : UiSpacing.Space24,
            UiSpacing.Space24);

        _rootLayout.PerformLayout();
        PerformLayout();
        _applicationNameLabel.Invalidate();
        _phaseLabel.Invalidate();

        if (changed)
        {
            SidebarStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void PositionNotificationArea()
    {
        _notificationArea.BringToFront();
    }

    private void UpdateNotificationSummary()
    {
        int count = _notificationArea.ActiveNotificationCount;
        _notificationSummaryButton.Text = $"Notices {count}";
        _notificationSummaryButton.AccessibleName = count == 0
            ? "Notifications, none"
            : $"Notifications, {count} active";
    }

    private void NavigationButton_KeyDown(
        object? sender,
        KeyEventArgs eventArgs)
    {
        if (sender is not DarkButton current
            || eventArgs.KeyCode is not Keys.Up and not Keys.Down)
        {
            return;
        }

        DarkButton[] buttons = _navigationEntries.Values
            .Select(entry => entry.Button)
            .ToArray();
        int currentIndex = Array.IndexOf(buttons, current);

        if (currentIndex < 0)
        {
            return;
        }

        int offset = eventArgs.KeyCode == Keys.Down ? 1 : -1;
        int nextIndex = (currentIndex + offset + buttons.Length)
            % buttons.Length;
        _ = buttons[nextIndex].Focus();
        eventArgs.Handled = true;
        eventArgs.SuppressKeyPress = true;
    }

    private async void MainShellForm_Shown(
        object? sender,
        EventArgs eventArgs)
    {
        if (_healthCheckStarted)
        {
            return;
        }

        _healthCheckStarted = true;
        _databaseStatusLabel.Text = "Database: checking";
        _databaseStatusLabel.ForeColor = ThemePalette.Warning;

        try
        {
            using var timeout = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));
            DatabaseHealthResult result =
                await _databaseHealthService.CheckAsync(timeout.Token);

            if (_isDisposed)
            {
                return;
            }

            _databaseStatusLabel.Text = $"Database: {result.Message}";
            _databaseStatusLabel.ForeColor = result.IsAvailable
                ? ThemePalette.Success
                : ThemePalette.Danger;

            if (!result.IsAvailable)
            {
                ShowNotification(
                    new ShellNotification(
                        "Database writes are unavailable until the connection recovers.",
                        ShellNotificationSeverity.Error));
            }
        }
        catch (Exception exception)
        {
            if (_isDisposed)
            {
                return;
            }

            LogDatabaseHealthFailure(_logger, exception);
            _databaseStatusLabel.Text =
                "Database: connection check failed";
            _databaseStatusLabel.ForeColor = ThemePalette.Danger;
            ShowNotification(
                new ShellNotification(
                    "The database connection check failed. Writes remain unavailable.",
                    ShellNotificationSeverity.Error));
        }
    }

    private sealed record NavigationEntry(
        DarkButton Button,
        string FullText,
        string CompactText);

    private sealed record SectionEntry(
        Label Label,
        string FullText);
}
