using System.Reflection;
using System.Runtime.CompilerServices;

namespace PersonalBusinessManager.WinForms.Theming;

public enum ThemeSurface
{
    Application,
    Sidebar,
    Header,
    Panel,
    Raised,
}

public enum ThemeTextRole
{
    Caption,
    Small,
    Body,
    BodyStrong,
    Label,
    SectionHeading,
    DialogHeading,
    PageHeading,
    DashboardValue,
    MonospaceSmall,
}

public enum ButtonVariant
{
    Primary,
    Secondary,
    Ghost,
    Danger,
}

public enum ControlSize
{
    Compact,
    Standard,
    Large,
}

public static class ControlStyler
{
    private static readonly ConditionalWeakTable<
        Button,
        ButtonStyleRegistration> ButtonRegistrations = new();

    private static readonly ConditionalWeakTable<
        Control,
        InputStyleRegistration> InputRegistrations = new();

    private static readonly ConditionalWeakTable<
        TabControl,
        TabStyleRegistration> TabRegistrations = new();

    private static readonly ConditionalWeakTable<
        ToolStrip,
        ToolStripProfessionalRenderer> ToolStripRenderers = new();

    public static void StyleForm(Form form)
    {
        ArgumentNullException.ThrowIfNull(form);

        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.AutoScaleDimensions = new SizeF(
            DpiScaler.BaselineDpi,
            DpiScaler.BaselineDpi);
        form.BackColor = ThemePalette.ApplicationBackground;
        form.ForeColor = ThemePalette.PrimaryText;
        form.Font = UiFonts.Body;
    }

    public static void StyleDialog(Form dialog)
    {
        StyleForm(dialog);
        dialog.BackColor = ThemePalette.RaisedPanel;
        dialog.ForeColor = ThemePalette.PrimaryText;
        dialog.ShowInTaskbar = false;
    }

    public static void StylePanel(
        Control panel,
        ThemeSurface surface = ThemeSurface.Panel)
    {
        ArgumentNullException.ThrowIfNull(panel);

        panel.BackColor = GetSurfaceColor(surface);
        panel.ForeColor = ThemePalette.PrimaryText;
        panel.Font = UiFonts.Body;
    }

    public static void StyleLabel(
        Label label,
        ThemeTextRole role = ThemeTextRole.Body,
        Color? foreground = null)
    {
        ArgumentNullException.ThrowIfNull(label);

        label.Font = GetFont(role);
        label.ForeColor = foreground ?? GetTextColor(role);
        label.BackColor = Color.Transparent;
    }

    public static void StyleButton(
        Button button,
        ButtonVariant variant = ButtonVariant.Secondary,
        ControlSize size = ControlSize.Standard)
    {
        ArgumentNullException.ThrowIfNull(button);

        ButtonStyleRegistration registration =
            ButtonRegistrations.GetValue(
                button,
                static key => new ButtonStyleRegistration(key));

        registration.Variant = variant;
        registration.Size = size;
        registration.Apply();
    }

    public static void StyleInput(Control input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input is not TextBoxBase
            && input is not ComboBox
            && input is not DateTimePicker
            && input is not NumericUpDown)
        {
            throw new ArgumentException(
                "Input styling supports text, combo, date/time and numeric controls.",
                nameof(input));
        }

        input.Font = UiFonts.Body;
        input.Height = UiDimensions.StandardControlHeight;
        input.MinimumSize = new Size(
            0,
            UiDimensions.StandardControlHeight);

        switch (input)
        {
            case TextBoxBase textBox:
                textBox.BorderStyle = BorderStyle.FixedSingle;
                break;

            case ComboBox comboBox:
                comboBox.FlatStyle = FlatStyle.Flat;
                break;

            case DateTimePicker dateTimePicker:
                dateTimePicker.CalendarMonthBackground =
                    ThemePalette.RaisedPanel;
                dateTimePicker.CalendarForeColor =
                    ThemePalette.PrimaryText;
                dateTimePicker.CalendarTitleBackColor =
                    ThemePalette.HeaderBackground;
                dateTimePicker.CalendarTitleForeColor =
                    ThemePalette.PrimaryText;
                break;
        }

