using System.ComponentModel;
using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Dialogs;

public enum ConfirmationSeverity
{
    Standard,
    Strong,
    Danger,
}

[DesignerCategory("Code")]
public sealed class ConfirmDialog : Form, IThemeAwareControl
{
    private readonly Label _headingLabel = new();
    private readonly Label _messageLabel = new();
    private readonly StatusBadge _consequenceBadge = new();
    private readonly Label _confirmationInstructionLabel = new();
    private readonly DarkTextBox _confirmationInput = new();
    private readonly DarkButton _confirmButton = new();
    private readonly DarkButton _cancelButton = new();
    private string _requiredConfirmationText = string.Empty;
    private ConfirmationSeverity _severity;

    public ConfirmDialog()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(
            UiDimensions.ConfirmationDialogWidth,
            UiDimensions.SummaryCardHeight * 2);
        MinimumSize = new Size(
            UiDimensions.ConfirmationDialogWidth,
            UiDimensions.SummaryCardHeight * 2);
        Padding = Padding.Empty;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var content = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 5,
            Margin = Padding.Empty,
            Padding = new Padding(UiSpacing.Space24),
        };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _headingLabel.AutoSize = true;
        _headingLabel.Text = "Confirm action";
        _headingLabel.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space8);

        _messageLabel.AutoSize = true;
        _messageLabel.MaximumSize = new Size(
            UiDimensions.ConfirmationDialogWidth
                - (UiSpacing.Space24 * 2),
            0);
        _messageLabel.Text =
            "Review the consequence before continuing.";
        _messageLabel.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space16);

        _consequenceBadge.Text = "Confirmation required";
        _consequenceBadge.SemanticRole = SemanticRole.Warning;
        _consequenceBadge.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space16);

        _confirmationInstructionLabel.AutoSize = true;
        _confirmationInstructionLabel.Visible = false;
        _confirmationInstructionLabel.Margin = new Padding(
            0,
            0,
            0,
            UiSpacing.Space4);

        _confirmationInput.Visible = false;
        _confirmationInput.Width =
            UiDimensions.ConfirmationDialogWidth
                - (UiSpacing.Space24 * 2);
        _confirmationInput.Margin = Padding.Empty;
        _confirmationInput.TextChanged += (_, _) =>
            UpdateConfirmAvailability();

        content.Controls.Add(_headingLabel, 0, 0);
        content.Controls.Add(_messageLabel, 0, 1);
        content.Controls.Add(_consequenceBadge, 0, 2);
        content.Controls.Add(_confirmationInstructionLabel, 0, 3);
        content.Controls.Add(_confirmationInput, 0, 4);

        var actionBar = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(
                UiSpacing.Space24,
                UiSpacing.Space16,
                UiSpacing.Space24,
                UiSpacing.Space16),
            Margin = Padding.Empty,
        };

        _confirmButton.Text = "Continue";
        _confirmButton.Variant = ButtonVariant.Primary;
        _confirmButton.MinimumSize = new Size(
            UiDimensions.DialogActionButtonMinimumWidth,
            UiDimensions.StandardControlHeight);
        _confirmButton.Margin = Padding.Empty;
        _confirmButton.DialogResult = DialogResult.OK;

        _cancelButton.Text = "Cancel";
        _cancelButton.Variant = ButtonVariant.Ghost;
        _cancelButton.MinimumSize = new Size(
            UiDimensions.DialogActionButtonMinimumWidth,
            UiDimensions.StandardControlHeight);
        _cancelButton.Margin = new Padding(
            0,
            0,
            UiSpacing.Space8,
            0);
        _cancelButton.DialogResult = DialogResult.Cancel;

        actionBar.Controls.Add(_confirmButton);
        actionBar.Controls.Add(_cancelButton);
        root.Controls.Add(content, 0, 0);
        root.Controls.Add(actionBar, 0, 1);
        Controls.Add(root);

        CancelButton = _cancelButton;
        ApplyTheme();
        Configure(
            "Confirm action",
            "Confirm action",
            "Review the consequence before continuing.",
            "Continue",
            ConfirmationSeverity.Standard);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public ConfirmationSeverity Severity => _severity;

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public string RequiredConfirmationText =>
        _requiredConfirmationText;

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public DarkButton ConfirmButton => _confirmButton;

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public DarkTextBox ConfirmationInput => _confirmationInput;

    public void Configure(
        string windowTitle,
        string heading,
        string message,
        string confirmActionText,
        ConfirmationSeverity severity,
        string? requiredConfirmationText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(windowTitle);
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmActionText);

        Text = windowTitle;
        _headingLabel.Text = heading;
        _messageLabel.Text = message;
        _confirmButton.Text = confirmActionText;
        _severity = severity;
        _requiredConfirmationText =
            requiredConfirmationText?.Trim() ?? string.Empty;

        _consequenceBadge.SemanticRole = severity switch
        {
            ConfirmationSeverity.Standard => SemanticRole.Neutral,
            ConfirmationSeverity.Strong => SemanticRole.Warning,
            ConfirmationSeverity.Danger => SemanticRole.Danger,
            _ => throw new ArgumentOutOfRangeException(
                nameof(severity)),
        };
        _consequenceBadge.Text = severity switch
        {
            ConfirmationSeverity.Standard => "Confirmation required",
            ConfirmationSeverity.Strong => "Review the consequence",
            ConfirmationSeverity.Danger => "Destructive action",
            _ => throw new ArgumentOutOfRangeException(
                nameof(severity)),
        };
        _confirmButton.Variant = severity == ConfirmationSeverity.Danger
            ? ButtonVariant.Danger
            : ButtonVariant.Primary;

        bool requiresTypedConfirmation =
            !string.IsNullOrWhiteSpace(_requiredConfirmationText);
        _confirmationInstructionLabel.Visible =
            requiresTypedConfirmation;
        _confirmationInput.Visible = requiresTypedConfirmation;
        _confirmationInstructionLabel.Text = requiresTypedConfirmation
            ? $"Type {_requiredConfirmationText} to continue."
            : string.Empty;
        _confirmationInput.Text = string.Empty;

        ClientSize = new Size(
            UiDimensions.ConfirmationDialogWidth,
            requiresTypedConfirmation
                ? (UiDimensions.SummaryCardHeight * 3)
                    + UiDimensions.StandardControlHeight
                : (UiDimensions.SummaryCardHeight * 2)
                    + UiDimensions.TimerStripHeight
                    + UiDimensions.StandardControlHeight);

        AcceptButton = severity == ConfirmationSeverity.Standard
            && !requiresTypedConfirmation
            ? _confirmButton
            : null;
        UpdateConfirmAvailability();
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        ControlStyler.StyleDialog(this);

        foreach (Control child in Controls)
        {
            ApplyDialogSurface(child);
        }

        ControlStyler.StyleLabel(
            _headingLabel,
            ThemeTextRole.DialogHeading);
        ControlStyler.StyleLabel(
            _messageLabel,
            ThemeTextRole.Body,
            ThemePalette.SecondaryText);
        ControlStyler.StyleLabel(
            _confirmationInstructionLabel,
            ThemeTextRole.Label);
        _consequenceBadge.ApplyTheme();
        _confirmationInput.ApplyTheme();
        _confirmButton.ApplyTheme();
        _cancelButton.ApplyTheme();
    }

    public static DialogResult ShowConfirmation(
        IWin32Window owner,
        string windowTitle,
        string heading,
        string message,
        string confirmActionText,
        ConfirmationSeverity severity = ConfirmationSeverity.Standard,
        string? requiredConfirmationText = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        using var dialog = new ConfirmDialog();
        dialog.Configure(
            windowTitle,
            heading,
            message,
            confirmActionText,
            severity,
            requiredConfirmationText);
        return dialog.ShowDialog(owner);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_confirmationInput.Visible)
        {
            _ = _confirmationInput.EditorTextBox.Focus();
        }
        else
        {
            _ = _cancelButton.Focus();
        }
    }

    private void UpdateConfirmAvailability()
    {
        _confirmButton.Enabled =
            string.IsNullOrEmpty(_requiredConfirmationText)
            || string.Equals(
                _confirmationInput.Text,
                _requiredConfirmationText,
                StringComparison.Ordinal);
    }

    private static void ApplyDialogSurface(Control control)
    {
        if (control is Panel
            or TableLayoutPanel
            or FlowLayoutPanel)
        {
            control.BackColor = control.Dock == DockStyle.Fill
                && control.Parent is TableLayoutPanel
                && control.Parent.Controls.GetChildIndex(control) > 0
                ? ThemePalette.PanelBackground
                : ThemePalette.RaisedPanel;
            control.ForeColor = ThemePalette.PrimaryText;
            control.Font = UiFonts.Body;
        }

        foreach (Control child in control.Controls)
        {
            ApplyDialogSurface(child);
        }
    }
}
