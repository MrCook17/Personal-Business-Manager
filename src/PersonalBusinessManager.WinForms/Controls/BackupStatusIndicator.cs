using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public enum BackupHealthState
{
    NotConfigured,
    Healthy,
    InProgress,
    Warning,
    Failed,
}

public sealed record BackupStatusSnapshot(
    BackupHealthState State,
    string Message,
    DateTimeOffset? LastSuccessfulBackup = null)
{
    public static BackupStatusSnapshot NotConfigured { get; } =
        new(
            BackupHealthState.NotConfigured,
            "Not configured");
}

[DefaultEvent(nameof(Click))]
[DesignerCategory("Code")]
public sealed class BackupStatusIndicator : Button, IThemeAwareControl
{
    private BackupStatusSnapshot _snapshot =
        BackupStatusSnapshot.NotConfigured;

    public BackupStatusIndicator()
    {
        AutoSize = false;
        Size = new Size(
            UiDimensions.HeaderStatusControlWidth,
            UiDimensions.StandardControlHeight);
        MinimumSize = new Size(
            UiDimensions.LargeButtonMinimumWidth,
            UiDimensions.StandardControlHeight);
        FlatStyle = FlatStyle.Flat;
        TextAlign = ContentAlignment.MiddleCenter;
        AutoEllipsis = true;
        Cursor = Cursors.Hand;
        TabStop = true;
        AccessibleRole = AccessibleRole.PushButton;
        ApplyTheme();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public BackupStatusSnapshot Snapshot
    {
        get => _snapshot;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentException.ThrowIfNullOrWhiteSpace(value.Message);
            _snapshot = value;
            ApplyTheme();
        }
    }

    public void ApplyTheme()
    {
        SemanticRole role = Snapshot.State switch
        {
            BackupHealthState.Healthy => SemanticRole.Success,
            BackupHealthState.InProgress => SemanticRole.Information,
            BackupHealthState.Warning => SemanticRole.Warning,
            BackupHealthState.Failed => SemanticRole.Danger,
            _ => SemanticRole.Neutral,
        };
        SemanticColors colors = SemanticTheme.GetColors(role);

        Font = UiFonts.Small;
        BackColor = Enabled
            ? colors.Background
            : ThemePalette.InputDisabledBackground;
        ForeColor = Enabled
            ? colors.Text
            : ThemePalette.DisabledText;
        FlatAppearance.BorderColor = Focused
            ? ThemePalette.FocusBorder
            : Enabled
                ? colors.Border
                : ThemePalette.BorderSubtle;
        FlatAppearance.BorderSize = DpiScaler.Scale(
            Focused
                ? UiDimensions.FocusBorderWidth
                : UiDimensions.StandardBorderWidth,
            DeviceDpi);
        Text = $"Backup: {Snapshot.Message}";
        AccessibleName = Text;
        AccessibleDescription = Snapshot.LastSuccessfulBackup is null
            ? Text
            : $"{Text}. Last successful backup "
                + Snapshot.LastSuccessfulBackup.Value
                    .ToLocalTime()
                    .ToString("g", System.Globalization.CultureInfo
                        .GetCultureInfo("en-GB"));
        Invalidate();
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

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        TabStop = Enabled;
        ApplyTheme();
    }
}
