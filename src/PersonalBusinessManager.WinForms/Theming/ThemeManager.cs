using System.ComponentModel;

namespace PersonalBusinessManager.WinForms.Theming;

public interface IThemeAwareControl
{
    void ApplyTheme();
}

public sealed record ThemeValidationIssue(
    string ControlType,
    string ControlName,
    string Problem);

public static class ThemeManager
{
    private static readonly HashSet<int> ApprovedBackgrounds =
    [
        ThemePalette.ApplicationBackground.ToArgb(),
        ThemePalette.SidebarBackground.ToArgb(),
        ThemePalette.HeaderBackground.ToArgb(),
        ThemePalette.PanelBackground.ToArgb(),
        ThemePalette.RaisedPanel.ToArgb(),
        ThemePalette.InputBackground.ToArgb(),
        ThemePalette.InputHoverBackground.ToArgb(),
        ThemePalette.InputDisabledBackground.ToArgb(),
        ThemePalette.OverlayBackground.ToArgb(),
        ThemePalette.TooltipBackground.ToArgb(),
        ThemePalette.GridAlternateRow.ToArgb(),
        ThemePalette.GridSelectedRow.ToArgb(),
        ThemePalette.GridHoverRow.ToArgb(),
        ThemePalette.Accent.ToArgb(),
        ThemePalette.AccentHover.ToArgb(),
        ThemePalette.AccentPressed.ToArgb(),
        ThemePalette.AccentSoft.ToArgb(),
        ThemePalette.SuccessSoft.ToArgb(),
        ThemePalette.WarningSoft.ToArgb(),
        ThemePalette.DangerSoft.ToArgb(),
        ThemePalette.InformationSoft.ToArgb(),
        ThemePalette.NeutralSoft.ToArgb(),
    ];

    private static readonly HashSet<int> ApprovedForegrounds =
    [
        ThemePalette.PrimaryText.ToArgb(),
        ThemePalette.SecondaryText.ToArgb(),
        ThemePalette.MutedText.ToArgb(),
        ThemePalette.DisabledText.ToArgb(),
        ThemePalette.InverseText.ToArgb(),
        ThemePalette.LinkText.ToArgb(),
        ThemePalette.LinkHoverText.ToArgb(),
        ThemePalette.PlaceholderText.ToArgb(),
        ThemePalette.AccentText.ToArgb(),
        ThemePalette.Success.ToArgb(),
        ThemePalette.SuccessText.ToArgb(),
        ThemePalette.Warning.ToArgb(),
        ThemePalette.WarningText.ToArgb(),
        ThemePalette.Danger.ToArgb(),
        ThemePalette.DangerText.ToArgb(),
        ThemePalette.Information.ToArgb(),
        ThemePalette.InformationText.ToArgb(),
        ThemePalette.Neutral.ToArgb(),
        ThemePalette.NeutralText.ToArgb(),
    ];

    public static void Apply(Form form)
    {
        ArgumentNullException.ThrowIfNull(form);

        ControlStyler.StyleForm(form);
        ApplyControlTree(form);
    }

    public static void ApplyControlTree(Control root)
    {
        ArgumentNullException.ThrowIfNull(root);

        ApplyControl(root);

        foreach (Control child in root.Controls)
        {
            ApplyControlTree(child);
        }
    }

    public static IReadOnlyList<ThemeValidationIssue>
        FindUnthemedControls(Control root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var issues = new List<ThemeValidationIssue>();
        ValidateControl(root, issues);

        return issues;
    }

    private static void ApplyControl(Control control)
    {
        if (control is IThemeAwareControl themeAware)
        {
            themeAware.ApplyTheme();
            return;
        }

        switch (control)
        {
            case Form form:
                ControlStyler.StyleForm(form);
                break;

            case DataGridView grid:
                ControlStyler.StyleDataGridView(grid);
                break;

            case TabControl tabs:
                ControlStyler.StyleTabControl(tabs);
                break;

            case ContextMenuStrip contextMenu:
                ControlStyler.StyleContextMenu(contextMenu);
                break;

            case ToolStrip toolStrip:
                ControlStyler.StyleToolStrip(toolStrip);
                break;

            case TextBoxBase or ComboBox
                or DateTimePicker or NumericUpDown:
                ControlStyler.StyleInput(control);
                break;

            case Button button when HasNoExplicitAppearance(button):
                ControlStyler.StyleButton(button);
                break;

            case Label label when HasNoExplicitAppearance(label):
                ControlStyler.StyleLabel(label);
                break;

            case UserControl userControl
                when HasNoExplicitAppearance(userControl):
                ControlStyler.StylePanel(
                    userControl,
                    ThemeSurface.Application);
                break;

            case Panel or TableLayoutPanel or FlowLayoutPanel
                when HasNoExplicitAppearance(control):
                ControlStyler.StylePanel(control);
                break;
        }
    }

    private static bool HasNoExplicitAppearance(Control control)
    {
        PropertyDescriptorCollection properties =
            TypeDescriptor.GetProperties(control);

        return properties[nameof(Control.BackColor)]
                ?.ShouldSerializeValue(control) != true
            && properties[nameof(Control.ForeColor)]
                ?.ShouldSerializeValue(control) != true
            && properties[nameof(Control.Font)]
                ?.ShouldSerializeValue(control) != true;
    }

    private static void ValidateControl(
        Control control,
        List<ThemeValidationIssue> issues)
    {
        if (RequiresThemedBackground(control)
            && control.BackColor != Color.Transparent
            && !ApprovedBackgrounds.Contains(
                control.BackColor.ToArgb()))
        {
            issues.Add(new ThemeValidationIssue(
                control.GetType().Name,
                GetSafeControlName(control),
                "Background does not use an approved theme token."));
        }

        if (RequiresThemedForeground(control)
            && !ApprovedForegrounds.Contains(
                control.ForeColor.ToArgb()))
        {
            issues.Add(new ThemeValidationIssue(
                control.GetType().Name,
                GetSafeControlName(control),
                "Text does not use an approved theme token."));
        }

        foreach (Control child in control.Controls)
        {
            ValidateControl(child, issues);
        }
    }

    private static bool RequiresThemedBackground(Control control)
    {
        return control is Form
            or UserControl
            or Panel
            or TableLayoutPanel
            or FlowLayoutPanel
            or Button
            or TextBoxBase
            or ComboBox
            or DateTimePicker
            or NumericUpDown
            or DataGridView
            or TabControl;
    }

    private static bool RequiresThemedForeground(Control control)
    {
        return control is Label
            or Button
            or TextBoxBase
            or ComboBox
            or DateTimePicker
            or NumericUpDown
            or DataGridView
            or TabControl;
    }

    private static string GetSafeControlName(Control control)
    {
        return string.IsNullOrWhiteSpace(control.Name)
            ? "<unnamed>"
            : control.Name;
    }
}
