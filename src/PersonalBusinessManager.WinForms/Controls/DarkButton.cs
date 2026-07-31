using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public sealed class DarkButton : Button, IThemeAwareControl
{
    private bool _isSelected;
    private bool _isHovered;
    private bool _isPressed;

    public DarkButton()
    {
        AutoSize = false;
        Height = UiDimensions.SidebarNavigationHeight;
        Width = UiDimensions.ExpandedSidebarWidth
            - (UiSpacing.Space16 * 2);
        MinimumSize = new Size(
            0,
            UiDimensions.SidebarNavigationHeight);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Font = UiFonts.Button;
        TextAlign = ContentAlignment.MiddleLeft;
        Padding = new Padding(
            UiSpacing.Space16,
            0,
            UiSpacing.Space8,
            0);
        TabStop = true;
        UseVisualStyleBackColor = false;

        ApplyTheme();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            ApplyTheme();
            Invalidate();
        }
    }

    public void ApplyTheme()
    {
        Font = UiFonts.Button;
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;

        if (!Enabled)
        {
            BackColor = ThemePalette.InputDisabledBackground;
            ForeColor = ThemePalette.DisabledText;
            return;
        }

        if (_isPressed)
        {
            BackColor = IsSelected
                ? ThemePalette.AccentPressed
                : ThemePalette.InputBackground;
            ForeColor = ThemePalette.PrimaryText;
            return;
        }

        if (_isHovered)
        {
            BackColor = ThemePalette.InputHoverBackground;
            ForeColor = ThemePalette.PrimaryText;
            return;
        }

        BackColor = IsSelected
            ? ThemePalette.AccentSoft
            : ThemePalette.SidebarBackground;
        ForeColor = IsSelected
            ? ThemePalette.PrimaryText
            : ThemePalette.SecondaryText;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
        ApplyTheme();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        _isPressed = false;
        ApplyTheme();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        _isPressed = mevent.Button == MouseButtons.Left;
        ApplyTheme();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _isPressed = false;
        ApplyTheme();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        TabStop = Enabled;
        ApplyTheme();
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        base.OnPaint(pevent);

        if (IsSelected)
        {
            int indicatorWidth = DpiScaler.Scale(
                UiDimensions.SelectionIndicatorWidth,
                DeviceDpi);
            using var indicatorBrush = new SolidBrush(
                ThemePalette.SelectionIndicator);
            pevent.Graphics.FillRectangle(
                indicatorBrush,
                ClientRectangle.Left,
                ClientRectangle.Top,
                indicatorWidth,
                ClientRectangle.Height);
        }

        if (Focused && ShowFocusCues)
        {
            int focusWidth = DpiScaler.Scale(
                UiDimensions.FocusBorderWidth,
                DeviceDpi);
            Rectangle focusBounds = Rectangle.Inflate(
                ClientRectangle,
                -focusWidth,
                -focusWidth);
            using var focusPen = new Pen(
                ThemePalette.FocusBorder,
                focusWidth);
            pevent.Graphics.DrawRectangle(
                focusPen,
                focusBounds);
        }
    }
}
