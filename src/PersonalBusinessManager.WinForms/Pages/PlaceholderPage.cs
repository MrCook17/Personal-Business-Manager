using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Pages;

public sealed class PlaceholderPage : UserControl
{
    public PlaceholderPage(
        string headingText,
        string descriptionText)
    {
        Dock = DockStyle.Fill;
        BackColor = ThemePalette.ApplicationBackground;

        Label heading = new()
        {
            AutoSize = true,
            Text = headingText,
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
            MaximumSize = new Size(700, 0),
            Text = descriptionText,
            ForeColor = ThemePalette.SecondaryText,
            Font = new Font("Segoe UI", 10F),
            Location = new Point(0, 40)
        };

        Controls.Add(heading);
        Controls.Add(description);
    }
}