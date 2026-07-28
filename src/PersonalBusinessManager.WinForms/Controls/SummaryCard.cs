using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public sealed class SummaryCard : Panel
{
    private readonly Label _valueLabel;

    public SummaryCard(string heading, string value)
    {
        Width = 230;
        Height = 110;
        Margin = new Padding(0, 0, 16, 16);
        Padding = new Padding(16);
        BackColor = ThemePalette.RaisedPanel;

        Label headingLabel = new()
        {
            AutoSize = true,
            Text = heading,
            ForeColor = ThemePalette.SecondaryText,
            Font = new Font("Segoe UI", 10F),
            Location = new Point(16, 16)
        };

        _valueLabel = new Label
        {
            AutoSize = true,
            Text = value,
            ForeColor = ThemePalette.PrimaryText,
            Font = new Font(
                "Segoe UI",
                20F,
                FontStyle.Bold),
            Location = new Point(16, 47)
        };

        Controls.Add(headingLabel);
        Controls.Add(_valueLabel);
    }

    public void SetValue(string value)
    {
        _valueLabel.Text = value;
    }
}