        InputStyleRegistration registration =
            InputRegistrations.GetValue(
                input,
                static key => new InputStyleRegistration(key));
        registration.Apply();
    }

    public static void StyleDataGridView(DataGridView grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        grid.EnableHeadersVisualStyles = false;
        grid.BackgroundColor = ThemePalette.PanelBackground;
        grid.BackColor = ThemePalette.PanelBackground;
        grid.ForeColor = ThemePalette.PrimaryText;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.GridColor = ThemePalette.BorderSubtle;
        grid.CellBorderStyle =
            DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle =
            DataGridViewHeaderBorderStyle.Single;
        grid.RowHeadersVisible = false;
        grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ColumnHeadersHeight =
            UiDimensions.GridHeaderHeight;
        grid.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.RowTemplate.Height = UiDimensions.GridRowHeight;
        grid.Font = UiFonts.Body;

        grid.ColumnHeadersDefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor = ThemePalette.RaisedPanel,
                ForeColor = ThemePalette.SecondaryText,
                SelectionBackColor = ThemePalette.RaisedPanel,
                SelectionForeColor = ThemePalette.PrimaryText,
                Font = UiFonts.BodyStrong,
                Padding = new Padding(
                    UiDimensions.GridCellHorizontalPadding,
                    UiSpacing.Space8,
                    UiDimensions.GridCellHorizontalPadding,
                    UiSpacing.Space8),
            };

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ThemePalette.PanelBackground,
            ForeColor = ThemePalette.PrimaryText,
            SelectionBackColor = ThemePalette.GridSelectedRow,
            SelectionForeColor = ThemePalette.PrimaryText,
            Font = UiFonts.Body,
            Padding = new Padding(
                UiDimensions.GridCellHorizontalPadding,
                UiSpacing.Space8,
                UiDimensions.GridCellHorizontalPadding,
                UiSpacing.Space8),
        };

        grid.AlternatingRowsDefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor = ThemePalette.GridAlternateRow,
                ForeColor = ThemePalette.PrimaryText,
                SelectionBackColor = ThemePalette.GridSelectedRow,
                SelectionForeColor = ThemePalette.PrimaryText,
            };

        SetDoubleBuffered(grid);
    }

    public static void StyleTabControl(TabControl tabs)
    {
        ArgumentNullException.ThrowIfNull(tabs);

        tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.BackColor = ThemePalette.PanelBackground;
        tabs.ForeColor = ThemePalette.PrimaryText;
        tabs.ItemSize = new Size(
            UiDimensions.TabHeaderWidth,
            UiDimensions.TabHeaderHeight);
        tabs.Padding = new Point(
            UiSpacing.Space16,
            UiSpacing.Space8);
        tabs.Font = UiFonts.Button;

        _ = TabRegistrations.GetValue(
            tabs,
            static key => new TabStyleRegistration(key));

        foreach (TabPage page in tabs.TabPages)
        {
            page.BackColor = ThemePalette.PanelBackground;
            page.ForeColor = ThemePalette.PrimaryText;
            page.Font = UiFonts.Body;
        }
    }

    public static void StyleToolStrip(ToolStrip toolStrip)
    {
        ArgumentNullException.ThrowIfNull(toolStrip);

        toolStrip.BackColor = ThemePalette.HeaderBackground;
        toolStrip.ForeColor = ThemePalette.PrimaryText;
        toolStrip.Font = UiFonts.Body;
        toolStrip.RenderMode = ToolStripRenderMode.Professional;
        toolStrip.Renderer = ToolStripRenderers.GetValue(
            toolStrip,
            static _ => new ToolStripProfessionalRenderer(
                new DarkProfessionalColorTable()));
    }

    public static void StyleContextMenu(
        ContextMenuStrip contextMenu)
    {
        StyleToolStrip(contextMenu);
        contextMenu.BackColor = ThemePalette.RaisedPanel;
    }

    private static Font GetFont(ThemeTextRole role)
    {
        return role switch
        {
            ThemeTextRole.Caption => UiFonts.Caption,
            ThemeTextRole.Small => UiFonts.Small,
            ThemeTextRole.Body => UiFonts.Body,
            ThemeTextRole.BodyStrong => UiFonts.BodyStrong,
            ThemeTextRole.Label => UiFonts.Label,
            ThemeTextRole.SectionHeading => UiFonts.SectionHeading,
            ThemeTextRole.DialogHeading => UiFonts.DialogHeading,
            ThemeTextRole.PageHeading => UiFonts.PageHeading,
            ThemeTextRole.DashboardValue => UiFonts.DashboardValue,
            ThemeTextRole.MonospaceSmall => UiFonts.MonospaceSmall,
            _ => throw new ArgumentOutOfRangeException(nameof(role)),
        };
    }

    private static Color GetTextColor(ThemeTextRole role)
    {
        return role is ThemeTextRole.Caption
            or ThemeTextRole.Small
            or ThemeTextRole.MonospaceSmall
            ? ThemePalette.SecondaryText
            : ThemePalette.PrimaryText;
    }

    private static Color GetSurfaceColor(ThemeSurface surface)
    {
        return surface switch
        {
            ThemeSurface.Application =>
                ThemePalette.ApplicationBackground,
            ThemeSurface.Sidebar => ThemePalette.SidebarBackground,
            ThemeSurface.Header => ThemePalette.HeaderBackground,
            ThemeSurface.Panel => ThemePalette.PanelBackground,
            ThemeSurface.Raised => ThemePalette.RaisedPanel,
            _ => throw new ArgumentOutOfRangeException(
                nameof(surface)),
        };
    }

    private static void SetDoubleBuffered(DataGridView grid)
    {
        PropertyInfo? property = typeof(Control).GetProperty(
            "DoubleBuffered",
            BindingFlags.Instance | BindingFlags.NonPublic);
        property?.SetValue(grid, true);
    }

    private sealed class ButtonStyleRegistration
    {
        private readonly Button _button;
        private bool _hovered;
        private bool _pressed;

        public ButtonStyleRegistration(Button button)
        {
            _button = button;
            _button.MouseEnter += (_, _) =>
            {
                _hovered = true;
                Apply();
            };
            _button.MouseLeave += (_, _) =>
            {
                _hovered = false;
                _pressed = false;
                Apply();
            };
            _button.MouseDown += (_, _) =>
            {
                _pressed = true;
                Apply();
            };
            _button.MouseUp += (_, _) =>
            {
                _pressed = false;
                Apply();
            };
            _button.GotFocus += (_, _) => Apply();
            _button.LostFocus += (_, _) => Apply();
            _button.EnabledChanged += (_, _) => Apply();
        }

        public ButtonVariant Variant { get; set; }

        public ControlSize Size { get; set; }

        public void Apply()
        {
            _button.AutoSize = false;
            _button.FlatStyle = FlatStyle.Flat;
            _button.UseVisualStyleBackColor = false;
            _button.Font = UiFonts.Button;
            _button.Cursor = _button.Enabled
                ? Cursors.Hand
                : Cursors.Default;
            _button.TabStop = _button.Enabled;

            (int height, int minimumWidth, int padding) =
                Size switch
                {
                    ControlSize.Compact =>
                        (UiDimensions.CompactControlHeight,
                            UiDimensions.CompactControlHeight,
                            UiSpacing.Space8),
                    ControlSize.Standard =>
                        (UiDimensions.StandardControlHeight,
                            UiDimensions.StandardButtonMinimumWidth,
                            UiSpacing.Space16),
                    ControlSize.Large =>
                        (UiDimensions.LargeControlHeight,
                            UiDimensions.LargeButtonMinimumWidth,
                            UiSpacing.Space24),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(Size)),
                };

            _button.Height = height;
            _button.MinimumSize = new Size(minimumWidth, height);
            _button.Padding = new Padding(padding, 0, padding, 0);

            (Color background, Color foreground, Color border) =
                GetBaseColors();

            if (!_button.Enabled)
            {
                background = ThemePalette.InputDisabledBackground;
                foreground = ThemePalette.DisabledText;
                border = ThemePalette.BorderSubtle;
            }
            else if (_pressed)
            {
                background = Variant == ButtonVariant.Primary
                    ? ThemePalette.AccentPressed
                    : ThemePalette.InputBackground;
            }
            else if (_hovered)
            {
                background = Variant switch
                {
                    ButtonVariant.Primary => ThemePalette.AccentHover,
                    ButtonVariant.Secondary => ThemePalette.RaisedPanel,
                    ButtonVariant.Ghost =>
                        ThemePalette.InputHoverBackground,
                    ButtonVariant.Danger => ThemePalette.DangerText,
                    _ => background,
                };
                foreground = Variant is ButtonVariant.Primary
                    or ButtonVariant.Danger
                    ? ThemePalette.InverseText
                    : ThemePalette.PrimaryText;
                border = Variant == ButtonVariant.Ghost
                    ? ThemePalette.BorderStrong
                    : border;
            }

            _button.BackColor = background;
            _button.ForeColor = foreground;
            _button.FlatAppearance.BorderColor = _button.Focused
                ? ThemePalette.FocusBorder
                : border;
            _button.FlatAppearance.BorderSize =
                _button.Focused
                ? DpiScaler.Scale(
                    UiDimensions.FocusBorderWidth,
                    _button.DeviceDpi)
                : Variant == ButtonVariant.Ghost
                    ? 0
                    : DpiScaler.Scale(
                        UiDimensions.StandardBorderWidth,
                        _button.DeviceDpi);
        }

        private (Color Background, Color Foreground, Color Border)
            GetBaseColors()
        {
            return Variant switch
            {
                ButtonVariant.Primary =>
                    (ThemePalette.Accent,
                        ThemePalette.InverseText,
                        ThemePalette.Accent),
                ButtonVariant.Secondary =>
                    (ThemePalette.RaisedPanel,
                        ThemePalette.PrimaryText,
                        ThemePalette.BorderDefault),
                ButtonVariant.Ghost =>
                    (_button.Parent?.BackColor
                        ?? ThemePalette.PanelBackground,
                        ThemePalette.SecondaryText,
                        ThemePalette.BorderSubtle),
                ButtonVariant.Danger =>
                    (ThemePalette.Danger,
                        ThemePalette.InverseText,
                        ThemePalette.Danger),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(Variant)),
            };
        }
    }

    private sealed class InputStyleRegistration
    {
        private readonly Control _input;
        private bool _hovered;

        public InputStyleRegistration(Control input)
        {
            _input = input;
            _input.MouseEnter += (_, _) =>
            {
                _hovered = true;
                Apply();
            };
            _input.MouseLeave += (_, _) =>
            {
                _hovered = false;
                Apply();
            };
            _input.GotFocus += (_, _) => Apply();
            _input.LostFocus += (_, _) => Apply();
            _input.EnabledChanged += (_, _) => Apply();
        }

        public void Apply()
        {
            bool readOnly = _input is TextBoxBase
                {
                    ReadOnly: true,
                };

            if (!_input.Enabled)
            {
                _input.BackColor =
                    ThemePalette.InputDisabledBackground;
                _input.ForeColor = ThemePalette.DisabledText;
                _input.TabStop = false;
                return;
            }

            _input.TabStop = true;
            _input.BackColor = readOnly
                ? ThemePalette.PanelBackground
                : _hovered || _input.Focused
                    ? ThemePalette.InputHoverBackground
                    : ThemePalette.InputBackground;
            _input.ForeColor = readOnly
                ? ThemePalette.SecondaryText
                : ThemePalette.PrimaryText;
        }
    }

    private sealed class TabStyleRegistration
    {
        private readonly TabControl _tabs;

        public TabStyleRegistration(TabControl tabs)
        {
            _tabs = tabs;
            _tabs.DrawItem += DrawItem;
            _tabs.ControlAdded += (_, eventArgs) =>
            {
                if (eventArgs.Control is TabPage page)
                {
                    page.BackColor = ThemePalette.PanelBackground;
                    page.ForeColor = ThemePalette.PrimaryText;
                    page.Font = UiFonts.Body;
                }
            };
        }

        private void DrawItem(
            object? sender,
            DrawItemEventArgs eventArgs)
        {
            bool selected = eventArgs.Index == _tabs.SelectedIndex;
            Rectangle bounds = eventArgs.Bounds;

            using var backgroundBrush = new SolidBrush(
                selected
                    ? ThemePalette.AccentSoft
                    : ThemePalette.PanelBackground);
            eventArgs.Graphics.FillRectangle(
                backgroundBrush,
                bounds);

            TextRenderer.DrawText(
                eventArgs.Graphics,
                _tabs.TabPages[eventArgs.Index].Text,
                UiFonts.Button,
                bounds,
                selected
                    ? ThemePalette.PrimaryText
                    : ThemePalette.SecondaryText,
                TextFormatFlags.HorizontalCenter
                    | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.EndEllipsis);

            if (selected)
            {
                int indicatorHeight = DpiScaler.Scale(
                    UiDimensions.FocusBorderWidth,
                    _tabs.DeviceDpi);
                using var indicatorBrush = new SolidBrush(
                    ThemePalette.SelectionIndicator);
                eventArgs.Graphics.FillRectangle(
                    indicatorBrush,
                    bounds.Left,
                    bounds.Bottom - indicatorHeight,
                    bounds.Width,
                    indicatorHeight);
            }

            if (_tabs.Focused && selected)
            {
                Rectangle focusBounds = Rectangle.Inflate(
                    bounds,
                    -UiSpacing.Space4,
                    -UiSpacing.Space4);
                ControlPaint.DrawFocusRectangle(
                    eventArgs.Graphics,
                    focusBounds,
                    ThemePalette.FocusBorder,
                    selected
                        ? ThemePalette.AccentSoft
                        : ThemePalette.PanelBackground);
            }
        }
    }

    private sealed class DarkProfessionalColorTable
        : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin =>
            ThemePalette.HeaderBackground;

        public override Color ToolStripGradientMiddle =>
            ThemePalette.HeaderBackground;

        public override Color ToolStripGradientEnd =>
            ThemePalette.HeaderBackground;

        public override Color MenuItemSelected =>
            ThemePalette.InputHoverBackground;

        public override Color MenuItemBorder =>
            ThemePalette.BorderStrong;

        public override Color ImageMarginGradientBegin =>
            ThemePalette.RaisedPanel;

        public override Color ImageMarginGradientMiddle =>
            ThemePalette.RaisedPanel;

        public override Color ImageMarginGradientEnd =>
            ThemePalette.RaisedPanel;

        public override Color SeparatorDark =>
            ThemePalette.BorderSubtle;

        public override Color SeparatorLight =>
            ThemePalette.BorderSubtle;
    }
}
