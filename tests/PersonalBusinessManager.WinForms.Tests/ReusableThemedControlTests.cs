using System.ComponentModel;
using System.Reflection;
using System.Runtime.ExceptionServices;
using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Dialogs;
using PersonalBusinessManager.WinForms.Forms;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Tests;

public sealed class ReusableThemedControlTests
{
    private static readonly Type[] RequiredControlTypes =
    [
        typeof(DarkButton),
        typeof(DarkTextBox),
        typeof(DarkComboBox),
        typeof(DarkDateTimePicker),
        typeof(DarkDataGridView),
        typeof(DarkTabControl),
        typeof(PageHeader),
        typeof(FilterBar),
        typeof(SummaryCard),
        typeof(StatusBadge),
        typeof(EmptyStatePanel),
        typeof(LoadingOverlay),
        typeof(ValidationMessage),
        typeof(ConfirmDialog),
    ];

    [Fact]
    public void EveryRequiredControlIsThemeAwareAndDesignerConstructible()
    {
        RunInSta(() =>
        {
            foreach (Type controlType in RequiredControlTypes)
            {
                Assert.True(
                    typeof(IThemeAwareControl).IsAssignableFrom(controlType),
                    $"{controlType.Name} must participate in shared theming.");

                object instance = Activator.CreateInstance(controlType)
                    ?? throw new InvalidOperationException(
                        $"Could not construct {controlType.Name}.");

                Assert.NotEmpty(TypeDescriptor.GetProperties(instance));

                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        });
    }

    [Fact]
    public void DarkTextBoxUsesApprovedValidationFocusReadOnlyAndDisabledStates()
    {
        RunInSta(() =>
        {
            using var form = CreateOffscreenForm();
            using var input = new DarkTextBox
            {
                Text = "Entered value",
                HasValidationError = true,
            };
            using var nextButton = new DarkButton
            {
                Text = "Next",
            };
            form.Controls.Add(nextButton);
            form.Controls.Add(input);
            form.Show();
            Application.DoEvents();

            Assert.Equal(
                ThemePalette.DangerBorder,
                input.CurrentBorderColor);
            Assert.True(input.EditorTextBox.Focus());
            Application.DoEvents();
            Assert.Equal(
                ThemePalette.FocusBorder,
                input.CurrentBorderColor);
            Assert.Equal(
                UiDimensions.FocusBorderWidth,
                input.CurrentBorderWidth);

            Assert.True(nextButton.Focus());
            input.HasValidationError = false;
            input.ReadOnly = true;
            Assert.Equal(ThemePalette.PanelBackground, input.BackColor);
            Assert.Equal(ThemePalette.SecondaryText, input.ForeColor);
            Assert.True(input.EditorTextBox.TabStop);

            input.Enabled = false;
            Assert.Equal(
                ThemePalette.InputDisabledBackground,
                input.BackColor);
            Assert.Equal(ThemePalette.DisabledText, input.ForeColor);
            Assert.False(input.EditorTextBox.TabStop);
        });
    }

    [Fact]
    public void DarkComboBoxOwnerDrawsItsDarkDropDownAndRetainsTextStatus()
    {
        RunInSta(() =>
        {
            using var combo = new DarkComboBox();
            combo.Items.AddRange(["Active", "Archived", "All"]);
            combo.SelectedIndex = 0;

            Assert.Equal(DrawMode.OwnerDrawFixed, combo.EditorComboBox.DrawMode);
            Assert.Equal(
                ComboBoxStyle.DropDownList,
                combo.EditorComboBox.DropDownStyle);
            Assert.Equal("Active", combo.SelectedItem);
            Assert.Equal(
                ThemePalette.InputBackground,
                combo.EditorComboBox.BackColor);

            combo.Enabled = false;
            Assert.Equal(
                ThemePalette.InputDisabledBackground,
                combo.BackColor);
            Assert.Equal(
                ThemePalette.DisabledText,
                combo.EditorComboBox.ForeColor);
        });
    }

    [Fact]
    public void DarkDateTimePickerUsesApprovedBritishDatePresentation()
    {
        RunInSta(() =>
        {
            using var form = CreateOffscreenForm();
            using var picker = new DarkDateTimePicker
            {
                Value = new DateTime(2026, 12, 31),
            };
            form.Controls.Add(picker);
            form.Show();
            Assert.True(picker.Focus());
            Application.DoEvents();

            Assert.Equal(DateTimePickerFormat.Custom, picker.Format);
            Assert.Equal("dd/MM/yyyy", picker.CustomFormat);
            Assert.Equal("31/12/2026", picker.GetApprovedDisplayText());
            Assert.Equal(
                ThemePalette.RaisedPanel,
                picker.EditorDateTimePicker.CalendarMonthBackground);
            Assert.Equal(
                ThemePalette.FocusBorder,
                picker.CurrentBorderColor);
        });
    }

    [Fact]
    public void DarkButtonSupportsGeneralVariantsAndNavigationSelection()
    {
        RunInSta(() =>
        {
            using var primary = new DarkButton
            {
                Variant = ButtonVariant.Primary,
            };
            using var danger = new DarkButton
            {
                Variant = ButtonVariant.Danger,
            };
            using var navigation = new DarkButton
            {
                IsNavigationItem = true,
                IsSelected = true,
            };

            Assert.Equal(ThemePalette.Accent, primary.BackColor);
            Assert.Equal(ThemePalette.InverseText, primary.ForeColor);
            Assert.Equal(ThemePalette.Danger, danger.BackColor);
            Assert.Equal(ThemePalette.AccentSoft, navigation.BackColor);
            Assert.Equal(
                UiDimensions.SidebarNavigationHeight,
                navigation.Height);
            Assert.Equal(
                ContentAlignment.MiddleLeft,
                navigation.TextAlign);
        });
    }

    [Fact]
    public void TypedGridAndTabsApplyDarkBufferedOwnerDrawnStyles()
    {
        RunInSta(() =>
        {
            using var grid = new DarkDataGridView();
            using var tabs = new DarkTabControl();
            tabs.TabPages.Add("Overview");

            PropertyInfo doubleBuffered = typeof(Control).GetProperty(
                "DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException(
                    "Could not inspect double buffering.");

            Assert.True((bool)doubleBuffered.GetValue(grid)!);
            Assert.Equal(
                ThemePalette.GridSelectedRow,
                grid.DefaultCellStyle.SelectionBackColor);
            Assert.Equal(TabDrawMode.OwnerDrawFixed, tabs.DrawMode);
            Assert.Equal(
                UiDimensions.TabHeaderHeight,
                tabs.ItemSize.Height);
        });
    }

    [Fact]
    public void PageHeaderAndFilterBarAcceptReusableActionsAndFilters()
    {
        RunInSta(() =>
        {
            using var header = new PageHeader
            {
                TitleText = "Customers",
                SubtitleText = "Manage active and archived customers.",
                BreadcrumbText = "Work / Customers",
            };
            using var filterBar = new FilterBar();

            header.AddAction(new DarkButton
            {
                Text = "Add customer",
                Variant = ButtonVariant.Primary,
            });
            filterBar.AddFilter(new DarkTextBox
            {
                PlaceholderText = "Search customers",
            });
            filterBar.AddFilter(new DarkComboBox());

            Assert.Single(header.Actions);
            Assert.Equal(2, filterBar.FilterControls.Count);
            Assert.Equal(
                UiDimensions.HeaderHeight,
                header.MinimumSize.Height);
            Assert.Equal(
                UiDimensions.FilterBarMinimumHeight,
                filterBar.MinimumSize.Height);
        });
    }

    [Theory]
    [InlineData(SemanticRole.Neutral)]
    [InlineData(SemanticRole.Information)]
    [InlineData(SemanticRole.Success)]
    [InlineData(SemanticRole.Warning)]
    [InlineData(SemanticRole.Danger)]
    [InlineData(SemanticRole.Accent)]
    public void StatusBadgeMapsSemanticRoleToApprovedTextAndBackground(
        SemanticRole role)
    {
        RunInSta(() =>
        {
            using var badge = new StatusBadge
            {
                Text = role.ToString(),
                SemanticRole = role,
                AccessibleDescription = $"Status: {role}",
            };
            SemanticColors colors = SemanticTheme.GetColors(role);

            Assert.Equal(colors.Background, badge.BackColor);
            Assert.Equal(colors.Text, badge.ForeColor);
            Assert.Equal(role.ToString(), badge.AccessibleName);
            Assert.False(string.IsNullOrWhiteSpace(
                badge.AccessibleDescription));
            Assert.True(
                badge.Height >= UiDimensions.StatusBadgeMinimumHeight);
        });
    }

    [Fact]
    public void EmptyLoadingErrorAndValidationStatesRemainExplicitInText()
    {
        RunInSta(() =>
        {
            using var error = new EmptyStatePanel
            {
                StateKind = ContentStateKind.Error,
                HeadingText = "Records could not be loaded",
                DescriptionText = "Try this operation again.",
                TechnicalReference = "Reference: TEST-001",
                PrimaryActionText = "Retry",
            };
            using var loading = new LoadingOverlay
            {
                MessageText = "Loading records…",
                CanCancel = true,
            };
            using var validation = new ValidationMessage
            {
                MessageKind = ValidationMessageKind.Summary,
                Text = "Review the highlighted fields.",
            };

            loading.IsActive = true;

            Assert.Equal(
                ThemePalette.DangerSoft,
                error.BackColor);
            Assert.Contains("could not", error.HeadingText);
            Assert.True(loading.Visible);
            Assert.Contains("Loading", loading.MessageText);
            Assert.Equal(
                ThemePalette.DangerSoft,
                validation.BackColor);
            Assert.Contains("highlighted", validation.Text);
            Assert.True(
                validation.MinimumSize.Height
                    >= UiDimensions.ValidationSummaryMinimumHeight);
        });
    }

    [Fact]
    public void ConfirmDialogRequiresExactTypedConfirmationForDangerousAction()
    {
        RunInSta(() =>
        {
            using var dialog = new ConfirmDialog();
            dialog.Configure(
                "Restore backup",
                "Replace current data?",
                "Current data will be replaced after a safety backup.",
                "Restore backup",
                ConfirmationSeverity.Danger,
                "RESTORE");

            Assert.Equal(ConfirmationSeverity.Danger, dialog.Severity);
            Assert.Equal("RESTORE", dialog.RequiredConfirmationText);
            Assert.False(dialog.ConfirmButton.Enabled);
            Assert.Null(dialog.AcceptButton);
            Assert.NotNull(dialog.CancelButton);

            dialog.ConfirmationInput.Text = "restore";
            Assert.False(dialog.ConfirmButton.Enabled);
            dialog.ConfirmationInput.Text = "RESTORE";
            Assert.True(dialog.ConfirmButton.Enabled);
            Assert.Equal(ButtonVariant.Danger, dialog.ConfirmButton.Variant);
            Assert.Equal(
                UiDimensions.ConfirmationDialogWidth,
                dialog.ClientSize.Width);
        });
    }

    [Fact]
    public void DevelopmentGalleryContainsEveryNonDialogRequiredControl()
    {
        RunInSta(() =>
        {
            using var gallery = new ThemeControlGalleryForm();
            Type[] galleryTypes = Descendants(gallery)
                .Select(control => control.GetType())
                .Distinct()
                .ToArray();

            foreach (Type requiredType in RequiredControlTypes
                .Where(type => type != typeof(ConfirmDialog)))
            {
                Assert.Contains(requiredType, galleryTypes);
            }

            Assert.Equal(3, gallery.GalleryPageCount);
            Assert.DoesNotContain(
                Descendants(gallery).OfType<DarkButton>(),
                button => button.IsNavigationItem
                    && string.Equals(
                        button.Text,
                        "Theme control gallery",
                        StringComparison.Ordinal));
        });
    }

    [Theory]
    [InlineData(96, 0, "inputs")]
    [InlineData(96, 1, "data")]
    [InlineData(96, 2, "states")]
    [InlineData(120, 0, "inputs")]
    [InlineData(120, 1, "data")]
    [InlineData(120, 2, "states")]
    [InlineData(144, 0, "inputs")]
    [InlineData(144, 1, "data")]
    [InlineData(144, 2, "states")]
    public void DevelopmentGalleryRendersAllPagesAtApprovedDpi(
        int deviceDpi,
        int pageIndex,
        string pageName)
    {
        RunInSta(() =>
        {
            using var gallery = new ThemeControlGalleryForm
            {
                WindowState = FormWindowState.Normal,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32_000, -32_000),
            };
            gallery.ClientSize = new Size(
                UiDimensions.MinimumWindowWidth,
                UiDimensions.MinimumWindowHeight);
            gallery.SelectedGalleryPage = pageIndex;

            ScaleForTest(gallery, deviceDpi);
            gallery.Show();
            Application.DoEvents();
            gallery.PerformLayout();

            Assert.Empty(ThemeManager.FindUnthemedControls(gallery));
            if (pageIndex == 2)
            {
                LoadingOverlay loading = Descendants(gallery)
                    .OfType<LoadingOverlay>()
                    .Single();
                Assert.True(loading.IsActive);
                Assert.True(loading.Visible);
                Assert.True(loading.Width > 0);
                Assert.True(loading.Height > 0);
                Assert.Equal(
                    0,
                    loading.Parent!.Controls.GetChildIndex(loading));
            }

            CaptureIfRequested(
                gallery,
                $"control-gallery-{pageName}-{deviceDpi}dpi.png");
        });
    }

