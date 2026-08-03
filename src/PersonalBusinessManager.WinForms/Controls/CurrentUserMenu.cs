using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DefaultProperty(nameof(UserDisplayName))]
[DesignerCategory("Code")]
public sealed class CurrentUserMenu : UserControl, IThemeAwareControl
{
    private readonly DarkButton _menuButton = new();
    private readonly ContextMenuStrip _menu = new();
    private readonly ToolStripMenuItem _lockItem = new("Lock");
    private readonly ToolStripMenuItem _accountItem =
        new("Account / Security");
    private readonly ToolStripMenuItem _signOutItem = new("Sign out");
    private string _userDisplayName = "User unavailable";
    private bool _sessionAvailable;

    public CurrentUserMenu()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = false;
        Size = new Size(
            UiDimensions.HeaderStatusControlWidth,
            UiDimensions.StandardControlHeight);
        MinimumSize = new Size(
            UiDimensions.LargeButtonMinimumWidth,
            UiDimensions.StandardControlHeight);
        Margin = Padding.Empty;
        TabStop = false;

        _menuButton.Dock = DockStyle.Fill;
        _menuButton.Text = _userDisplayName;
        _menuButton.Variant = ButtonVariant.Ghost;
        _menuButton.AccessibleRole = AccessibleRole.MenuItem;
        _menuButton.Click += MenuButton_Click;

        _lockItem.Click += (_, _) =>
            LockRequested?.Invoke(this, EventArgs.Empty);
        _accountItem.Click += (_, _) =>
            AccountSecurityRequested?.Invoke(this, EventArgs.Empty);
        _signOutItem.Click += (_, _) =>
            SignOutRequested?.Invoke(this, EventArgs.Empty);
        _menu.Items.AddRange(
            [_lockItem, _accountItem, _signOutItem]);

        Controls.Add(_menuButton);
        ApplyTheme();
        UpdateAvailability();
    }

    public event EventHandler? LockRequested;

    public event EventHandler? AccountSecurityRequested;

    public event EventHandler? SignOutRequested;

    [DefaultValue("User unavailable")]
    public string UserDisplayName
    {
        get => _userDisplayName;
        set
        {
            _userDisplayName = string.IsNullOrWhiteSpace(value)
                ? "User unavailable"
                : value.Trim();
            UpdateButtonText();
        }
    }

    [DefaultValue(false)]
    public bool SessionAvailable
    {
        get => _sessionAvailable;
        set
        {
            if (_sessionAvailable == value)
            {
                return;
            }

            _sessionAvailable = value;
            UpdateAvailability();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public bool SessionActionsEnabled => _lockItem.Enabled;

    public void ApplyTheme()
    {
        BackColor = ThemePalette.HeaderBackground;
        ForeColor = ThemePalette.PrimaryText;
        Font = UiFonts.Body;
        _menuButton.ApplyTheme();
        ControlStyler.StyleContextMenu(_menu);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _menu.Dispose();
        }

        base.Dispose(disposing);
    }

    private void MenuButton_Click(object? sender, EventArgs e)
    {
        _menu.Show(
            _menuButton,
            new Point(0, _menuButton.Height));
    }

    private void UpdateAvailability()
    {
        _lockItem.Enabled = SessionAvailable;
        _accountItem.Enabled = SessionAvailable;
        _signOutItem.Enabled = SessionAvailable;
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        _menuButton.Text = SessionAvailable
            ? $"{UserDisplayName}  ▾"
            : "User (Phase 3)  ▾";
        _menuButton.AccessibleName = SessionAvailable
            ? $"Current user menu for {UserDisplayName}"
            : "Current user menu placeholder for Phase 3";
        _menuButton.AccessibleDescription = SessionAvailable
            ? "Open session actions."
            : "Authentication and session actions become available in Phase 3.";
    }
}
