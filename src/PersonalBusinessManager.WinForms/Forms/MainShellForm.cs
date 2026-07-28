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
        _databaseHealthService = databaseHealthService;

        Text = "Personal Business Manager";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        WindowState = FormWindowState.Maximized;
        BackColor = ThemePalette.ApplicationBackground;
        ForeColor = ThemePalette.PrimaryText;
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;

        BuildLayout();

        Shown += MainShellForm_Shown;
    }

    private void BuildLayout()
    {
        TableLayoutPanel rootLayout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = ThemePalette.ApplicationBackground
        };

        rootLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 220F));

        rootLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        rootLayout.Controls.Add(BuildSidebar(), 0, 0);
        rootLayout.Controls.Add(BuildMainArea(), 1, 0);

        Controls.Add(rootLayout);
    }

    private Control BuildSidebar()
    {
        TableLayoutPanel sidebar = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = ThemePalette.SidebarBackground,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };

        sidebar.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 72F));

        sidebar.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));

        sidebar.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 44F));

        Label applicationName = new()
        {
            Dock = DockStyle.Fill,
            Text = "PBM",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0),
            ForeColor = ThemePalette.PrimaryText,
            Font = new Font(
                "Segoe UI",
                17F,
                FontStyle.Bold)
        };

        FlowLayoutPanel menu = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(8, 8, 8, 8),
            BackColor = ThemePalette.SidebarBackground
        };

        AddNavigationButton(
            menu,
            "Dashboard",
            "Home / Dashboard",
            () => new DashboardPage());

        AddSectionHeading(menu, "WORK");

        AddNavigationButton(
            menu,
            "Customers",
            "Work / Customers",
            () => new PlaceholderPage(
                "Customers",
                "Customer management will be implemented in Phase 4."));

        AddNavigationButton(
            menu,
            "Jobs",
            "Work / Jobs",
            () => new PlaceholderPage(
                "Jobs",
                "Job management will be implemented in Phase 4."));

        AddNavigationButton(
            menu,
            "Time",
            "Work / Time",
            () => new PlaceholderPage(
                "Time",
                "Persistent time tracking will be implemented in Phase 5."));

        AddNavigationButton(
            menu,
            "Tasks",
            "Work / Tasks",
            () => new PlaceholderPage(
                "Tasks",
                "Task management will be implemented in Phase 6."));

        AddSectionHeading(menu, "BUSINESS FINANCE");

        AddNavigationButton(
            menu,
            "Invoices",
            "Business finance / Invoices",
            () => new PlaceholderPage(
                "Invoices",
                "Invoice management will be implemented in Phase 8."));

        AddNavigationButton(
            menu,
            "Expenses",
            "Business finance / Expenses",
            () => new PlaceholderPage(
                "Expenses",
                "Expense management will be implemented in Phase 9."));

        AddNavigationButton(
            menu,
            "Business Reports",
            "Business finance / Reports",
            () => new PlaceholderPage(
                "Business reports",
                "Business reporting will be implemented in Phase 10."));

        AddSectionHeading(menu, "PERSONAL FINANCE");

        AddNavigationButton(
            menu,
            "Accounts",
            "Personal finance / Accounts",
            () => new PlaceholderPage(
                "Accounts",
                "Financial account tracking will be implemented in Phase 7."));

        AddNavigationButton(
            menu,
            "Applications",
            "Personal finance / Applications",
            () => new PlaceholderPage(
                "Applications",
                "Financial account applications will be implemented in Phase 7."));

        AddNavigationButton(
            menu,
            "Personal Reports",
            "Personal finance / Reports",
            () => new PlaceholderPage(
                "Personal reports",
                "Personal finance reporting will be implemented in Phase 10."));

        AddSectionHeading(menu, "SYSTEM");

        AddNavigationButton(
            menu,
            "Audit History",
            "System / Audit history",
            () => new PlaceholderPage(
                "Audit history",
                "The audit foundation will be implemented in Phase 3."));

        AddNavigationButton(
            menu,
            "Backups",
            "System / Backups",
            () => new PlaceholderPage(
                "Backups",
                "Backup and restore will be implemented in Phase 11."));

        AddNavigationButton(
            menu,
            "Settings",
            "System / Settings",
            () => new PlaceholderPage(
                "Settings",
                "Application settings will be implemented in Phase 3."));

        Label phaseLabel = new()
        {
            Dock = DockStyle.Fill,
            Text = "Phase 2 application shell",
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = ThemePalette.MutedText,
            Font = new Font("Segoe UI", 8.5F)
        };

        sidebar.Controls.Add(applicationName, 0, 0);
        sidebar.Controls.Add(menu, 0, 1);
        sidebar.Controls.Add(phaseLabel, 0, 2);

        return sidebar;
    }

    private Control BuildMainArea()
    {
        TableLayoutPanel mainLayout = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = ThemePalette.ApplicationBackground,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        mainLayout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 72F));

        mainLayout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));

        mainLayout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 44F));

        mainLayout.Controls.Add(BuildHeader(), 0, 0);

        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.Padding = new Padding(24);
        _contentPanel.BackColor =
            ThemePalette.ApplicationBackground;

        mainLayout.Controls.Add(_contentPanel, 0, 1);
        mainLayout.Controls.Add(BuildTimerStrip(), 0, 2);

        return mainLayout;
    }

    private Control BuildHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(24, 8, 24, 8),
            BackColor = ThemePalette.PanelBackground
        };

        header.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 65F));

        header.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 35F));

        Panel titlePanel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemePalette.PanelBackground
        };

        _pageTitleLabel.AutoSize = true;
        _pageTitleLabel.Text = "Dashboard";
        _pageTitleLabel.Location = new Point(0, 4);
        _pageTitleLabel.ForeColor = ThemePalette.PrimaryText;
        _pageTitleLabel.Font = new Font(
            "Segoe UI",
            18F,
            FontStyle.Bold);

        _breadcrumbLabel.AutoSize = true;
        _breadcrumbLabel.Text = "Home / Dashboard";
        _breadcrumbLabel.Location = new Point(1, 40);
        _breadcrumbLabel.ForeColor =
            ThemePalette.SecondaryText;
        _breadcrumbLabel.Font = new Font("Segoe UI", 9F);

        titlePanel.Controls.Add(_pageTitleLabel);
        titlePanel.Controls.Add(_breadcrumbLabel);

        Panel statusPanel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemePalette.PanelBackground
        };

        _databaseStatusLabel.Dock = DockStyle.Top;
        _databaseStatusLabel.Height = 26;
        _databaseStatusLabel.Text = "Database: checking";
        _databaseStatusLabel.TextAlign =
            ContentAlignment.MiddleRight;
        _databaseStatusLabel.ForeColor =
            ThemePalette.Warning;
        _databaseStatusLabel.Font = new Font(
            "Segoe UI",
            9F,
            FontStyle.Bold);

        Label backupStatusLabel = new()
        {
            Dock = DockStyle.Top,
            Height = 24,
            Text = "Backup: not configured",
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = ThemePalette.MutedText,
            Font = new Font("Segoe UI", 9F)
        };

        statusPanel.Controls.Add(backupStatusLabel);
        statusPanel.Controls.Add(_databaseStatusLabel);

        header.Controls.Add(titlePanel, 0, 0);
        header.Controls.Add(statusPanel, 1, 0);

        return header;
    }

    private static Control BuildTimerStrip()
    {
        Panel timerStrip = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemePalette.PanelBackground,
            Padding = new Padding(24, 0, 24, 0)
        };

        Label timerLabel = new()
        {
            Dock = DockStyle.Fill,
            Text = "Timer: no active timer — available from Phase 5",
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = ThemePalette.SecondaryText,
            Font = new Font("Segoe UI", 9.5F)
        };

        timerStrip.Controls.Add(timerLabel);

        return timerStrip;
    }

    private void AddNavigationButton(
        FlowLayoutPanel menu,
        string pageTitle,
        string breadcrumb,
        Func<UserControl> pageFactory)
    {
        DarkButton button = new()
        {
            Text = pageTitle
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
    }

    private static void AddSectionHeading(
        FlowLayoutPanel menu,
        string text)
    {
        Label heading = new()
        {
            Width = 188,
            Height = 32,
            Margin = new Padding(8, 14, 0, 0),
            Text = text,
            TextAlign = ContentAlignment.BottomLeft,
            ForeColor = ThemePalette.MutedText,
            Font = new Font(
                "Segoe UI",
                8F,
                FontStyle.Bold)
        };

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

        _contentPanel.Controls.Add(page);

        _pageTitleLabel.Text = pageTitle;
        _breadcrumbLabel.Text = breadcrumb;
    }

    private async void MainShellForm_Shown(
        object? sender,
        EventArgs eventArgs)
    {
        ShowPage(
            new DashboardPage(),
            "Dashboard",
            "Home / Dashboard");

        _databaseStatusLabel.Text = "Database: checking";
        _databaseStatusLabel.ForeColor =
            ThemePalette.Warning;

        try
        {
            using CancellationTokenSource timeout =
                new(TimeSpan.FromSeconds(5));

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