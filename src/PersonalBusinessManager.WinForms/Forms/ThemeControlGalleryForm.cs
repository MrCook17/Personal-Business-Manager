using System.ComponentModel;
using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Dialogs;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Forms;

[EditorBrowsable(EditorBrowsableState.Never)]
[DesignerCategory("Code")]
public sealed class ThemeControlGalleryForm : Form
{
    private readonly DarkTabControl _galleryTabs = new();

    public ThemeControlGalleryForm()
    {
        Text = "Theme control gallery — development only";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(
            UiDimensions.MinimumWindowWidth,
            UiDimensions.MinimumWindowHeight);
        MinimumSize = new Size(
            UiDimensions.MinimumWindowWidth,
            UiDimensions.MinimumWindowHeight);
        ShowInTaskbar = false;

        ControlStyler.StyleForm(this);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(UiSpacing.Space24),
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        ControlStyler.StylePanel(root, ThemeSurface.Application);

        var header = new PageHeader
        {
            TitleText = "Theme control gallery",
            SubtitleText =
                "Development-only verification of reusable controls and states.",
            BreadcrumbText = "Development / Theme gallery",
        };
        var openDialogButton = new DarkButton
        {
            Text = "Open confirmation",
            Variant = ButtonVariant.Secondary,
        };
        openDialogButton.Click += (_, _) =>
            _ = ConfirmDialog.ShowConfirmation(
                this,
                "Discard changes",
                "Discard unsaved changes?",
                "The entered values will be lost. This action cannot be undone.",
                "Discard changes",
                ConfirmationSeverity.Danger);
        header.AddAction(openDialogButton);

        _galleryTabs.Dock = DockStyle.Fill;
        _galleryTabs.Margin = Padding.Empty;
        _galleryTabs.TabPages.Add(BuildInputsPage());
        _galleryTabs.TabPages.Add(BuildDataPage());
        _galleryTabs.TabPages.Add(BuildStatesPage());

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(_galleryTabs, 0, 1);
        Controls.Add(root);

        ThemeManager.Apply(this);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public int GalleryPageCount => _galleryTabs.TabCount;

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public int SelectedGalleryPage
    {
        get => _galleryTabs.SelectedIndex;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);

            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
                value,
                _galleryTabs.TabCount);

