using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DefaultEvent(nameof(Click))]
[DefaultProperty(nameof(Text))]
[DesignerCategory("Code")]
public sealed class DarkButton : Button, IThemeAwareControl
{
    private bool _isSelected;
    private bool _isHovered;
    private bool _isPressed;
    private bool _isNavigationItem;
    private ButtonVariant _variant = ButtonVariant.Secondary;
    private ControlSize _sizeVariant = ControlSize.Standard;

    public DarkButton()
    {
        AutoSize = false;
        FlatStyle = FlatStyle.Flat;
        Font = UiFonts.Button;
        TextAlign = ContentAlignment.MiddleCenter;
        TabStop = true;
        UseVisualStyleBackColor = false;

        ApplyTheme();
    }

    [DefaultValue(ButtonVariant.Secondary)]
    public ButtonVariant Variant
    {
        get => _variant;
        set
        {
            if (_variant == value)
            {
                return;
            }

            _variant = value;
            ApplyTheme();
        }
    }

    [DefaultValue(ControlSize.Standard)]
    public ControlSize SizeVariant
    {
        get => _sizeVariant;
        set
        {
            if (_sizeVariant == value)
            {
                return;
            }

            _sizeVariant = value;
            ApplyTheme();
        }
    }

    [DefaultValue(false)]
    public bool IsNavigationItem
    {
        get => _isNavigationItem;
        set
        {
            if (_isNavigationItem == value)
            {
                return;
            }

            _isNavigationItem = value;
            ApplyTheme();
        }
    }

    [DefaultValue(false)]
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
        }
    }

    public void ApplyTheme()
    {
        Font = UiFonts.Button;
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        TabStop = Enabled;
        ApplyDimensions();

        (Color background, Color foreground, Color border) =
            GetBaseColors();

        if (!Enabled)
        {
            background = ThemePalette.InputDisabledBackground;
            foreground = ThemePalette.DisabledText;
            border = ThemePalette.BorderSubtle;
        }
        else if (_isPressed)
        {
            background = Variant switch
            {
                ButtonVariant.Primary => ThemePalette.AccentPressed,
                ButtonVariant.Danger => ThemePalette.DangerBorder,
                _ => ThemePalette.InputBackground,
            };
            foreground = Variant is ButtonVariant.Primary
                or ButtonVariant.Danger
                ? ThemePalette.InverseText
                : ThemePalette.PrimaryText;
        }
        else if (_isHovered)
        {
            background = Variant switch
            {
                ButtonVariant.Primary => ThemePalette.AccentHover,
                ButtonVariant.Secondary => ThemePalette.RaisedPanel,
                ButtonVariant.Ghost => ThemePalette.InputHoverBackground,
                ButtonVariant.Danger => ThemePalette.DangerText,
                _ => background,
            };
            foreground = Variant is ButtonVariant.Primary
                or ButtonVariant.Danger
                ? ThemePalette.InverseText
                : ThemePalette.PrimaryText;
            border = Variant == ButtonVariant.Ghost
                ? ThemePalette.BorderStrong
                : border;
        }

        BackColor = background;
        ForeColor = foreground;
        FlatAppearance.BorderColor = Focused
            ? ThemePalette.FocusBorder
            : border;
        FlatAppearance.BorderSize = Focused
            ? DpiScaler.Scale(
                UiDimensions.FocusBorderWidth,
                DeviceDpi)
            : IsNavigationItem || Variant == ButtonVariant.Ghost
                ? 0
                : DpiScaler.Scale(
                    UiDimensions.StandardBorderWidth,
                    DeviceDpi);

        Invalidate();
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
        ApplyTheme();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        ApplyTheme();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        ApplyTheme();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        ApplyTheme();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        base.OnPaint(pevent);

        if (!IsNavigationItem || !IsSelected)
        {
            return;
        }

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

    private void ApplyDimensions()
    {
        if (IsNavigationItem)
        {
            Height = UiDimensions.SidebarNavigationHeight;
            Width = UiDimensions.ExpandedSidebarWidth
                - (UiSpacing.Space16 * 2);
            MinimumSize = new Size(
                0,
                UiDimensions.SidebarNavigationHeight);
            Padding = new Padding(
                UiSpacing.Space16,
                0,
                UiSpacing.Space8,
                0);
            TextAlign = ContentAlignment.MiddleLeft;
            return;
        }

        (int height, int minimumWidth, int horizontalPadding) =
            SizeVariant switch
            {
                ControlSize.Compact =>
                    (UiDimensions.CompactControlHeight,
                        UiDimensions.CompactControlHeight,
                        UiSpacing.Space8),
                ControlSize.Standard =>
                    (UiDimensions.StandardControlHeight,
                        UiDimensions.StandardButtonMinimumWidth,
                        UiSpacing.Space16),
                ControlSize.Large =>
                    (UiDimensions.LargeControlHeight,
                        UiDimensions.LargeButtonMinimumWidth,
                        UiSpacing.Space24),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(SizeVariant)),
            };

        Height = height;
        MinimumSize = new Size(minimumWidth, height);
        Padding = new Padding(horizontalPadding, 0, horizontalPadding, 0);
        TextAlign = ContentAlignment.MiddleCenter;

        Size textSize = TextRenderer.MeasureText(
            Text,
            UiFonts.Button,
            Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        int preferredWidth = textSize.Width
            + (horizontalPadding * 2)
            + (UiDimensions.FocusBorderWidth * 2)
            + UiSpacing.Space24;
        Width = Math.Max(Width, Math.Max(minimumWidth, preferredWidth));
    }

    private (Color Background, Color Foreground, Color Border)
        GetBaseColors()
    {
        if (IsNavigationItem)
        {
            return IsSelected
                ? (ThemePalette.AccentSoft,
                    ThemePalette.PrimaryText,
                    ThemePalette.AccentSoft)
                : (ThemePalette.SidebarBackground,
                    ThemePalette.SecondaryText,
                    ThemePalette.SidebarBackground);
        }

        return Variant switch
        {
            ButtonVariant.Primary =>
                (ThemePalette.Accent,
                    ThemePalette.InverseText,
                    ThemePalette.Accent),
            ButtonVariant.Secondary =>
                (ThemePalette.RaisedPanel,
                    ThemePalette.PrimaryText,
                    ThemePalette.BorderDefault),
            ButtonVariant.Ghost =>
                (Parent?.BackColor ?? ThemePalette.PanelBackground,
                    ThemePalette.SecondaryText,
                    ThemePalette.BorderSubtle),
            ButtonVariant.Danger =>
                (ThemePalette.Danger,
                    ThemePalette.InverseText,
                    ThemePalette.Danger),
            _ => throw new ArgumentOutOfRangeException(
                nameof(Variant)),
        };
    }
}
