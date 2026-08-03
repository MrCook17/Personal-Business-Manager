using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DefaultEvent(nameof(SelectedIndexChanged))]
[DesignerCategory("Code")]
public sealed class DarkTabControl : TabControl, IThemeAwareControl
{
    private int _hoveredIndex = -1;

    public DarkTabControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint,
            true);
        AccessibleRole = AccessibleRole.PageTabList;
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        ControlStyler.StyleTabControl(this);
        Invalidate();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        SetHoveredIndex(HitTest(e.Location));
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        SetHoveredIndex(-1);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.ResetClip();
        e.Graphics.Clear(ThemePalette.PanelBackground);

        for (int index = 0; index < TabCount; index++)
        {
            DrawTab(e.Graphics, index);
        }

        Rectangle display = DisplayRectangle;
        using var borderPen = new Pen(
            ThemePalette.BorderSubtle,
            DpiScaler.Scale(
                UiDimensions.StandardBorderWidth,
                DeviceDpi));
        e.Graphics.DrawRectangle(
            borderPen,
            Rectangle.Inflate(display, 0, 0));
    }

    private void DrawTab(Graphics graphics, int index)
    {
        Rectangle bounds = GetVisualTabBounds(index);
        bool selected = index == SelectedIndex;
        bool hovered = index == _hoveredIndex;
        Color background = selected
            ? ThemePalette.AccentSoft
            : hovered
                ? ThemePalette.InputHoverBackground
                : ThemePalette.PanelBackground;
        Color foreground = selected || hovered
            ? ThemePalette.PrimaryText
            : ThemePalette.SecondaryText;

        using var brush = new SolidBrush(background);
        graphics.FillRectangle(brush, bounds);
        TextRenderer.DrawText(
            graphics,
            TabPages[index].Text,
            UiFonts.Button,
            bounds,
            foreground,
            TextFormatFlags.HorizontalCenter
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPadding);

        if (selected)
        {
            int indicatorHeight = DpiScaler.Scale(
                UiDimensions.FocusBorderWidth,
                DeviceDpi);
            using var indicatorBrush = new SolidBrush(
                ThemePalette.SelectionIndicator);
            graphics.FillRectangle(
                indicatorBrush,
                bounds.Left,
                bounds.Bottom - indicatorHeight,
                bounds.Width,
                indicatorHeight);
        }

        if (Focused && selected && ShowFocusCues)
        {
            ControlPaint.DrawFocusRectangle(
                graphics,
                Rectangle.Inflate(
                    bounds,
                    -UiSpacing.Space4,
                    -UiSpacing.Space4),
                ThemePalette.FocusBorder,
                background);
        }
    }

    private int HitTest(Point point)
    {
        for (int index = 0; index < TabCount; index++)
        {
            if (GetVisualTabBounds(index).Contains(point))
            {
                return index;
            }
        }

        return -1;
    }

    private static Rectangle GetVisualTabBounds(int index)
    {
        return new Rectangle(
            index * UiDimensions.TabHeaderWidth,
            0,
            UiDimensions.TabHeaderWidth,
            UiDimensions.TabHeaderHeight);
    }

    private void SetHoveredIndex(int index)
    {
        if (_hoveredIndex == index)
        {
            return;
        }

        _hoveredIndex = index;
        Invalidate();
    }
}
