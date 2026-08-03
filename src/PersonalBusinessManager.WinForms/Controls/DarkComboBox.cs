using System.Collections;
using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DefaultEvent(nameof(SelectedIndexChanged))]
[DesignerCategory("Code")]
public sealed class DarkComboBox : DarkInputBase
{
    private readonly ComboBox _comboBox;

    public DarkComboBox()
        : base(new ComboBox())
    {
        _comboBox = (ComboBox)Editor;
        _comboBox.Dock = DockStyle.None;
        _comboBox.DrawMode = DrawMode.OwnerDrawFixed;
        _comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _comboBox.FlatStyle = FlatStyle.Flat;
        _comboBox.IntegralHeight = false;
        _comboBox.ItemHeight = UiDimensions.StatusBadgeMinimumHeight;
        _comboBox.DrawItem += ComboBox_DrawItem;
        _comboBox.SelectedIndexChanged += (_, _) =>
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        _comboBox.DropDown += (_, _) =>
            DropDown?.Invoke(this, EventArgs.Empty);
        _comboBox.DropDownClosed += (_, _) =>
            DropDownClosed?.Invoke(this, EventArgs.Empty);

        ApplyTheme();
    }

    public event EventHandler? SelectedIndexChanged;

    public event EventHandler? DropDown;

    public event EventHandler? DropDownClosed;

    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Content)]
    public ComboBox.ObjectCollection Items => _comboBox.Items;

    [DefaultValue(null)]
    [AttributeProvider(typeof(IListSource))]
    public object? DataSource
    {
        get => _comboBox.DataSource;
        set => _comboBox.DataSource = value;
    }

    [DefaultValue("")]
    public string DisplayMember
    {
        get => _comboBox.DisplayMember;
        set => _comboBox.DisplayMember = value ?? string.Empty;
    }

    [DefaultValue("")]
    public string ValueMember
    {
        get => _comboBox.ValueMember;
        set => _comboBox.ValueMember = value ?? string.Empty;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex
    {
        get => _comboBox.SelectedIndex;
        set => _comboBox.SelectedIndex = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public object? SelectedItem
    {
        get => _comboBox.SelectedItem;
        set
        {
            if (value is null)
            {
                _comboBox.SelectedIndex = -1;
            }
            else
            {
                _comboBox.SelectedItem = value;
            }
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public object? SelectedValue
    {
        get => _comboBox.SelectedValue;
        set
        {
            if (value is null)
            {
                _comboBox.SelectedIndex = -1;
            }
            else
            {
                _comboBox.SelectedValue = value;
            }
        }
    }

    [DefaultValue(ComboBoxStyle.DropDownList)]
    public ComboBoxStyle DropDownStyle
    {
        get => _comboBox.DropDownStyle;
        set => _comboBox.DropDownStyle = value;
    }

    [DefaultValue(8)]
    public int MaxDropDownItems
    {
        get => _comboBox.MaxDropDownItems;
        set => _comboBox.MaxDropDownItems = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public ComboBox EditorComboBox => _comboBox;

    public override void ApplyTheme()
    {
        base.ApplyTheme();
        _comboBox.FlatStyle = FlatStyle.Flat;
    }

    protected override int TrailingEditorReservation =>
        Math.Max(
            0,
            UiDimensions.CompactControlHeight - Padding.Right);

    protected override void LayoutEditor(Rectangle contentBounds)
    {
        ((ComboBox)Editor).SetBounds(
            contentBounds.Left,
            contentBounds.Top,
            contentBounds.Width + UiDimensions.CompactControlHeight,
            contentBounds.Height);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (Enabled
            && e.X >= ClientSize.Width
                - UiDimensions.CompactControlHeight)
        {
            _comboBox.DroppedDown = true;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        DropDownGlyphPainter.Draw(
            e.Graphics,
            new Rectangle(
                Math.Max(
                    0,
                    ClientSize.Width
                        - UiDimensions.CompactControlHeight),
                UiDimensions.StandardBorderWidth,
                UiDimensions.CompactControlHeight
                    - UiDimensions.StandardBorderWidth,
                Math.Max(
                    0,
                    ClientSize.Height
                        - (UiDimensions.StandardBorderWidth * 2))),
            Enabled
                ? ThemePalette.SecondaryText
                : ThemePalette.DisabledText,
            DeviceDpi);
    }

    private void ComboBox_DrawItem(
        object? sender,
        DrawItemEventArgs e)
    {
        if (e.Index < 0)
        {
            return;
        }

        bool selected = (e.State & DrawItemState.Selected) != 0;
        Color background = selected
            ? ThemePalette.AccentSoft
            : ThemePalette.RaisedPanel;
        Color foreground = Enabled
            ? ThemePalette.PrimaryText
            : ThemePalette.DisabledText;

        using var backgroundBrush = new SolidBrush(background);
        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
        TextRenderer.DrawText(
            e.Graphics,
            _comboBox.GetItemText(_comboBox.Items[e.Index]),
            UiFonts.Body,
            Rectangle.Inflate(
                e.Bounds,
                -UiSpacing.Space8,
                0),
            foreground,
            TextFormatFlags.Left
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis);

        if ((e.State & DrawItemState.Focus) != 0)
        {
            ControlPaint.DrawFocusRectangle(
                e.Graphics,
                e.Bounds,
                ThemePalette.FocusBorder,
                background);
        }
    }
}
