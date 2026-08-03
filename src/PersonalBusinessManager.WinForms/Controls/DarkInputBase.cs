using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DesignerCategory("Code")]
public abstract class DarkInputBase : UserControl, IThemeAwareControl
{
    private readonly Panel _editorHost;
    private bool _hovered;
    private bool _hasValidationError;
    private Color _borderColor = ThemePalette.BorderDefault;
    private int _borderWidth = UiDimensions.StandardBorderWidth;

    protected DarkInputBase(Control editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        Editor = editor;
        _editorHost = new Panel
        {
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            TabStop = false,
        };
        _editorHost.Controls.Add(Editor);
        Controls.Add(_editorHost);

        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint,
            true);

        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = false;
        Height = UiDimensions.StandardControlHeight;
        MinimumSize = new Size(0, UiDimensions.StandardControlHeight);
        Padding = new Padding(
            UiDimensions.InputHorizontalPadding,
            0,
            UiDimensions.InputHorizontalPadding,
            0);
        TabStop = false;

        Editor.Font = UiFonts.Body;
        Editor.TabStop = true;
        MouseEnter += (_, _) => SetHovered(true);
        MouseLeave += (_, _) => RefreshHoverFromPointer();
        _editorHost.MouseEnter += (_, _) => SetHovered(true);
        _editorHost.MouseLeave += (_, _) => RefreshHoverFromPointer();
        Editor.MouseEnter += (_, _) => SetHovered(true);
        Editor.MouseLeave += (_, _) => RefreshHoverFromPointer();
        Editor.GotFocus += (_, _) => ApplyTheme();
        Editor.LostFocus += (_, _) => ApplyTheme();
        Editor.EnabledChanged += (_, _) => ApplyTheme();

    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    protected Control Editor { get; }

    [DefaultValue(false)]
    public bool HasValidationError
    {
        get => _hasValidationError;
        set
        {
            if (_hasValidationError == value)
            {
                return;
            }

            _hasValidationError = value;
            ApplyTheme();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public Color CurrentBorderColor => _borderColor;

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public int CurrentBorderWidth => _borderWidth;

    protected virtual bool IsReadOnly => false;

    protected virtual int TrailingEditorReservation => 0;

    protected virtual bool FocusEditorOnEnter => true;

    protected void SetEditorHostVisible(bool visible)
    {
        _editorHost.Visible = visible;
    }

    public virtual void ApplyTheme()
    {
        Font = UiFonts.Body;
        Editor.Font = UiFonts.Body;
        Editor.Enabled = Enabled;

        Color background;
        Color foreground;

        if (!Enabled)
        {
            background = ThemePalette.InputDisabledBackground;
            foreground = ThemePalette.DisabledText;
            _borderColor = ThemePalette.BorderSubtle;
            _borderWidth = UiDimensions.StandardBorderWidth;
            Editor.TabStop = false;
        }
        else if (IsReadOnly)
        {
            background = ThemePalette.PanelBackground;
            foreground = ThemePalette.SecondaryText;
            _borderColor = ThemePalette.BorderDefault;
            _borderWidth = UiDimensions.StandardBorderWidth;
            Editor.TabStop = true;
        }
        else
        {
            background = _hovered || ContainsFocus
                ? ThemePalette.InputHoverBackground
                : ThemePalette.InputBackground;
            foreground = ThemePalette.PrimaryText;
            Editor.TabStop = true;

            if (ContainsFocus)
            {
                _borderColor = ThemePalette.FocusBorder;
                _borderWidth = UiDimensions.FocusBorderWidth;
            }
            else if (HasValidationError)
            {
                _borderColor = ThemePalette.DangerBorder;
                _borderWidth = UiDimensions.FocusBorderWidth;
            }
            else
            {
                _borderColor = _hovered
                    ? ThemePalette.BorderStrong
                    : ThemePalette.BorderDefault;
                _borderWidth = UiDimensions.StandardBorderWidth;
            }
        }

        BackColor = background;
        ForeColor = foreground;
        _editorHost.BackColor = background;
        _editorHost.ForeColor = foreground;
        Editor.BackColor = background;
        Editor.ForeColor = foreground;

        PerformLayout();
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        ApplyTheme();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);

        if (Editor is not null)
        {
            Editor.Font = Font;
        }
    }

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);

        if (Enabled && FocusEditorOnEnter && !Editor.Focused)
        {
            _ = Editor.Focus();
        }

        ApplyTheme();
    }

    protected override void OnLeave(EventArgs e)
    {
        base.OnLeave(e);
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

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);

        if (Editor is null)
        {
            return;
        }

        int verticalBorderReservation = Math.Max(
            Padding.Top,
            UiDimensions.FocusBorderWidth);
        Rectangle contentBounds = Rectangle.FromLTRB(
            Padding.Left,
            verticalBorderReservation,
            Math.Max(
                Padding.Left,
                ClientSize.Width
                    - Padding.Right
                    - TrailingEditorReservation),
            Math.Max(
                verticalBorderReservation,
                ClientSize.Height
                    - Math.Max(
                        Padding.Bottom,
                        UiDimensions.FocusBorderWidth)));

        _editorHost.Bounds = contentBounds;
        LayoutEditor(_editorHost.ClientRectangle);
    }

    protected virtual void LayoutEditor(Rectangle contentBounds)
    {
        Editor.Bounds = contentBounds;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        int borderWidth = DpiScaler.Scale(
            _borderWidth,
            DeviceDpi);
        Rectangle borderBounds = Rectangle.Inflate(
            ClientRectangle,
            -Math.Max(1, borderWidth / 2),
            -Math.Max(1, borderWidth / 2));

        using var pen = new Pen(_borderColor, borderWidth);
        e.Graphics.DrawRectangle(pen, borderBounds);
    }

    private void SetHovered(bool hovered)
    {
        if (_hovered == hovered)
        {
            return;
        }

        _hovered = hovered;
        ApplyTheme();
    }

    private void RefreshHoverFromPointer()
    {
        Point clientPoint = PointToClient(Cursor.Position);
        SetHovered(ClientRectangle.Contains(clientPoint));
    }
}
