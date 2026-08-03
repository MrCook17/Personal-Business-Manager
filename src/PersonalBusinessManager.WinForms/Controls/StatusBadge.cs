using System.ComponentModel;
using System.Drawing.Drawing2D;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DefaultProperty(nameof(Text))]
[DesignerCategory("Code")]
public sealed class StatusBadge : Control, IThemeAwareControl
{
    private SemanticRole _semanticRole;
    private Image? _icon;

    public StatusBadge()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.SupportsTransparentBackColor
                | ControlStyles.UserPaint,
            true);

        AutoSize = true;
        Font = UiFonts.Small;
        MinimumSize = new Size(0, UiDimensions.StatusBadgeMinimumHeight);
        Text = "Status";
        TabStop = false;
        AccessibleRole = AccessibleRole.StaticText;
        ApplyTheme();
    }

    [DefaultValue(SemanticRole.Neutral)]
    public SemanticRole SemanticRole
    {
        get => _semanticRole;
        set
        {
            if (_semanticRole == value)
            {
                return;
            }

            _semanticRole = value;
            ApplyTheme();
        }
    }

    [DefaultValue(null)]
    public Image? Icon
    {
        get => _icon;
        set
        {
            if (ReferenceEquals(_icon, value))
            {
                return;
            }

            _icon = value;
            ResizeToPreferred();
            Invalidate();
        }
    }

    public void ApplyTheme()
    {
        SemanticColors colors = SemanticTheme.GetColors(SemanticRole);
        Font = UiFonts.Small;
        BackColor = Enabled
            ? colors.Background
            : ThemePalette.InputDisabledBackground;
        ForeColor = Enabled
            ? colors.Text
            : ThemePalette.DisabledText;
        ResizeToPreferred();
        Invalidate();
    }

    public override Size GetPreferredSize(
        Size proposedSize)
    {
        Size textSize = TextRenderer.MeasureText(
            string.IsNullOrWhiteSpace(Text) ? "Status" : Text,
            UiFonts.Small,
            Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        int iconWidth = Icon is null
            ? 0
            : UiDimensions.StandardIconSize + UiSpacing.Space4;
        int width = (UiDimensions.StatusBadgeHorizontalPadding * 2)
            + iconWidth
            + textSize.Width;
        int height = Math.Max(
            UiDimensions.StatusBadgeMinimumHeight,
            textSize.Height + (UiSpacing.Space4 * 2));

        return new Size(width, height);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        AccessibleName = Text;
        ResizeToPreferred();
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        ApplyTheme();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        SemanticColors colors = SemanticTheme.GetColors(SemanticRole);
        Color borderColor = Enabled
            ? colors.Border
            : ThemePalette.BorderSubtle;
        int borderWidth = DpiScaler.Scale(
            UiDimensions.StandardBorderWidth,
            DeviceDpi);
        int radius = DpiScaler.Scale(
            UiDimensions.CornerRadius,
            DeviceDpi);
        Rectangle bounds = Rectangle.Inflate(
            ClientRectangle,
            -Math.Max(1, borderWidth / 2),
            -Math.Max(1, borderWidth / 2));

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = CreateRoundedRectangle(bounds, radius);
        using var backgroundBrush = new SolidBrush(BackColor);
        using var borderPen = new Pen(borderColor, borderWidth);
        e.Graphics.FillPath(backgroundBrush, path);
        e.Graphics.DrawPath(borderPen, path);

        int left = UiDimensions.StatusBadgeHorizontalPadding;

        if (Icon is not null)
        {
            int iconSize = DpiScaler.Scale(
                UiDimensions.StandardIconSize,
                DeviceDpi);
            e.Graphics.DrawImage(
                Icon,
                new Rectangle(
                    left,
                    (ClientSize.Height - iconSize) / 2,
                    iconSize,
                    iconSize));
            left += iconSize + UiSpacing.Space4;
        }

        Rectangle textBounds = Rectangle.FromLTRB(
            left,
            0,
            ClientSize.Width - UiDimensions.StatusBadgeHorizontalPadding,
            ClientSize.Height);
        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            textBounds,
            ForeColor,
            TextFormatFlags.Left
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPadding);
    }

    private static GraphicsPath CreateRoundedRectangle(
        Rectangle bounds,
        int radius)
    {
        var path = new GraphicsPath();
        int diameter = Math.Min(
            radius * 2,
            Math.Min(bounds.Width, bounds.Height));

        if (diameter <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var arc = new Rectangle(
            bounds.Location,
            new Size(diameter, diameter));
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void ResizeToPreferred()
    {
        if (AutoSize)
        {
            Size = GetPreferredSize(Size.Empty);
        }
    }
}
