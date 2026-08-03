using System.ComponentModel;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

[DesignerCategory("Code")]
public sealed class FilterBar : UserControl, IThemeAwareControl
{
    private readonly FlowLayoutPanel _contentPanel = new();

    public FilterBar()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Dock = DockStyle.Top;
        MinimumSize = new Size(0, UiDimensions.FilterBarMinimumHeight);
        Margin = new Padding(0, 0, 0, UiSpacing.Space16);
        Padding = Padding.Empty;
        TabStop = false;

        _contentPanel.AutoSize = true;
        _contentPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _contentPanel.Dock = DockStyle.Top;
        _contentPanel.FlowDirection = FlowDirection.LeftToRight;
        _contentPanel.WrapContents = true;
        _contentPanel.Margin = Padding.Empty;
        _contentPanel.Padding = new Padding(
            UiSpacing.Space16,
            UiDimensions.FilterRowVerticalPadding,
            UiSpacing.Space16,
            UiDimensions.FilterRowVerticalPadding);

        Controls.Add(_contentPanel);
        ApplyTheme();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(
        DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<Control> FilterControls =>
        _contentPanel.Controls.Cast<Control>().ToArray();

    public void AddFilter(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        control.Margin = new Padding(0, 0, UiSpacing.Space8, 0);
        _contentPanel.Controls.Add(control);
        ThemeManager.ApplyControlTree(control);
    }

    public void ClearFilters()
    {
        Control[] controls = _contentPanel.Controls
            .Cast<Control>()
            .ToArray();
        _contentPanel.Controls.Clear();

        foreach (Control control in controls)
        {
            control.Dispose();
        }
    }

    public void ApplyTheme()
    {
        ControlStyler.StylePanel(this, ThemeSurface.Panel);
        ControlStyler.StylePanel(_contentPanel, ThemeSurface.Panel);

        foreach (Control control in _contentPanel.Controls)
        {
            ThemeManager.ApplyControlTree(control);
        }
    }
}
