using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public enum ContentStateKind
{
    Empty,
    Error,
    Information,
}

[DefaultProperty(nameof(HeadingText))]
[DesignerCategory("Code")]
public sealed class EmptyStatePanel : UserControl, IThemeAwareControl
{
    private readonly Label _stateLabel = new();
    private readonly Label _headingLabel = new();
    private readonly Label _descriptionLabel = new();
    private readonly Label _referenceLabel = new();
    private readonly FlowLayoutPanel _actionPanel = new();
    private readonly DarkButton _primaryButton = new();
    private readonly DarkButton _secondaryButton = new();
    private ContentStateKind _stateKind;

    public EmptyStatePanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint,
            true);

        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = false;
        MinimumSize = new Size(0, UiDimensions.EmptyStateMinimumHeight);
        Height = UiDimensions.EmptyStateMinimumHeight;
        Padding = new Padding(UiSpacing.Space24);
        TabStop = false;
        AccessibleRole = AccessibleRole.Grouping;

        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 5,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _stateLabel.AutoSize = true;
        _stateLabel.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space8);

        _headingLabel.AutoSize = true;
        _headingLabel.Text = "Nothing to show";
        _headingLabel.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space8);

        _descriptionLabel.AutoSize = true;
        _descriptionLabel.MaximumSize = new Size(
            UiDimensions.SummaryCardWidth * 3,
            0);
        _descriptionLabel.Text =
            "There are no records for the current view.";
        _descriptionLabel.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space16);

        _referenceLabel.AutoSize = true;
        _referenceLabel.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space16);
        _referenceLabel.Visible = false;

        _primaryButton.Text = "Add record";
        _primaryButton.Variant = ButtonVariant.Primary;
        _primaryButton.Click += (_, _) =>
            PrimaryActionClicked?.Invoke(this, EventArgs.Empty);

        _secondaryButton.Text = "Clear filters";
        _secondaryButton.Variant = ButtonVariant.Ghost;
        _secondaryButton.Click += (_, _) =>
            SecondaryActionClicked?.Invoke(this, EventArgs.Empty);

        _actionPanel.AutoSize = true;
        _actionPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _actionPanel.FlowDirection = FlowDirection.LeftToRight;
        _actionPanel.WrapContents = true;
        _actionPanel.Margin = Padding.Empty;
        _actionPanel.Padding = Padding.Empty;
        _primaryButton.Margin = new Padding(
            0,
            0,
            UiSpacing.Space8,
            0);
        _secondaryButton.Margin = Padding.Empty;
        _actionPanel.Controls.Add(_primaryButton);
        _actionPanel.Controls.Add(_secondaryButton);

        layout.Controls.Add(_stateLabel, 0, 0);
        layout.Controls.Add(_headingLabel, 0, 1);
        layout.Controls.Add(_descriptionLabel, 0, 2);
        layout.Controls.Add(_referenceLabel, 0, 3);
        layout.Controls.Add(_actionPanel, 0, 4);
        Controls.Add(layout);

        ApplyTheme();
        UpdateStateLabel();
    }

    public event EventHandler? PrimaryActionClicked;

    public event EventHandler? SecondaryActionClicked;

    [DefaultValue(ContentStateKind.Empty)]
    public ContentStateKind StateKind
    {
        get => _stateKind;
        set
        {
            if (_stateKind == value)
            {
                return;
            }

            _stateKind = value;
            UpdateStateLabel();
            ApplyTheme();
        }
    }

    [DefaultValue("Nothing to show")]
    public string HeadingText
    {
        get => _headingLabel.Text;
        set
        {
            _headingLabel.Text = value ?? string.Empty;
            AccessibleName = _headingLabel.Text;
        }
    }

    [DefaultValue("There are no records for the current view.")]
    public string DescriptionText
    {
        get => _descriptionLabel.Text;
        set => _descriptionLabel.Text = value ?? string.Empty;
    }

    [DefaultValue("")]
    public string TechnicalReference
    {
        get => _referenceLabel.Text;
        set
        {
            _referenceLabel.Text = value ?? string.Empty;
            _referenceLabel.Visible =
                !string.IsNullOrWhiteSpace(_referenceLabel.Text);
        }
    }

    [DefaultValue("Add record")]
    public string PrimaryActionText
    {
        get => _primaryButton.Text;
        set
        {
            _primaryButton.Text = value ?? string.Empty;
            _primaryButton.Visible =
                !string.IsNullOrWhiteSpace(_primaryButton.Text);
        }
    }

    [DefaultValue("Clear filters")]
    public string SecondaryActionText
    {
        get => _secondaryButton.Text;
        set
        {
            _secondaryButton.Text = value ?? string.Empty;
            _secondaryButton.Visible =
                !string.IsNullOrWhiteSpace(_secondaryButton.Text);
        }
    }

    public void ApplyTheme()
    {
        SemanticColors semantic = SemanticTheme.GetColors(
            StateKind switch
            {
                ContentStateKind.Error => SemanticRole.Danger,
                ContentStateKind.Information => SemanticRole.Information,
                _ => SemanticRole.Neutral,
            });

        BackColor = StateKind == ContentStateKind.Error
            ? semantic.Background
            : ThemePalette.PanelBackground;
        ForeColor = ThemePalette.PrimaryText;
        Font = UiFonts.Body;

        ControlStyler.StyleLabel(
            _stateLabel,
            ThemeTextRole.Small,
            semantic.Text);
        ControlStyler.StyleLabel(
            _headingLabel,
            ThemeTextRole.SectionHeading);
        ControlStyler.StyleLabel(
            _descriptionLabel,
            ThemeTextRole.Body,
            ThemePalette.SecondaryText);
        ControlStyler.StyleLabel(
            _referenceLabel,
            ThemeTextRole.MonospaceSmall,
            semantic.Text);
        ControlStyler.StylePanel(_actionPanel, ThemeSurface.Panel);
        _actionPanel.BackColor = BackColor;
        _primaryButton.ApplyTheme();
        _secondaryButton.ApplyTheme();

        foreach (Control child in Controls)
        {
            child.BackColor = BackColor;
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        SemanticColors semantic = SemanticTheme.GetColors(
            StateKind == ContentStateKind.Error
                ? SemanticRole.Danger
                : SemanticRole.Neutral);
        int borderWidth = DpiScaler.Scale(
            UiDimensions.StandardBorderWidth,
            DeviceDpi);
        Rectangle bounds = Rectangle.Inflate(
            ClientRectangle,
            -Math.Max(1, borderWidth / 2),
            -Math.Max(1, borderWidth / 2));
        using var pen = new Pen(semantic.Border, borderWidth);
        e.Graphics.DrawRectangle(pen, bounds);
    }

    private void UpdateStateLabel()
    {
        _stateLabel.Text = StateKind switch
        {
            ContentStateKind.Empty => "EMPTY",
            ContentStateKind.Error => "ERROR",
            ContentStateKind.Information => "INFORMATION",
            _ => throw new ArgumentOutOfRangeException(
                nameof(StateKind)),
        };
    }
}
