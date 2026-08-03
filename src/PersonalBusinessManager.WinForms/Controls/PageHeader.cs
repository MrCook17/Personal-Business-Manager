using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DefaultProperty(nameof(TitleText))]
[DesignerCategory("Code")]
public sealed class PageHeader : UserControl, IThemeAwareControl
{
    private readonly Label _breadcrumbLabel = new();
    private readonly Label _titleLabel = new();
    private readonly Label _subtitleLabel = new();
    private readonly FlowLayoutPanel _actionPanel = new();
    private readonly TableLayoutPanel _rootLayout = new();
    private readonly TableLayoutPanel _textLayout = new();

    public PageHeader()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Dock = DockStyle.Top;
        MinimumSize = new Size(0, UiDimensions.HeaderHeight);
        Margin = new Padding(0, 0, 0, UiSpacing.Space24);
        TabStop = false;

        _rootLayout.AutoSize = true;
        _rootLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _rootLayout.Dock = DockStyle.Top;
        _rootLayout.ColumnCount = 2;
        _rootLayout.RowCount = 1;
        _rootLayout.Margin = Padding.Empty;
        _rootLayout.Padding = Padding.Empty;
        _rootLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.AutoSize));

        _textLayout.AutoSize = true;
        _textLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _textLayout.Dock = DockStyle.Top;
        _textLayout.ColumnCount = 1;
        _textLayout.RowCount = 3;
        _textLayout.Margin = Padding.Empty;
        _textLayout.Padding = Padding.Empty;
        _textLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _textLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _textLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _breadcrumbLabel.AutoSize = true;
        _breadcrumbLabel.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space8);

        _titleLabel.AutoSize = true;
        _titleLabel.Margin = Padding.Empty;
        _titleLabel.Text = "Page title";
        _titleLabel.AccessibleRole = AccessibleRole.StaticText;

        _subtitleLabel.AutoSize = true;
        _subtitleLabel.Margin = new Padding(
            0,
            UiSpacing.Space4,
            0,
            0);

        _textLayout.Controls.Add(_breadcrumbLabel, 0, 0);
        _textLayout.Controls.Add(_titleLabel, 0, 1);
        _textLayout.Controls.Add(_subtitleLabel, 0, 2);

        _actionPanel.AutoSize = true;
        _actionPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _actionPanel.FlowDirection = FlowDirection.RightToLeft;
        _actionPanel.WrapContents = true;
        _actionPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _actionPanel.Margin = Padding.Empty;
        _actionPanel.Padding = Padding.Empty;

        _rootLayout.Controls.Add(_textLayout, 0, 0);
        _rootLayout.Controls.Add(_actionPanel, 1, 0);
        Controls.Add(_rootLayout);

        ApplyTheme();
        UpdateOptionalTextVisibility();
    }

    [DefaultValue("Page title")]
    public string TitleText
    {
        get => _titleLabel.Text;
        set
        {
            _titleLabel.Text = string.IsNullOrWhiteSpace(value)
                ? "Page title"
                : value;
            AccessibleName = _titleLabel.Text;
        }
    }

    [DefaultValue("")]
    public string SubtitleText
    {
        get => _subtitleLabel.Text;
        set
        {
            _subtitleLabel.Text = value ?? string.Empty;
            UpdateOptionalTextVisibility();
        }
    }

    [DefaultValue("")]
    public string BreadcrumbText
    {
        get => _breadcrumbLabel.Text;
        set
        {
            _breadcrumbLabel.Text = value ?? string.Empty;
            UpdateOptionalTextVisibility();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<Control> Actions =>
        _actionPanel.Controls.Cast<Control>().ToArray();

    public void AddAction(Control action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action.Margin = new Padding(UiSpacing.Space8, 0, 0, 0);
        _actionPanel.Controls.Add(action);
    }

    public void ClearActions()
    {
        Control[] actions = _actionPanel.Controls
            .Cast<Control>()
            .ToArray();
        _actionPanel.Controls.Clear();

        foreach (Control action in actions)
        {
            action.Dispose();
        }
    }

    public void ApplyTheme()
    {
        ControlStyler.StylePanel(this, ThemeSurface.Application);
        ControlStyler.StylePanel(
            _rootLayout,
            ThemeSurface.Application);
        ControlStyler.StylePanel(
            _textLayout,
            ThemeSurface.Application);
        ControlStyler.StylePanel(
            _actionPanel,
            ThemeSurface.Application);
        ControlStyler.StyleLabel(
            _breadcrumbLabel,
            ThemeTextRole.Small,
            ThemePalette.SecondaryText);
        ControlStyler.StyleLabel(
            _titleLabel,
            ThemeTextRole.PageHeading);
        ControlStyler.StyleLabel(
            _subtitleLabel,
            ThemeTextRole.Body,
            ThemePalette.SecondaryText);

        foreach (Control action in _actionPanel.Controls)
        {
            ThemeManager.ApplyControlTree(action);
        }
    }

    private void UpdateOptionalTextVisibility()
    {
        _breadcrumbLabel.Visible =
            !string.IsNullOrWhiteSpace(_breadcrumbLabel.Text);
        _subtitleLabel.Visible =
            !string.IsNullOrWhiteSpace(_subtitleLabel.Text);
        PerformLayout();
    }
}
