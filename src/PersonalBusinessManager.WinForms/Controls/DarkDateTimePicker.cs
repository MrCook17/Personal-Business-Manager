using System.ComponentModel;
using System.Globalization;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DefaultEvent(nameof(ValueChanged))]
[DesignerCategory("Code")]
public sealed class DarkDateTimePicker : DarkInputBase
{
    private const string ApprovedDateFormat = "dd/MM/yyyy";
    private readonly DateTimePicker _dateTimePicker;

    public DarkDateTimePicker()
        : base(new DateTimePicker())
    {
        _dateTimePicker = (DateTimePicker)Editor;
        _dateTimePicker.Format = DateTimePickerFormat.Custom;
        _dateTimePicker.CustomFormat = ApprovedDateFormat;
        _dateTimePicker.ValueChanged += (_, _) =>
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        };
        SetEditorHostVisible(false);
        _dateTimePicker.TabStop = false;
        TabStop = true;
        AccessibleRole = AccessibleRole.DropList;

        ApplyTheme();
    }

    public event EventHandler? ValueChanged;

    [DefaultValue(typeof(DateTimePickerFormat), "Custom")]
    public DateTimePickerFormat Format
    {
        get => _dateTimePicker.Format;
        set => _dateTimePicker.Format = value;
    }

    [DefaultValue(ApprovedDateFormat)]
    public string CustomFormat
    {
        get => _dateTimePicker.CustomFormat ?? ApprovedDateFormat;
        set => _dateTimePicker.CustomFormat =
            string.IsNullOrWhiteSpace(value)
                ? ApprovedDateFormat
                : value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public DateTime Value
    {
        get => _dateTimePicker.Value;
        set => _dateTimePicker.Value = value;
    }

    [DefaultValue(false)]
    public bool ShowCheckBox
    {
        get => _dateTimePicker.ShowCheckBox;
        set => _dateTimePicker.ShowCheckBox = value;
    }

    [DefaultValue(true)]
    public bool Checked
    {
        get => _dateTimePicker.Checked;
        set => _dateTimePicker.Checked = value;
    }

    [DefaultValue(false)]
    public bool ShowUpDown
    {
        get => _dateTimePicker.ShowUpDown;
        set => _dateTimePicker.ShowUpDown = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public DateTimePicker EditorDateTimePicker => _dateTimePicker;

    public override void ApplyTheme()
    {
        base.ApplyTheme();
        _dateTimePicker.TabStop = false;
        _dateTimePicker.CalendarFont = UiFonts.Body;
        _dateTimePicker.CalendarMonthBackground =
            ThemePalette.RaisedPanel;
        _dateTimePicker.CalendarForeColor =
            ThemePalette.PrimaryText;
        _dateTimePicker.CalendarTitleBackColor =
            ThemePalette.HeaderBackground;
        _dateTimePicker.CalendarTitleForeColor =
            ThemePalette.PrimaryText;
        _dateTimePicker.CalendarTrailingForeColor =
            ThemePalette.MutedText;
    }

    protected override int TrailingEditorReservation =>
        Math.Max(
            0,
            UiDimensions.CompactControlHeight - Padding.Right);

    protected override bool FocusEditorOnEnter => false;

    protected override void LayoutEditor(Rectangle contentBounds)
    {
        ((DateTimePicker)Editor).SetBounds(
            -UiDimensions.StandardControlHeight,
            contentBounds.Top,
            1,
            contentBounds.Height);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _ = Focus();

        if (Enabled
            && e.X >= ClientSize.Width
                - UiDimensions.CompactControlHeight)
        {
            ShowCalendarPopup();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (Enabled
            && (e.KeyCode == Keys.F4
                || (e.Alt && e.KeyCode == Keys.Down)))
        {
            ShowCalendarPopup();
            e.Handled = true;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Rectangle textBounds = Rectangle.FromLTRB(
            Padding.Left,
            0,
            Math.Max(
                Padding.Left,
                ClientSize.Width
                    - UiDimensions.CompactControlHeight),
            ClientSize.Height);
        string displayText = ShowCheckBox && !Checked
            ? string.Empty
            : GetApprovedDisplayText();
        TextRenderer.DrawText(
            e.Graphics,
            displayText,
            UiFonts.Body,
            textBounds,
            Enabled
                ? ThemePalette.PrimaryText
                : ThemePalette.DisabledText,
            TextFormatFlags.Left
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPadding);
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

    public string GetApprovedDisplayText()
    {
        return Value.ToString(
            ApprovedDateFormat,
            CultureInfo.GetCultureInfo("en-GB"));
    }

    private void ShowCalendarPopup()
    {
        var calendar = new MonthCalendar
        {
            CalendarDimensions = new Size(1, 1),
            MaxSelectionCount = 1,
            SelectionStart = Value,
            SelectionEnd = Value,
            BackColor = ThemePalette.RaisedPanel,
            ForeColor = ThemePalette.PrimaryText,
            TitleBackColor = ThemePalette.HeaderBackground,
            TitleForeColor = ThemePalette.PrimaryText,
            TrailingForeColor = ThemePalette.MutedText,
            Font = UiFonts.Body,
        };
        var calendarHost = new ToolStripControlHost(calendar)
        {
            AutoSize = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Size = calendar.Size,
        };
        var dropDown = new ToolStripDropDown
        {
            AutoClose = true,
            BackColor = ThemePalette.RaisedPanel,
            ForeColor = ThemePalette.PrimaryText,
            Margin = Padding.Empty,
            Padding = new Padding(UiSpacing.Space4),
        };
        dropDown.Items.Add(calendarHost);
        calendar.DateSelected += (_, eventArgs) =>
        {
            Value = eventArgs.Start;
            dropDown.Close(ToolStripDropDownCloseReason.ItemClicked);
        };
        dropDown.Closed += (_, _) => dropDown.Dispose();
        dropDown.Show(this, new Point(0, Height));
    }
}
