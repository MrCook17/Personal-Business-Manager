using PersonalBusinessManager.Core.Application.Contracts;
using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Pages;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Forms;

public sealed class MainShellForm : Form
{
    private readonly IDatabaseHealthService _databaseHealthService;
    private readonly Panel _contentPanel = new();
    private readonly Label _pageTitleLabel = new();
    private readonly Label _breadcrumbLabel = new();
    private readonly Label _databaseStatusLabel = new();

    private DarkButton? _selectedNavigationButton;

    public MainShellForm(
        IDatabaseHealthService databaseHealthService)
    {
        ArgumentNullException.ThrowIfNull(databaseHealthService);
        _databaseHealthService = databaseHealthService;

        Text = "Personal Business Manager";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(
            UiDimensions.MinimumWindowWidth,
            UiDimensions.MinimumWindowHeight);
        WindowState = FormWindowState.Maximized;
        DoubleBuffered = true;

        ControlStyler.StyleForm(this);
        BuildLayout();
        ShowPage(
            new DashboardPage(),
            "Dashboard",
            "Home / Dashboard");
        ThemeManager.Apply(this);

        Shown += MainShellForm_Shown;
    }

    private void BuildLayout()
    {
        var rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(
            rootLayout,
            ThemeSurface.Application);

        rootLayout.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                UiDimensions.ExpandedSidebarWidth));
        rootLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        rootLayout.Controls.Add(BuildSidebar(), 0, 0);
        rootLayout.Controls.Add(BuildMainArea(), 1, 0);
        Controls.Add(rootLayout);
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
        ControlStyler.StylePanel(
            sidebar,
            ThemeSurface.Sidebar);

        sidebar.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                UiDimensions.HeaderHeight));
        sidebar.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));
        sidebar.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                UiDimensions.TimerStripHeight));

        var applicationName = new Label
        {
            Dock = DockStyle.Fill,
            Text = "PBM",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(
                UiSpacing.Space16,
                0,
                0,
                0),
            Margin = Padding.Empty,
        };
        ControlStyler.StyleLabel(
            applicationName,
            ThemeTextRole.DialogHeading);

        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(UiSpacing.Space16),
            Margin = Padding.Empty,
        };
        ControlStyler.StylePanel(
            menu,
            ThemeSurface.Sidebar);

        DarkButton dashboardButton = AddNavigationButton(
            menu,
            "Dashboard",
            "Home / Dashboard",
            static () => new DashboardPage());
        SelectNavigationButton(dashboardButton);

        AddSectionHeading(menu, "WORK");

        AddNavigationButton(
            menu,
            "Customers",
            "Work / Customers",
            static () => new PlaceholderPage(
                "Customers",
                "Customer management will be implemented in Phase 4."));

        AddNavigationButton(
            menu,
            "Jobs",
            "Work / Jobs",
            static () => new PlaceholderPage(
                "Jobs",
                "Job management will be implemented in Phase 4."));

        AddNavigationButton(
            menu,
            "Time",
            "Work / Time",
            static () => new PlaceholderPage(
                "Time",
                "Persistent time tracking will be implemented in Phase 5."));

        AddNavigationButton(
            menu,
            "Tasks",
            "Work / Tasks",
            static () => new PlaceholderPage(
                "Tasks",
                "Task management will be implemented in Phase 6."));

        AddSectionHeading(menu, "BUSINESS FINANCE");

        AddNavigationButton(
            menu,
            "Invoices",
            "Business finance / Invoices",
            static () => new PlaceholderPage(
                "Invoices",
                "Invoice management will be implemented in Phase 8."));

        AddNavigationButton(
            menu,
            "Expenses",
            "Business finance / Expenses",
            static () => new PlaceholderPage(
                "Expenses",
                "Expense management will be implemented in Phase 9."));

        AddNavigationButton(
            menu,
            "Business Reports",
            "Business finance / Reports",
            static () => new PlaceholderPage(
                "Business reports",
                "Business reporting will be implemented in Phase 10."));

        AddSectionHeading(menu, "PERSONAL FINANCE");

        AddNavigationButton(
            menu,
            "Accounts",
            "Personal finance / Accounts",
            static () => new PlaceholderPage(
                "Accounts",
                "Financial account tracking will be implemented in Phase 7."));

        AddNavigationButton(
            menu,
            "Applications",
            "Personal finance / Applications",
            static () => new PlaceholderPage(
                "Applications",
                "Financial account applications will be implemented in Phase 7."));

        AddNavigationButton(
            menu,
            "Personal Reports",
            "Personal finance / Reports",
            static () => new PlaceholderPage(
                "Personal reports",
                "Personal finance reporting will be implemented in Phase 10."));

        AddSectionHeading(menu, "SYSTEM");

        AddNavigationButton(
            menu,
            "Audit History",
            "System / Audit history",
            static () => new PlaceholderPage(
                "Audit history",
                "The audit foundation will be implemented in Phase 3."));

        AddNavigationButton(
            menu,
            "Backups",
            "System / Backups",
            static () => new PlaceholderPage(
                "Backups",
                "Backup and restore will be implemented in Phase 11."));

        AddNavigationButton(
            menu,
            "Settings",
            "System / Settings",
            static () => new PlaceholderPage(
                "Settings",
                "Application settings will be implemented in Phase 3."));

        var phaseLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Phase 2 application shell",
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = Padding.Empty,
        };
        ControlStyler.StyleLabel(
            phaseLabel,
            ThemeTextRole.Caption,
            ThemePalette.MutedText);

        sidebar.Controls.Add(applicationName, 0, 0);
        sidebar.Controls.Add(menu, 0, 1);
        sidebar.Controls.Add(phaseLabel, 0, 2);

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

        mainLayout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                UiDimensions.HeaderHeight));
        mainLayout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                UiDimensions.TimerStripHeight));

        mainLayout.Controls.Add(BuildHeader(), 0, 0);

        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.Padding = new Padding(UiSpacing.Space24);
        _contentPanel.Margin = Padding.Empty;
        ControlStyler.StylePanel(
            _contentPanel,
            ThemeSurface.Application);

        mainLayout.Controls.Add(_contentPanel, 0, 1);
        mainLayout.Controls.Add(BuildTimerStrip(), 0, 2);

        return mainLayout;
    }

    private TableLayoutPanel BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(
                UiSpacing.Space24,
                UiSpacing.Space4,
                UiSpacing.Space24,
                UiSpacing.Space4),
            Margin = Padding.Empty,
        };
        ControlStyler.StylePanel(
            header,
            ThemeSurface.Header);

        header.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 65F));
        header.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 35F));

        var titlePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
        };
        ControlStyler.StylePanel(
            titlePanel,
            ThemeSurface.Header);
        titlePanel.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));
        titlePanel.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));

        _pageTitleLabel.AutoSize = true;
        _pageTitleLabel.Text = "Dashboard";
        _pageTitleLabel.Margin = Padding.Empty;
        ControlStyler.StyleLabel(
            _pageTitleLabel,
            ThemeTextRole.PageHeading);

        _breadcrumbLabel.AutoSize = true;
        _breadcrumbLabel.Text = "Home / Dashboard";
        _breadcrumbLabel.Margin = Padding.Empty;
        ControlStyler.StyleLabel(
            _breadcrumbLabel,
            ThemeTextRole.Small,
            ThemePalette.SecondaryText);

        titlePanel.Controls.Add(_pageTitleLabel, 0, 0);
        titlePanel.Controls.Add(_breadcrumbLabel, 0, 1);

        var statusPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
        };
        ControlStyler.StylePanel(
            statusPanel,
            ThemeSurface.Header);
        statusPanel.RowStyles.Add(
            new RowStyle(SizeType.Percent, 50F));
        statusPanel.RowStyles.Add(
            new RowStyle(SizeType.Percent, 50F));

        _databaseStatusLabel.Dock = DockStyle.Fill;
        _databaseStatusLabel.Text = "Database: checking";
        _databaseStatusLabel.TextAlign =
            ContentAlignment.MiddleRight;
        _databaseStatusLabel.Margin = Padding.Empty;
        ControlStyler.StyleLabel(
            _databaseStatusLabel,
            ThemeTextRole.Small,
            ThemePalette.Warning);

        var backupStatusLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Backup: not configured",
            TextAlign = ContentAlignment.MiddleRight,
            Margin = Padding.Empty,
        };
        ControlStyler.StyleLabel(
            backupStatusLabel,
            ThemeTextRole.Small,
            ThemePalette.MutedText);

        statusPanel.Controls.Add(_databaseStatusLabel, 0, 0);
        statusPanel.Controls.Add(backupStatusLabel, 0, 1);
        header.Controls.Add(titlePanel, 0, 0);
        header.Controls.Add(statusPanel, 1, 0);

        return header;
    }

    private static Panel BuildTimerStrip()
    {
        var timerStrip = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(
                UiSpacing.Space24,
                0,
                UiSpacing.Space24,
                0),
            Margin = Padding.Empty,
        };
        ControlStyler.StylePanel(
            timerStrip,
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

        timerStrip.Controls.Add(timerLabel);
        return timerStrip;
    }

    private DarkButton AddNavigationButton(
        FlowLayoutPanel menu,
        string pageTitle,
        string breadcrumb,
        Func<UserControl> pageFactory)
    {
        var button = new DarkButton
        {
            Text = pageTitle,
            Margin = Padding.Empty,
        };

        button.Click += (_, _) =>
        {
            SelectNavigationButton(button);
            ShowPage(
                pageFactory(),
                pageTitle,
                breadcrumb);
        };

        menu.Controls.Add(button);
        return button;
    }

    private static void AddSectionHeading(
        FlowLayoutPanel menu,
        string text)
    {
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
        };
        ControlStyler.StyleLabel(
            heading,
            ThemeTextRole.Small,
            ThemePalette.MutedText);

        menu.Controls.Add(heading);
    }

    private void SelectNavigationButton(
        DarkButton selectedButton)
    {
        if (_selectedNavigationButton is not null)
        {
            _selectedNavigationButton.IsSelected = false;
        }

        selectedButton.IsSelected = true;
        _selectedNavigationButton = selectedButton;
    }

    private void ShowPage(
        UserControl page,
        string pageTitle,
        string breadcrumb)
    {
        ArgumentNullException.ThrowIfNull(page);

        Control[] existingControls =
            _contentPanel.Controls
                .Cast<Control>()
                .ToArray();

        _contentPanel.Controls.Clear();

        foreach (Control existingControl in existingControls)
        {
            existingControl.Dispose();
        }

        page.Dock = DockStyle.Fill;
        ThemeManager.ApplyControlTree(page);
        _contentPanel.Controls.Add(page);

        _pageTitleLabel.Text = pageTitle;
        _breadcrumbLabel.Text = breadcrumb;
    }

    protected override void OnDpiChanged(
        DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        PerformLayout();
        Invalidate(true);
    }

    private async void MainShellForm_Shown(
        object? sender,
        EventArgs eventArgs)
    {
        _databaseStatusLabel.Text = "Database: checking";
        _databaseStatusLabel.ForeColor = ThemePalette.Warning;

        try
        {
            using var timeout = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));
            DatabaseHealthResult result =
                await _databaseHealthService.CheckAsync(
                    timeout.Token);

            _databaseStatusLabel.Text =
                $"Database: {result.Message}";
            _databaseStatusLabel.ForeColor =
                result.IsAvailable
                    ? ThemePalette.Success
                    : ThemePalette.Danger;
        }
        catch (Exception)
        {
            _databaseStatusLabel.Text =
                "Database: connection check failed";
            _databaseStatusLabel.ForeColor =
                ThemePalette.Danger;
        }
    }
}
