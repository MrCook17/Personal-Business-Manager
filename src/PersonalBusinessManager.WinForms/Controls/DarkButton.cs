using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public sealed class DarkButton : Button
{
    private bool _isSelected;

    public DarkButton()
    {
        AutoSize = false;
        Height = 40;
        Width = 188;

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;

        BackColor = ThemePalette.SidebarBackground;
        ForeColor = ThemePalette.SecondaryText;

        Font = new Font(
            "Segoe UI",
            10F,
            FontStyle.Regular,
            GraphicsUnit.Point);

        TextAlign = ContentAlignment.MiddleLeft;
        Padding = new Padding(16, 0, 8, 0);
        Cursor = Cursors.Hand;
        TabStop = true;
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            ApplyState();
        }
    }

    protected override void OnMouseEnter(EventArgs eventArgs)
    {
        base.OnMouseEnter(eventArgs);

        if (!IsSelected)
        {
            BackColor = ThemePalette.RaisedPanel;
            ForeColor = ThemePalette.PrimaryText;
        }
    }

    protected override void OnMouseLeave(EventArgs eventArgs)
    {
        base.OnMouseLeave(eventArgs);
        ApplyState();
    }

    private void ApplyState()
    {
        BackColor = IsSelected
            ? ThemePalette.Accent
            : ThemePalette.SidebarBackground;

        ForeColor = IsSelected
            ? ThemePalette.PrimaryText
            : ThemePalette.SecondaryText;
    }
}