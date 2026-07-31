using System.Reflection;
using System.Runtime.ExceptionServices;
using PersonalBusinessManager.Core.Application.Contracts;
using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Forms;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Tests;

public sealed class ThemeInfrastructureTests
{
    private static readonly int[] ApprovedSpacing =
        [4, 8, 16, 24, 32];

    private static readonly int[] ImplementedSpacing =
    [
        UiSpacing.Space4,
        UiSpacing.Space8,
        UiSpacing.Space16,
        UiSpacing.Space24,
        UiSpacing.Space32,
    ];

    [Fact]
    public void PaletteMatchesTheApprovedDarkThemeTokens()
    {
        Assert.Equal("#111318", ToHex(ThemePalette.ApplicationBackground));
        Assert.Equal("#171A20", ToHex(ThemePalette.SidebarBackground));
        Assert.Equal("#1D2128", ToHex(ThemePalette.PanelBackground));
        Assert.Equal("#242932", ToHex(ThemePalette.RaisedPanel));
        Assert.Equal("#191D23", ToHex(ThemePalette.InputBackground));
        Assert.Equal("#F1F3F5", ToHex(ThemePalette.PrimaryText));
        Assert.Equal("#AAB1BB", ToHex(ThemePalette.SecondaryText));
        Assert.Equal("#8B94A3", ToHex(ThemePalette.MutedText));
        Assert.Equal("#7C6CF2", ToHex(ThemePalette.Accent));
        Assert.Equal("#302B55", ToHex(ThemePalette.AccentSoft));
        Assert.Equal("#A99FFF", ToHex(ThemePalette.FocusBorder));
        Assert.Equal("#46B981", ToHex(ThemePalette.Success));
        Assert.Equal("#D6A64A", ToHex(ThemePalette.Warning));
        Assert.Equal("#DC5C68", ToHex(ThemePalette.Danger));
        Assert.Equal("#5C9DED", ToHex(ThemePalette.Information));
    }

    [Fact]
    public void SpacingAndDimensionsMatchTheApprovedLogicalPixels()
    {
        Assert.Equal(
            ApprovedSpacing,
            ImplementedSpacing);
        Assert.Equal(36, UiDimensions.StandardControlHeight);
        Assert.Equal(40, UiDimensions.GridHeaderHeight);
        Assert.Equal(36, UiDimensions.GridRowHeight);
        Assert.Equal(224, UiDimensions.ExpandedSidebarWidth);
        Assert.Equal(64, UiDimensions.HeaderHeight);
        Assert.Equal(48, UiDimensions.TimerStripHeight);
        Assert.Equal(1100, UiDimensions.MinimumWindowWidth);
        Assert.Equal(700, UiDimensions.MinimumWindowHeight);
    }

    [Theory]
    [InlineData(96, 36, 36)]
    [InlineData(120, 36, 45)]
    [InlineData(144, 36, 54)]
    [InlineData(120, 2, 3)]
    [InlineData(144, 3, 5)]
    public void DpiScalerProducesExpectedCommonScaleValues(
        int deviceDpi,
        int logicalPixels,
        int expectedPixels)
    {
        Assert.Equal(
            expectedPixels,
            DpiScaler.Scale(logicalPixels, deviceDpi));
    }

