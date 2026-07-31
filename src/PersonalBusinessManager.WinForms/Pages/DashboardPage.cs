using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Pages;

public sealed class DashboardPage : UserControl
{
    public DashboardPage()
    {
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
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(
            layout,
            ThemeSurface.Application);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label
        {
            AutoSize = true,
            Text = "Application foundation",
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
            Text =
                "The Phase 2 application shell is running. "
                + "Feature data will be added in later phases.",
            Margin = new Padding(
                0,
                0,
                0,
                UiSpacing.Space24),
        };
        ControlStyler.StyleLabel(
            description,
            ThemeTextRole.Body,
            ThemePalette.SecondaryText);

        var cards = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        ControlStyler.StylePanel(
            cards,
            ThemeSurface.Application);

        cards.Controls.Add(
            new SummaryCard("Current phase", "2"));
        cards.Controls.Add(
            new SummaryCard("Application", "Running"));
        cards.Controls.Add(
            new SummaryCard("Theme", "Dark"));

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(description, 0, 1);
        layout.Controls.Add(cards, 0, 2);
        Controls.Add(layout);

        ThemeManager.ApplyControlTree(this);
    }
}
