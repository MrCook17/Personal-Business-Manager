using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public sealed class SummaryCard : Panel, IThemeAwareControl
{
    private readonly Label _headingLabel;
    private readonly Label _valueLabel;

    public SummaryCard()
        : this("Summary", "0")
    {
    }

    public SummaryCard(string heading, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentNullException.ThrowIfNull(value);

        Width = UiDimensions.SummaryCardWidth;
        Height = UiDimensions.SummaryCardHeight;
        MinimumSize = new Size(
            UiDimensions.SummaryCardWidth,
            UiDimensions.SummaryCardHeight);
        Margin = new Padding(
            0,
            0,
            UiSpacing.Space16,
            UiSpacing.Space16);
        Padding = new Padding(UiSpacing.Space16);
        TabStop = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        layout.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));

        _headingLabel = new Label
        {
            AutoSize = true,
            Text = heading,
            Margin = new Padding(
                0,
                0,
                0,
                UiSpacing.Space8),
        };

        _valueLabel = new Label
        {
            AutoSize = true,
            Text = value,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.Left,
        };

        layout.Controls.Add(_headingLabel, 0, 0);
        layout.Controls.Add(_valueLabel, 0, 1);
        Controls.Add(layout);

        ApplyTheme();
    }

    public void SetValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _valueLabel.Text = value;
    }

    public void ApplyTheme()
    {
        ControlStyler.StylePanel(this, ThemeSurface.Raised);
        ControlStyler.StyleLabel(
            _headingLabel,
            ThemeTextRole.Small,
            ThemePalette.SecondaryText);
        ControlStyler.StyleLabel(
            _valueLabel,
            ThemeTextRole.DashboardValue);

        foreach (Control child in Controls)
        {
            child.BackColor = ThemePalette.RaisedPanel;
            child.ForeColor = ThemePalette.PrimaryText;
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        int borderWidth = DpiScaler.Scale(
            UiDimensions.StandardBorderWidth,
            DeviceDpi);
        Rectangle borderBounds = Rectangle.Inflate(
            ClientRectangle,
            -borderWidth,
            -borderWidth);
        using var borderPen = new Pen(
            ThemePalette.BorderSubtle,
            borderWidth);
        e.Graphics.DrawRectangle(borderPen, borderBounds);
    }
}
