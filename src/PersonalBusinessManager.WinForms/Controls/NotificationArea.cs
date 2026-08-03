using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public enum ShellNotificationSeverity
{
    Information,
    Success,
    Warning,
    Error,
}

public sealed record ShellNotification(
    string Message,
    ShellNotificationSeverity Severity =
        ShellNotificationSeverity.Information,
    string? ActionText = null,
    Action? Action = null,
    TimeSpan? AutoDismissAfter = null);

[DesignerCategory("Code")]
public sealed class NotificationArea : UserControl, IThemeAwareControl
{
    private readonly FlowLayoutPanel _stack = new();

    public NotificationArea()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Width = UiDimensions.NotificationMaximumWidth;
        MaximumSize = new Size(
            UiDimensions.NotificationMaximumWidth,
            0);
        Margin = Padding.Empty;
        Padding = Padding.Empty;
        TabStop = false;
        AccessibleRole = AccessibleRole.Grouping;
        AccessibleName = "Notifications";

        _stack.AutoSize = true;
        _stack.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _stack.FlowDirection = FlowDirection.TopDown;
        _stack.WrapContents = false;
        _stack.Margin = Padding.Empty;
        _stack.Padding = Padding.Empty;
        _stack.Width = UiDimensions.NotificationMaximumWidth;
        Controls.Add(_stack);

