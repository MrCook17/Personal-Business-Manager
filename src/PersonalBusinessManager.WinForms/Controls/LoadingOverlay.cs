using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DefaultProperty(nameof(MessageText))]
[DesignerCategory("Code")]
public sealed class LoadingOverlay : UserControl, IThemeAwareControl
{
    private readonly Label _messageLabel = new();
    private readonly MarqueeIndicator _indicator = new();
    private readonly DarkButton _cancelButton = new();
    private readonly System.Windows.Forms.Timer _animationTimer;
    private bool _isActive;
    private bool _canCancel;
    private Control? _visibilityParent;

    public LoadingOverlay()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);

        AutoScaleMode = AutoScaleMode.Dpi;
        Dock = DockStyle.Fill;
        TabStop = false;
        AccessibleRole = AccessibleRole.Grouping;
        AccessibleName = "Loading";
        Visible = false;

        var centrePanel = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = ThemePalette.RaisedPanel,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(UiSpacing.Space24),
            Margin = Padding.Empty,
        };
        centrePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        centrePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        centrePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _messageLabel.AutoSize = true;
        _messageLabel.Text = "Loading…";
        _messageLabel.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space16);

        _indicator.Width = UiDimensions.SummaryCardWidth;
        _indicator.Height = UiSpacing.Space8;
        _indicator.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space16);

        _cancelButton.Text = "Cancel";
        _cancelButton.Variant = ButtonVariant.Ghost;
        _cancelButton.Visible = false;
        _cancelButton.Anchor = AnchorStyles.Right;
        _cancelButton.Margin = Padding.Empty;
        _cancelButton.Click += (_, _) =>
            CancelRequested?.Invoke(this, EventArgs.Empty);

        centrePanel.Controls.Add(_messageLabel, 0, 0);
        centrePanel.Controls.Add(_indicator, 0, 1);
        centrePanel.Controls.Add(_cancelButton, 0, 2);
        Controls.Add(centrePanel);

        centrePanel.Location = new Point(
            Math.Max(0, (ClientSize.Width - centrePanel.Width) / 2),
            Math.Max(0, (ClientSize.Height - centrePanel.Height) / 2));
        SizeChanged += (_, _) => CentreContent(centrePanel);

        _animationTimer = new System.Windows.Forms.Timer
        {
            Interval = 80,
        };
        _animationTimer.Tick += (_, _) => _indicator.Advance();

        ApplyTheme();
    }

    public event EventHandler? CancelRequested;

    [DefaultValue("Loading…")]
    public string MessageText
    {
        get => _messageLabel.Text;
        set
        {
            _messageLabel.Text = string.IsNullOrWhiteSpace(value)
                ? "Loading…"
                : value;
            AccessibleDescription = _messageLabel.Text;
        }
    }

    [DefaultValue(false)]
    public bool CanCancel
    {
        get => _canCancel;
        set
        {
            _canCancel = value;
            _cancelButton.Visible = value;
        }
    }

    [DefaultValue(false)]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
            {
                return;
            }

            _isActive = value;
            Visible = value;

            if (value && !IsInDesignMode())
            {
                BringToFront();
                _animationTimer.Start();
            }
            else
            {
                _animationTimer.Stop();
            }
        }
    }

    public void ApplyTheme()
    {
        BackColor = ThemePalette.OverlayBackground;
        ForeColor = ThemePalette.PrimaryText;
        Font = UiFonts.Body;
        ControlStyler.StyleLabel(
            _messageLabel,
            ThemeTextRole.BodyStrong);
        _indicator.ApplyTheme();
        _cancelButton.ApplyTheme();
        Invalidate(true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_visibilityParent is not null)
            {
                _visibilityParent.VisibleChanged -=
                    VisibilityParent_VisibleChanged;
            }

            _animationTimer.Stop();
            _animationTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (_isActive && Visible)
        {
            BringToFront();
            _animationTimer.Start();
        }
    }

    protected override void OnParentChanged(EventArgs e)
    {
        if (_visibilityParent is not null)
        {
            _visibilityParent.VisibleChanged -=
                VisibilityParent_VisibleChanged;
        }

        base.OnParentChanged(e);
        _visibilityParent = Parent;

        if (_visibilityParent is not null)
        {
            _visibilityParent.VisibleChanged +=
                VisibilityParent_VisibleChanged;
        }

        RefreshActiveLayer();
    }

    private void VisibilityParent_VisibleChanged(
        object? sender,
        EventArgs e)
    {
        RefreshActiveLayer();
    }

    private void RefreshActiveLayer()
    {
        if (!_isActive || Parent is null || !Parent.Visible)
        {
            return;
        }

        Visible = true;
        BringToFront();

        if (!IsInDesignMode())
        {
            _animationTimer.Start();
        }
    }

    private static void CentreContent(Control content)
    {
        if (content.Parent is null)
        {
            return;
        }

        content.Location = new Point(
            Math.Max(0, (content.Parent.ClientSize.Width - content.Width) / 2),
            Math.Max(0, (content.Parent.ClientSize.Height - content.Height) / 2));
    }

    private bool IsInDesignMode()
    {
        return DesignMode
            || LicenseManager.UsageMode
                == LicenseUsageMode.Designtime;
    }

    private sealed class MarqueeIndicator : Control
    {
        private int _offset;

        public MarqueeIndicator()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.UserPaint,
                true);
            TabStop = false;
        }

        public void ApplyTheme()
        {
            BackColor = ThemePalette.InputBackground;
            ForeColor = ThemePalette.Accent;
            Invalidate();
        }

        public void Advance()
        {
            _offset = (_offset + UiSpacing.Space8)
                % Math.Max(1, ClientSize.Width);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(BackColor);

            int segmentWidth = Math.Max(
                UiDimensions.LargeControlHeight,
                ClientSize.Width / 4);
            int x = _offset - segmentWidth;
            using var brush = new SolidBrush(ForeColor);
            e.Graphics.FillRectangle(
                brush,
                x,
                0,
                segmentWidth,
                ClientSize.Height);

            if (x + segmentWidth < ClientSize.Width)
            {
                return;
            }

            e.Graphics.FillRectangle(
                brush,
                x - ClientSize.Width,
                0,
                segmentWidth,
                ClientSize.Height);
        }
    }
}