            _galleryTabs.SelectedIndex = value;
        }
    }

    private static TabPage BuildInputsPage()
    {
        var page = CreatePage("Inputs and actions");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = new Padding(UiSpacing.Space16),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        ControlStyler.StylePanel(layout, ThemeSurface.Panel);

        var filterBar = new FilterBar();
        filterBar.AddFilter(new DarkTextBox
        {
            Width = UiDimensions.SummaryCardWidth,
            PlaceholderText = "Search records",
            AccessibleName = "Search records",
        });
        var filterStatus = new DarkComboBox
        {
            Width = UiDimensions.SummaryCardWidth,
            AccessibleName = "Status filter",
        };
        filterStatus.Items.AddRange(["Active", "Archived", "All"]);
        filterStatus.SelectedIndex = 0;
        filterBar.AddFilter(filterStatus);
        filterBar.AddFilter(new DarkDateTimePicker
        {
            Width = UiDimensions.SummaryCardWidth,
            AccessibleName = "From date",
        });
        filterBar.AddFilter(new DarkButton
        {
            Text = "Clear filters",
            Variant = ButtonVariant.Ghost,
        });

        var inputGrid = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            ColumnCount = 4,
            RowCount = 3,
            Margin = new Padding(0, UiSpacing.Space16, 0, 0),
            Padding = Padding.Empty,
        };
        inputGrid.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 25F));
        inputGrid.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 25F));
        inputGrid.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 25F));
        inputGrid.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 25F));
        ControlStyler.StylePanel(inputGrid, ThemeSurface.Panel);

        AddLabeledControl(
            inputGrid,
            0,
            "Normal text",
            new DarkTextBox
            {
                Text = "Editable value",
                AccessibleName = "Normal text input",
            });
        AddLabeledControl(
            inputGrid,
            1,
            "Read-only text",
            new DarkTextBox
            {
                Text = "Selectable value",
                ReadOnly = true,
                AccessibleName = "Read-only text input",
            });
        AddLabeledControl(
            inputGrid,
            2,
            "Disabled text",
            new DarkTextBox
            {
                Text = "Unavailable value",
                Enabled = false,
                AccessibleName = "Disabled text input",
            });
        AddLabeledControl(
            inputGrid,
            3,
            "Invalid text",
            new DarkTextBox
            {
                Text = "Invalid value",
                HasValidationError = true,
                AccessibleName = "Invalid text input",
            });

        var validation = new ValidationMessage
        {
            Text = "Enter a valid reference before continuing.",
            Dock = DockStyle.Top,
            Margin = new Padding(0, UiSpacing.Space4, 0, 0),
        };

        var buttonRow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, UiSpacing.Space16, 0, 0),
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(buttonRow, ThemeSurface.Panel);
        buttonRow.Controls.Add(CreateGalleryButton(
            "Primary",
            ButtonVariant.Primary));
        buttonRow.Controls.Add(CreateGalleryButton(
            "Secondary",
            ButtonVariant.Secondary));
        buttonRow.Controls.Add(CreateGalleryButton(
            "Ghost",
            ButtonVariant.Ghost));
        buttonRow.Controls.Add(CreateGalleryButton(
            "Danger",
            ButtonVariant.Danger));
        buttonRow.Controls.Add(new DarkButton
        {
            Text = "Disabled",
            Enabled = false,
            Margin = new Padding(0, 0, UiSpacing.Space8, 0),
        });

        layout.Controls.Add(filterBar, 0, 0);
        layout.Controls.Add(inputGrid, 0, 1);
        layout.Controls.Add(validation, 0, 2);
        layout.Controls.Add(buttonRow, 0, 3);
        page.Controls.Add(layout);
        return page;
    }

    private static TabPage BuildDataPage()
    {
        var page = CreatePage("Data and status");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = new Padding(UiSpacing.Space16),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        ControlStyler.StylePanel(layout, ThemeSurface.Panel);

        var badges = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            WrapContents = true,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(badges, ThemeSurface.Panel);
        badges.Controls.Add(CreateBadge("Draft", SemanticRole.Neutral));
        badges.Controls.Add(CreateBadge("Active", SemanticRole.Information));
        badges.Controls.Add(CreateBadge("Paid", SemanticRole.Success));
        badges.Controls.Add(CreateBadge("Part paid", SemanticRole.Warning));
        badges.Controls.Add(CreateBadge("Overdue", SemanticRole.Danger));
        badges.Controls.Add(new StatusBadge
        {
            Text = "Unavailable",
            Enabled = false,
            Margin = new Padding(0, 0, UiSpacing.Space8, 0),
        });

        var cards = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            WrapContents = true,
            Margin = new Padding(0, UiSpacing.Space16, 0, 0),
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(cards, ThemeSurface.Panel);
        cards.Controls.Add(new SummaryCard("Customers", "243"));
        cards.Controls.Add(new SummaryCard("Outstanding", "£550.00"));
        cards.Controls.Add(new SummaryCard("Unbilled time", "12h 30m"));

        var grid = new DarkDataGridView
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, UiSpacing.Space16, 0, 0),
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AccessibleName = "Example customer records",
        };
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Company",
            Name = "Company",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 180F,
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Primary contact",
            Name = "Contact",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 130F,
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Status",
            Name = "Status",
            Width = UiDimensions.TabHeaderWidth,
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Outstanding",
            Name = "Outstanding",
            Width = UiDimensions.TabHeaderWidth,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
            },
        });
        grid.Rows.Add("Acme Engineering", "Alex Smith", "Active", "£550.00");
        grid.Rows.Add("Harbour Design", "Priya Shah", "Draft", "£0.00");
        grid.Rows.Add("Long company name demonstrating clipped content", "Not set", "Archived", "£125.50");
        grid.Rows[0].Selected = true;

        layout.Controls.Add(badges, 0, 0);
        layout.Controls.Add(cards, 0, 1);
        layout.Controls.Add(grid, 0, 2);
        page.Controls.Add(layout);
        return page;
    }

    private static TabPage BuildStatesPage()
    {
        var page = CreatePage("States and feedback");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(UiSpacing.Space16),
        };
        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50F));
        layout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 60F));
        layout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 40F));
        ControlStyler.StylePanel(layout, ThemeSurface.Panel);

        var empty = new EmptyStatePanel
        {
            Dock = DockStyle.Fill,
            HeadingText = "No customers yet",
            DescriptionText =
                "Add the first customer to begin recording work.",
            PrimaryActionText = "Add customer",
            SecondaryActionText = string.Empty,
            Margin = new Padding(0, 0, UiSpacing.Space8, UiSpacing.Space8),
        };

        var error = new EmptyStatePanel
        {
            Dock = DockStyle.Fill,
            StateKind = ContentStateKind.Error,
            HeadingText = "Customers could not be loaded",
            DescriptionText =
                "Check the connection and try this operation again.",
            TechnicalReference = "Reference: UI-GALLERY-001",
            PrimaryActionText = "Retry",
            SecondaryActionText = "Back",
            Margin = new Padding(UiSpacing.Space8, 0, 0, UiSpacing.Space8),
        };

        var validation = new ValidationMessage
        {
            Dock = DockStyle.Fill,
            MessageKind = ValidationMessageKind.Summary,
            Text =
                "Review the highlighted fields. Company name and contact email require attention.",
            Margin = new Padding(0, UiSpacing.Space8, UiSpacing.Space8, 0),
        };

        var loadingHost = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(UiSpacing.Space8, UiSpacing.Space8, 0, 0),
        };
        ControlStyler.StylePanel(loadingHost, ThemeSurface.Panel);
        var loading = new LoadingOverlay
        {
            MessageText = "Loading customer records…",
            CanCancel = true,
            AccessibleDescription =
                "Loading overlay blocks its underlying page region.",
        };
        loadingHost.Controls.Add(loading);
        loading.IsActive = true;

        layout.Controls.Add(empty, 0, 0);
        layout.Controls.Add(error, 1, 0);
        layout.Controls.Add(validation, 0, 1);
        layout.Controls.Add(loadingHost, 1, 1);
        page.Controls.Add(layout);
        return page;
    }

    private static TabPage CreatePage(string title)
    {
        return new TabPage(title)
        {
            BackColor = ThemePalette.PanelBackground,
            ForeColor = ThemePalette.PrimaryText,
            Font = UiFonts.Body,
            Padding = Padding.Empty,
        };
    }

    private static void AddLabeledControl(
        TableLayoutPanel layout,
        int column,
        string labelText,
        Control control)
    {
        var host = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, UiSpacing.Space16, 0),
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(host, ThemeSurface.Panel);
        var label = new Label
        {
            AutoSize = true,
            Text = labelText,
            Margin = new Padding(0, 0, 0, UiSpacing.Space4),
        };
        ControlStyler.StyleLabel(label, ThemeTextRole.Label);
        control.Dock = DockStyle.Top;
        control.Margin = Padding.Empty;
        host.Controls.Add(label, 0, 0);
        host.Controls.Add(control, 0, 1);
        layout.Controls.Add(host, column, 0);
    }

    private static DarkButton CreateGalleryButton(
        string text,
        ButtonVariant variant)
    {
        return new DarkButton
        {
            Text = text,
            Variant = variant,
            Margin = new Padding(0, 0, UiSpacing.Space8, 0),
        };
    }

    private static StatusBadge CreateBadge(
        string text,
        SemanticRole role)
    {
        return new StatusBadge
        {
            Text = text,
            SemanticRole = role,
            AccessibleDescription = $"Status: {text}",
            Margin = new Padding(0, 0, UiSpacing.Space8, 0),
        };
    }
}
