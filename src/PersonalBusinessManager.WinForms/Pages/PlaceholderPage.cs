using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Pages;

public sealed class PlaceholderPage : UserControl
{
    public PlaceholderPage(
        string headingText,
        string descriptionText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(headingText);
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptionText);

        Dock = DockStyle.Fill;
        AutoScroll = true;
        ControlStyler.StylePanel(
            this,
            ThemeSurface.Application);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            MaximumSize = new Size(700, 0),
        };
        ControlStyler.StylePanel(
            layout,
            ThemeSurface.Application);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label
        {
            AutoSize = true,
            Text = headingText,
            Margin = new Padding(
                0,
                0,
                0,
                UiSpacing.Space8),
        };
        ControlStyler.StyleLabel(
            heading,
            ThemeTextRole.SectionHeading);

        var description = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(700, 0),
            Text = descriptionText,
            Margin = Padding.Empty,
        };
        ControlStyler.StyleLabel(
            description,
            ThemeTextRole.Body,
            ThemePalette.SecondaryText);

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(description, 0, 1);
        Controls.Add(layout);

        ThemeManager.ApplyControlTree(this);
    }
}