        ApplyTheme();
    }

    public event EventHandler? NotificationsChanged;

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public int ActiveNotificationCount => _stack.Controls.Count;

    public Guid ShowNotification(ShellNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentException.ThrowIfNullOrWhiteSpace(notification.Message);

        var toast = new NotificationToast(
            notification,
            notificationId =>
                _ = DismissNotification(notificationId));
        _stack.Controls.Add(toast);
        _stack.Controls.SetChildIndex(toast, 0);
        Visible = true;
        BringToFront();
        NotificationsChanged?.Invoke(this, EventArgs.Empty);
        return toast.NotificationId;
    }

    public bool DismissNotification(Guid notificationId)
    {
        NotificationToast? toast = _stack.Controls
            .OfType<NotificationToast>()
            .FirstOrDefault(candidate =>
                candidate.NotificationId == notificationId);

        if (toast is null)
        {
            return false;
        }

        _stack.Controls.Remove(toast);
        toast.Dispose();
        Visible = _stack.Controls.Count > 0;
        NotificationsChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void DismissAll()
    {
        NotificationToast[] toasts = _stack.Controls
            .OfType<NotificationToast>()
            .ToArray();

        foreach (NotificationToast toast in toasts)
        {
            _stack.Controls.Remove(toast);
            toast.Dispose();
        }

        Visible = false;
        NotificationsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void FocusLatest()
    {
        NotificationToast? latest = _stack.Controls
            .OfType<NotificationToast>()
            .FirstOrDefault();
        latest?.FocusDismissAction();
    }

    public void ApplyTheme()
    {
        BackColor = Color.Transparent;
        ForeColor = ThemePalette.PrimaryText;
        Font = UiFonts.Body;
        _stack.BackColor = Color.Transparent;

        foreach (NotificationToast toast in _stack.Controls
            .OfType<NotificationToast>())
        {
            toast.ApplyTheme();
        }
    }

    private sealed class NotificationToast
        : UserControl, IThemeAwareControl
    {
        private readonly ShellNotification _notification;
        private readonly Action<Guid> _dismiss;
        private readonly Label _messageLabel = new();
        private readonly DarkButton _actionButton = new();
        private readonly DarkButton _dismissButton = new();
        private readonly System.Windows.Forms.Timer? _dismissTimer;

        public NotificationToast(
            ShellNotification notification,
            Action<Guid> dismiss)
        {
            _notification = notification;
            _dismiss = dismiss;
            NotificationId = Guid.NewGuid();

            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = false;
            Width = UiDimensions.NotificationMaximumWidth;
            Height = UiDimensions.NotificationMinimumHeight;
            MinimumSize = new Size(
                0,
                UiDimensions.NotificationMinimumHeight);
            Margin = new Padding(
                0,
                0,
                0,
                UiSpacing.Space8);
            Padding = new Padding(UiSpacing.Space16);
            TabStop = false;
            AccessibleRole = AccessibleRole.Alert;
            AccessibleName = notification.Severity.ToString();
            AccessibleDescription = notification.Message;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
            };
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.AutoSize));

            _messageLabel.AutoSize = true;
            _messageLabel.MaximumSize = new Size(
                UiDimensions.NotificationMaximumWidth
                    - (UiSpacing.Space16 * 2)
                    - (UiDimensions.StandardButtonMinimumWidth * 2),
                0);
            _messageLabel.Text = notification.Message;
            _messageLabel.Anchor = AnchorStyles.Left;
            _messageLabel.Margin = Padding.Empty;

            _actionButton.Text = notification.ActionText ?? string.Empty;
            _actionButton.Variant = ButtonVariant.Ghost;
            _actionButton.SizeVariant = ControlSize.Compact;
            _actionButton.Visible =
                !string.IsNullOrWhiteSpace(notification.ActionText)
                && notification.Action is not null;
            _actionButton.Margin = new Padding(
                UiSpacing.Space8,
                0,
                0,
                0);
            _actionButton.Click += (_, _) =>
            {
                notification.Action?.Invoke();
                _dismiss(NotificationId);
            };

            _dismissButton.Text = "Dismiss";
            _dismissButton.Variant = ButtonVariant.Ghost;
            _dismissButton.SizeVariant = ControlSize.Compact;
            _dismissButton.Margin = new Padding(
                UiSpacing.Space8,
                0,
                0,
                0);
            _dismissButton.Click += (_, _) =>
                _dismiss(NotificationId);

            int actionWidth = _actionButton.Visible
                ? _actionButton.Width + UiSpacing.Space8
                : 0;
            int messageWidth = Math.Max(
                UiDimensions.SummaryCardWidth / 2,
                Width
                    - Padding.Horizontal
                    - _dismissButton.Width
                    - UiSpacing.Space8
                    - actionWidth);
            _messageLabel.MaximumSize = new Size(
                messageWidth,
                0);
            Size messageSize = TextRenderer.MeasureText(
                notification.Message,
                UiFonts.BodyStrong,
                new Size(messageWidth, 0),
                TextFormatFlags.WordBreak
                    | TextFormatFlags.NoPadding);
            Height = Math.Max(
                UiDimensions.NotificationMinimumHeight,
                messageSize.Height + Padding.Vertical);

            layout.Controls.Add(_messageLabel, 0, 0);
            layout.Controls.Add(_actionButton, 1, 0);
            layout.Controls.Add(_dismissButton, 2, 0);
            Controls.Add(layout);

            if (CanAutoDismiss(notification))
            {
                _dismissTimer = new System.Windows.Forms.Timer
                {
                    Interval = (int)Math.Clamp(
                        notification.AutoDismissAfter!.Value
                            .TotalMilliseconds,
                        1D,
                        int.MaxValue),
                };
                _dismissTimer.Tick += (_, _) =>
                {
                    _dismissTimer.Stop();
                    _dismiss(NotificationId);
                };
                _dismissTimer.Start();
            }

            ApplyTheme();
        }

        public Guid NotificationId { get; }

        public void ApplyTheme()
        {
            SemanticColors colors = SemanticTheme.GetColors(
                _notification.Severity switch
                {
                    ShellNotificationSeverity.Information =>
                        SemanticRole.Information,
                    ShellNotificationSeverity.Success =>
                        SemanticRole.Success,
                    ShellNotificationSeverity.Warning =>
                        SemanticRole.Warning,
                    ShellNotificationSeverity.Error =>
                        SemanticRole.Danger,
                    _ => throw new InvalidOperationException(
                        "Unknown notification severity."),
                });
            BackColor = colors.Background;
            ForeColor = colors.Text;
            Font = UiFonts.Body;
            ControlStyler.StyleLabel(
                _messageLabel,
                ThemeTextRole.BodyStrong,
                colors.Text);

            foreach (Control child in Controls)
            {
                child.BackColor = BackColor;
                child.ForeColor = ForeColor;
            }

            _actionButton.ApplyTheme();
            _dismissButton.ApplyTheme();
            Invalidate(true);
        }

        public void FocusDismissAction()
        {
            _ = _dismissButton.Focus();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dismissTimer?.Stop();
                _dismissTimer?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            SemanticColors colors = SemanticTheme.GetColors(
                _notification.Severity switch
                {
                    ShellNotificationSeverity.Information =>
                        SemanticRole.Information,
                    ShellNotificationSeverity.Success =>
                        SemanticRole.Success,
                    ShellNotificationSeverity.Warning =>
                        SemanticRole.Warning,
                    ShellNotificationSeverity.Error =>
                        SemanticRole.Danger,
                    _ => throw new InvalidOperationException(
                        "Unknown notification severity."),
                });
            using var pen = new Pen(
                colors.Border,
                DpiScaler.Scale(
                    UiDimensions.StandardBorderWidth,
                    DeviceDpi));
            e.Graphics.DrawRectangle(
                pen,
                Rectangle.Inflate(ClientRectangle, -1, -1));
        }

        private static bool CanAutoDismiss(
            ShellNotification notification)
        {
            return notification.AutoDismissAfter is not null
                && notification.AutoDismissAfter > TimeSpan.Zero
                && notification.Severity is
                    ShellNotificationSeverity.Success
                    or ShellNotificationSeverity.Information;
        }
    }
}