    [Theory]
    [InlineData(96)]
    [InlineData(120)]
    [InlineData(144)]
    public void ConfirmDialogRendersAtApprovedDpi(
        int deviceDpi)
    {
        RunInSta(() =>
        {
            using var dialog = new ConfirmDialog
            {
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32_000, -32_000),
            };
            dialog.Configure(
                "Restore backup",
                "Replace current data?",
                "Current data will be replaced. A safety backup runs first.",
                "Restore backup",
                ConfirmationSeverity.Danger,
                "RESTORE");

            ScaleForTest(dialog, deviceDpi);
            dialog.Show();
            Application.DoEvents();
            dialog.PerformLayout();

            Assert.Empty(ThemeManager.FindUnthemedControls(dialog));
            CaptureIfRequested(
                dialog,
                $"confirm-dialog-{deviceDpi}dpi.png");
        });
    }

    private static Form CreateOffscreenForm()
    {
        var form = new Form
        {
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32_000, -32_000),
            ShowInTaskbar = false,
        };
        ThemeManager.Apply(form);
        return form;
    }

    private static void ScaleForTest(
        Form form,
        int deviceDpi)
    {
        if (deviceDpi == DpiScaler.BaselineDpi)
        {
            return;
        }

        float scale = deviceDpi
            / (float)DpiScaler.BaselineDpi;
        form.Scale(new SizeF(scale, scale));
    }

    private static void CaptureIfRequested(
        Form form,
        string fileName)
    {
        string? captureDirectory =
            Environment.GetEnvironmentVariable(
                "PBM_THEME_CAPTURE_DIR",
                EnvironmentVariableTarget.Process);

        if (string.IsNullOrWhiteSpace(captureDirectory))
        {
            return;
        }

        Directory.CreateDirectory(captureDirectory);
        using var bitmap = new Bitmap(
            form.Width,
            form.Height);
        form.DrawToBitmap(
            bitmap,
            new Rectangle(Point.Empty, bitmap.Size));
        bitmap.Save(Path.Combine(captureDirectory, fileName));
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
}
