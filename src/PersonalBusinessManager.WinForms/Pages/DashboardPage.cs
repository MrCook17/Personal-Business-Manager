using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Pages;

public sealed class DashboardPage : UserControl
{
    public DashboardPage()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemePalette.ApplicationBackground;

        Label heading = new()
        {
            AutoSize = true,
            Text = "Application foundation",
            ForeColor = ThemePalette.PrimaryText,
            Font = new Font(
                "Segoe UI",
                15F,
                FontStyle.Bold),
            Location = new Point(0, 0)
        };

        Label description = new()
        {
            AutoSize = true,
            Text =
                "The Phase 2 application shell is running. " +
                "Feature data will be added in later phases.",
            ForeColor = ThemePalette.SecondaryText,
            Font = new Font("Segoe UI", 10F),
            Location = new Point(0, 38)
        };

        FlowLayoutPanel cards = new()
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Location = new Point(0, 85),
            BackColor = ThemePalette.ApplicationBackground
        };

        cards.Controls.Add(new SummaryCard("Current phase", "2"));
        cards.Controls.Add(new SummaryCard("Application", "Running"));
        cards.Controls.Add(new SummaryCard("Theme", "Dark"));

        Controls.Add(heading);
        Controls.Add(description);
        Controls.Add(cards);
    }
}