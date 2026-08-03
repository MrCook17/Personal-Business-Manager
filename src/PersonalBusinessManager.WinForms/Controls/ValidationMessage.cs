using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public enum ValidationMessageKind
{
    Field,
    Summary,
}

[DefaultProperty(nameof(Text))]
[DesignerCategory("Code")]
public sealed class ValidationMessage : UserControl, IThemeAwareControl
{
    private readonly Label _iconLabel = new();
    private readonly Label _messageLabel = new();
    private ValidationMessageKind _messageKind;
    private Control? _targetControl;

    public ValidationMessage()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint,
            true);

        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        TabStop = false;
        AccessibleRole = AccessibleRole.Alert;

        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        _iconLabel.AutoSize = true;
        _iconLabel.Text = "!";
        _iconLabel.Margin = new Padding(
            0,
            0,
            UiSpacing.Space8,
            0);

        _messageLabel.AutoSize = true;
        _messageLabel.MaximumSize = new Size(
            UiDimensions.SummaryCardWidth * 3,
            0);
        _messageLabel.Text = "Enter a valid value.";
        _messageLabel.Margin = Padding.Empty;

        layout.Controls.Add(_iconLabel, 0, 0);
        layout.Controls.Add(_messageLabel, 1, 0);
        Controls.Add(layout);

        Click += (_, _) => FocusTarget();
        _iconLabel.Click += (_, _) => FocusTarget();
        _messageLabel.Click += (_, _) => FocusTarget();

        ApplyTheme();
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Visible)]
    [AllowNull]
    public override string Text
    {
        get => _messageLabel.Text;
        set
        {
            _messageLabel.Text = value ?? string.Empty;
            AccessibleName = _messageLabel.Text;
        }
    }

    [DefaultValue(ValidationMessageKind.Field)]
    public ValidationMessageKind MessageKind
    {
        get => _messageKind;
        set
        {
            if (_messageKind == value)
            {
                return;
            }

            _messageKind = value;
            ApplyTheme();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public Control? TargetControl
    {
        get => _targetControl;
        set
        {
            _targetControl = value;
            Cursor = value is null
                ? Cursors.Default
                : Cursors.Hand;
        }
    }

    public void ApplyTheme()
    {
        Font = UiFonts.Small;
        ForeColor = ThemePalette.DangerText;
        BackColor = MessageKind == ValidationMessageKind.Summary
            ? ThemePalette.DangerSoft
            : Parent?.BackColor ?? ThemePalette.ApplicationBackground;
        Padding = MessageKind == ValidationMessageKind.Summary
            ? new Padding(UiSpacing.Space16)
            : new Padding(0, UiSpacing.Space4, 0, 0);
        MinimumSize = MessageKind == ValidationMessageKind.Summary
            ? new Size(0, UiDimensions.ValidationSummaryMinimumHeight)
            : Size.Empty;

        ControlStyler.StyleLabel(
            _iconLabel,
            ThemeTextRole.Small,
            ThemePalette.DangerText);
        ControlStyler.StyleLabel(
            _messageLabel,
            ThemeTextRole.Small,
            MessageKind == ValidationMessageKind.Summary
                ? ThemePalette.PrimaryText
                : ThemePalette.DangerText);

        foreach (Control child in Controls)
        {
            child.BackColor = BackColor;
        }

        Invalidate();
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        int availableWidth = proposedSize.Width > 0
            ? proposedSize.Width
            : UiDimensions.SummaryCardWidth * 3;
        int textWidth = Math.Max(
            UiDimensions.SummaryCardWidth,
            availableWidth - UiDimensions.StandardIconSize - UiSpacing.Space8);
        Size messageSize = TextRenderer.MeasureText(
            Text,
            UiFonts.Small,
            new Size(textWidth, 0),
            TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
        int horizontalPadding = MessageKind == ValidationMessageKind.Summary
            ? UiSpacing.Space16 * 2
            : 0;
        int verticalPadding = MessageKind == ValidationMessageKind.Summary
            ? UiSpacing.Space16 * 2
            : UiSpacing.Space4;
        int width = Math.Min(
            availableWidth,
            messageSize.Width
                + UiDimensions.StandardIconSize
                + UiSpacing.Space8
                + horizontalPadding);
        int height = messageSize.Height + verticalPadding;

        if (MessageKind == ValidationMessageKind.Summary)
        {
            height = Math.Max(
                height,
                UiDimensions.ValidationSummaryMinimumHeight);
        }

        return new Size(width, height);
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);

        if (MessageKind == ValidationMessageKind.Field)
        {
            ApplyTheme();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (MessageKind != ValidationMessageKind.Summary)
        {
            return;
        }

        int borderWidth = DpiScaler.Scale(
            UiDimensions.StandardBorderWidth,
            DeviceDpi);
        Rectangle bounds = Rectangle.Inflate(
            ClientRectangle,
            -Math.Max(1, borderWidth / 2),
            -Math.Max(1, borderWidth / 2));
        using var pen = new Pen(
            ThemePalette.DangerBorder,
            borderWidth);
        e.Graphics.DrawRectangle(pen, bounds);
    }

    private void FocusTarget()
    {
        if (TargetControl is null || !TargetControl.CanFocus)
        {
            return;
        }

        _ = TargetControl.Focus();
    }
}