    [Fact]
    public void ThemeManagerStylesDefaultDesignerControlsAndDisabledState()
    {
        RunInSta(() =>
        {
            using var form = new Form();
            using var panel = new Panel();
            using var label = new Label();
            using var button = new Button();
            using var input = new TextBox();
            using var semanticLabel = new Label
            {
                ForeColor = ThemePalette.DangerText,
            };

            panel.Controls.Add(label);
            panel.Controls.Add(button);
            panel.Controls.Add(input);
            panel.Controls.Add(semanticLabel);
            form.Controls.Add(panel);
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-32_000, -32_000);
            form.ShowInTaskbar = false;

            ThemeManager.Apply(form);
            ThemeManager.Apply(form);
            form.Show();
            Assert.True(button.Focus());
            Application.DoEvents();

            Assert.Equal(
                ThemePalette.FocusBorder,
                button.FlatAppearance.BorderColor);
            Assert.Equal(
                DpiScaler.Scale(
                    UiDimensions.FocusBorderWidth,
                    button.DeviceDpi),
                button.FlatAppearance.BorderSize);
            Assert.Equal(
                ThemePalette.InputBackground,
                input.BackColor);

            button.Enabled = false;
            input.Enabled = false;

            Assert.Equal(
                ThemePalette.ApplicationBackground,
                form.BackColor);
            Assert.Equal(
                ThemePalette.PanelBackground,
                panel.BackColor);
            Assert.Equal(
                ThemePalette.PrimaryText,
                label.ForeColor);
            Assert.Equal(
                ThemePalette.InputDisabledBackground,
                button.BackColor);
            Assert.Equal(
                ThemePalette.DisabledText,
                button.ForeColor);
            Assert.Equal(
                ThemePalette.InputDisabledBackground,
                input.BackColor);
            Assert.Equal(
                ThemePalette.DisabledText,
                input.ForeColor);
            Assert.False(input.TabStop);
            Assert.Equal(
                ThemePalette.DangerText,
                semanticLabel.ForeColor);

            form.Hide();
        });
    }

    [Fact]
    public void DataGridStylingUsesDarkRowsFocusSelectionAndDoubleBuffering()
    {
        RunInSta(() =>
        {
            using var grid = new DataGridView();

            ControlStyler.StyleDataGridView(grid);

            Assert.False(grid.EnableHeadersVisualStyles);
            Assert.Equal(
                UiDimensions.GridHeaderHeight,
                grid.ColumnHeadersHeight);
            Assert.Equal(
                UiDimensions.GridRowHeight,
                grid.RowTemplate.Height);
            Assert.Equal(
                DataGridViewSelectionMode.FullRowSelect,
                grid.SelectionMode);
            Assert.Equal(
                ThemePalette.GridSelectedRow,
                grid.DefaultCellStyle.SelectionBackColor);
            Assert.Equal(
                ThemePalette.GridAlternateRow,
                grid.AlternatingRowsDefaultCellStyle.BackColor);

            PropertyInfo doubleBuffered =
                typeof(Control).GetProperty(
                    "DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException(
                    "Could not inspect the grid buffering property.");

            Assert.True((bool)doubleBuffered.GetValue(grid)!);
        });
    }

    [Fact]
    public void TabStylingUsesOwnerDrawingAndApprovedDimensions()
    {
        RunInSta(() =>
        {
            using var tabs = new TabControl();
            using var page = new TabPage("Details");
            tabs.TabPages.Add(page);

            ControlStyler.StyleTabControl(tabs);

            Assert.Equal(TabDrawMode.OwnerDrawFixed, tabs.DrawMode);
            Assert.Equal(
                UiDimensions.TabHeaderHeight,
                tabs.ItemSize.Height);
            Assert.Equal(
                ThemePalette.PanelBackground,
                page.BackColor);
            Assert.Equal(
                ThemePalette.PrimaryText,
                page.ForeColor);
        });
    }

    [Theory]
    [InlineData(96)]
    [InlineData(120)]
    [InlineData(144)]
    public void MainShellRendersWithoutUnthemedControlsAtApprovedDpi(
        int deviceDpi)
    {
        RunInSta(() =>
        {
            using var form = new MainShellForm(
                new AvailableDatabaseHealthService());
            form.WindowState = FormWindowState.Normal;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-32_000, -32_000);
            form.ShowInTaskbar = false;
            form.Size = new Size(
                UiDimensions.MinimumWindowWidth,
                UiDimensions.MinimumWindowHeight);

            if (deviceDpi != DpiScaler.BaselineDpi)
            {
                float scale = deviceDpi
                    / (float)DpiScaler.BaselineDpi;
                form.Scale(new SizeF(scale, scale));
            }

            form.Show();
            Application.DoEvents();
            form.PerformLayout();
            Assert.Equal(AutoScaleMode.Dpi, form.AutoScaleMode);
            Assert.True(form.ClientSize.Width > 0);
            Assert.True(form.ClientSize.Height > 0);
            Assert.Empty(
                ThemeManager.FindUnthemedControls(form));

            DarkButton navigationButton = Descendants(form)
                .OfType<DarkButton>()
                .First();
            Assert.Equal(
                DpiScaler.Scale(
                    UiDimensions.SidebarNavigationHeight,
                    deviceDpi),
                navigationButton.Height);

            using var bitmap = new Bitmap(
                form.ClientSize.Width,
                form.ClientSize.Height);
            form.DrawToBitmap(
                bitmap,
                new Rectangle(Point.Empty, bitmap.Size));

            form.Hide();

            string? captureDirectory =
                Environment.GetEnvironmentVariable(
                    "PBM_THEME_CAPTURE_DIR",
                    EnvironmentVariableTarget.Process);

            if (!string.IsNullOrWhiteSpace(captureDirectory))
            {
                Directory.CreateDirectory(captureDirectory);
                bitmap.Save(
                    Path.Combine(
                        captureDirectory,
                        $"main-shell-{deviceDpi}dpi.png"));
            }
        });
    }

    private static string ToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static IEnumerable<Control> Descendants(
        Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;

            foreach (Control descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static void RunInSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private sealed class AvailableDatabaseHealthService
        : IDatabaseHealthService
    {
        public Task<DatabaseHealthResult> CheckAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                new DatabaseHealthResult(
                    true,
                    "Theme verification"));
        }
    }
}
