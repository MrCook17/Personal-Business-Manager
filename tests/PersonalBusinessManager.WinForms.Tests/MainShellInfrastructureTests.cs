using System.ComponentModel;
using System.Reflection;
using System.Runtime.ExceptionServices;
using PersonalBusinessManager.Core.Application.Contracts;
using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Forms;
using PersonalBusinessManager.WinForms.Navigation;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Tests;

public sealed class MainShellInfrastructureTests
{
    private static readonly string[] ApprovedRoutes =
    [
        "dashboard",
        "customers",
        "jobs",
        "time",
        "tasks",
        "invoices",
        "expenses",
        "business-reports",
        "accounts",
        "applications",
        "personal-reports",
        "audit-history",
        "backups",
        "settings",
    ];

    [Fact]
    public void EveryApprovedSidebarDestinationNavigatesAndUpdatesContext()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));

            Assert.Equal(ApprovedRoutes, shell.NavigationKeys);

            foreach (string route in ApprovedRoutes)
            {
                Assert.True(shell.NavigateAsync(route)
                    .GetAwaiter()
                    .GetResult());
                Application.DoEvents();

                Assert.Equal(route, shell.ActiveRouteKey);
                Assert.NotNull(shell.ActivePage);
                Assert.False(string.IsNullOrWhiteSpace(
                    shell.CurrentPageTitle));
                Assert.False(string.IsNullOrWhiteSpace(
                    shell.CurrentBreadcrumb));
                Assert.Single(
                    Descendants(shell)
                        .OfType<DarkButton>(),
                    button =>
                        button.IsNavigationItem
                        && button.IsSelected);
            }
        });
    }

    [Fact]
    public void SidebarSupportsManualAndResponsiveCompactStates()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));

            shell.SetSidebarCollapsed(false);
            Application.DoEvents();
            Assert.False(shell.IsSidebarCollapsed);
            Assert.Equal(
                UiDimensions.ExpandedSidebarWidth,
                shell.SidebarWidth);
            Assert.Equal(
                UiSpacing.Space24,
                shell.ContentHorizontalPadding);

            shell.SetSidebarCollapsed(true);
            Application.DoEvents();
            Assert.True(shell.IsSidebarCollapsed);
            Assert.Equal(
                UiDimensions.CollapsedSidebarWidth,
                shell.SidebarWidth);
            Assert.All(
                Descendants(shell)
                    .OfType<DarkButton>()
                    .Where(button => button.IsNavigationItem),
                button =>
                {
                    Assert.True(button.IsCompactNavigation);
                    Assert.Equal(
                        ContentAlignment.MiddleCenter,
                        button.TextAlign);
                    Assert.False(string.IsNullOrWhiteSpace(
                        button.AccessibleName));
                });

            shell.SetSidebarCollapsed(false);
            shell.Size = new Size(
                UiDimensions.MinimumWindowWidth,
                UiDimensions.MinimumWindowHeight);
            Application.DoEvents();

            Assert.True(shell.IsResponsiveCollapseRequired);
            Assert.True(shell.IsSidebarCollapsed);
            Assert.Equal(
                UiSpacing.Space16,
                shell.ContentHorizontalPadding);
        });
    }

    [Fact]
    public void UserAndBackupHostsExposeExplicitPhaseReadyStates()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));

            Assert.False(shell.CurrentUserMenu.SessionAvailable);
            Assert.False(shell.CurrentUserMenu.SessionActionsEnabled);

            shell.CurrentUserMenu.UserDisplayName = "Charlie Cook";
            shell.CurrentUserMenu.SessionAvailable = true;
            Assert.True(shell.CurrentUserMenu.SessionActionsEnabled);

            var successfulAt = new DateTimeOffset(
                2026,
                8,
                3,
                9,
                30,
                0,
                TimeSpan.Zero);
            shell.UpdateBackupStatus(
                new BackupStatusSnapshot(
                    BackupHealthState.Healthy,
                    "Completed",
                    successfulAt));

            Assert.Equal(
                BackupHealthState.Healthy,
                shell.BackupStatus.State);
            BackupStatusIndicator indicator = Descendants(shell)
                .OfType<BackupStatusIndicator>()
                .Single();
            Assert.Contains("Completed", indicator.Text);
            Assert.Contains(
                "Last successful backup",
                indicator.AccessibleDescription);
        });
    }

    [Fact]
    public void NotificationsAreNonBlockingExplicitAndDismissible()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));
            int actionCount = 0;

            Guid success = shell.ShowNotification(
                new ShellNotification(
                    "Customer saved successfully.",
                    ShellNotificationSeverity.Success));
            Guid warning = shell.ShowNotification(
                new ShellNotification(
                    "Backup destination requires attention.",
                    ShellNotificationSeverity.Warning,
                    "Review",
                    () => actionCount++));
            Application.DoEvents();

            Assert.NotEqual(Guid.Empty, success);
            Assert.NotEqual(Guid.Empty, warning);
            Assert.Equal(2, shell.ActiveNotificationCount);
            Assert.True(shell.IsTimerStripVisible);
            Assert.All(
                Descendants(shell)
                    .OfType<DarkButton>()
                    .Where(button => button.IsNavigationItem),
                button => Assert.True(button.Enabled));

            Assert.True(shell.DismissNotification(success));
            Assert.Equal(1, shell.ActiveNotificationCount);
            Assert.True(shell.DismissNotification(warning));
            Assert.Equal(0, shell.ActiveNotificationCount);
            Assert.Equal(0, actionCount);
        });
    }

    [Fact]
    public void NavigationPreservesSupportedStateAndDisposesOutgoingPage()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));
            var statefulPages = new List<StatefulTrackingPage>();

            shell.RegisterPageDefinition(
                new ShellPageDefinition(
                    "stateful",
                    "Stateful page",
                    "Tests / Stateful",
                    _ =>
                    {
                        var page = new StatefulTrackingPage();
                        statefulPages.Add(page);
                        return ValueTask.FromResult<UserControl>(page);
                    }));
            shell.RegisterPageDefinition(
                ShellPageDefinition.FromSynchronousFactory(
                    "other",
                    "Other page",
                    "Tests / Other",
                    static () => new TrackingPage()));

            Assert.True(shell.NavigateAsync("stateful")
                .GetAwaiter()
                .GetResult());
            StatefulTrackingPage first = statefulPages.Single();
            first.State = "filter=active;page=3";

            Assert.True(shell.NavigateAsync("other")
                .GetAwaiter()
                .GetResult());
            Assert.True(first.WasDisposed);

            Assert.True(shell.NavigateAsync("stateful")
                .GetAwaiter()
                .GetResult());
            StatefulTrackingPage restored = statefulPages.Last();
            Assert.NotSame(first, restored);
            Assert.Equal("filter=active;page=3", restored.State);
        });
    }

    [Fact]
    public void NavigationGuardCanKeepUnsavedPageAlive()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));
            var guarded = new GuardedTrackingPage
            {
                AllowNavigation = false,
            };
            shell.RegisterPageDefinition(
                new ShellPageDefinition(
                    "guarded",
                    "Guarded page",
                    "Tests / Guarded",
                    _ => ValueTask.FromResult<UserControl>(guarded)));

            Assert.True(shell.NavigateAsync("guarded")
                .GetAwaiter()
                .GetResult());
            Assert.False(shell.NavigateAsync("customers")
                .GetAwaiter()
                .GetResult());

            Assert.Same(guarded, shell.ActivePage);
            Assert.False(guarded.WasDisposed);
            Assert.Equal(1, shell.ActiveNotificationCount);

            guarded.AllowNavigation = true;
            Assert.True(shell.NavigateAsync("customers")
                .GetAwaiter()
                .GetResult());
            Assert.True(guarded.WasDisposed);
        });
    }

    [Fact]
    public void CancellableLoadingBlocksOnlyTheContentRegion()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));
            var started = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            shell.RegisterPageDefinition(
                new ShellPageDefinition(
                    "slow",
                    "Slow page",
                    "Tests / Slow",
                    async cancellationToken =>
                    {
                        started.TrySetResult(true);
                        await Task.Delay(
                            Timeout.InfiniteTimeSpan,
                            cancellationToken);
                        return new TrackingPage();
                    }));

            Task<bool> navigation = shell.NavigateAsync("slow");
            PumpUntil(() => started.Task.IsCompleted);

            Assert.True(shell.IsPageLoading);
            Assert.True(shell.IsTimerStripVisible);
            Assert.All(
                Descendants(shell)
                    .OfType<DarkButton>()
                    .Where(button => button.IsNavigationItem),
                button => Assert.True(button.Enabled));
            LoadingOverlay overlay = Descendants(shell)
                .OfType<LoadingOverlay>()
                .Single();
            Assert.True(overlay.Visible);
            Assert.DoesNotContain(
                overlay,
                Ancestors(Descendants(shell)
                    .First(control =>
                        string.Equals(
                            control.AccessibleName,
                            "Persistent timer strip",
                            StringComparison.Ordinal))));

            shell.CancelPageLoading();
            PumpUntil(() => navigation.IsCompleted);
            Assert.False(navigation.GetAwaiter().GetResult());
            Assert.False(shell.IsPageLoading);
            Assert.Equal("dashboard", shell.ActiveRouteKey);
            Assert.True(shell.IsTimerStripVisible);
        });
    }

    [Fact]
    public void NewNavigationCancelsAnObsoletePageLoad()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));
            var started = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            shell.RegisterPageDefinition(
                new ShellPageDefinition(
                    "obsolete",
                    "Obsolete page",
                    "Tests / Obsolete",
                    async cancellationToken =>
                    {
                        started.TrySetResult(true);
                        await Task.Delay(
                            Timeout.InfiniteTimeSpan,
                            cancellationToken);
                        return new TrackingPage();
                    }));
            shell.RegisterPageDefinition(
                ShellPageDefinition.FromSynchronousFactory(
                    "replacement",
                    "Replacement page",
                    "Tests / Replacement",
                    static () => new TrackingPage()));

            Task<bool> obsoleteNavigation =
                shell.NavigateAsync("obsolete");
            PumpUntil(() => started.Task.IsCompleted);
            Assert.True(shell.IsPageLoading);

            Task<bool> replacementNavigation =
                shell.NavigateAsync("replacement");
            PumpUntil(() =>
                replacementNavigation.IsCompleted
                && obsoleteNavigation.IsCompleted);

            Assert.True(replacementNavigation
                .GetAwaiter()
                .GetResult());
            Assert.False(obsoleteNavigation
                .GetAwaiter()
                .GetResult());
            Assert.Equal("replacement", shell.ActiveRouteKey);
            Assert.False(shell.IsPageLoading);
        });
    }

    [Fact]
    public void PageLoadFailureUsesRetryStateWithoutRawExceptionText()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));
            shell.RegisterPageDefinition(
                new ShellPageDefinition(
                    "failure",
                    "Failure page",
                    "Tests / Failure",
                    _ => ValueTask.FromException<UserControl>(
                        new InvalidOperationException(
                            "SECRET RAW STACK DETAIL"))));

            Assert.False(shell.NavigateAsync("failure")
                .GetAwaiter()
                .GetResult());
            Application.DoEvents();

            Assert.Equal("failure", shell.ActiveRouteKey);
            Assert.IsType<EmptyStatePanel>(shell.ActivePage);
            Assert.Equal(1, shell.ActiveNotificationCount);
            string visibleText = string.Join(
                " ",
                Descendants(shell)
                    .Select(control => control.Text));
            Assert.DoesNotContain(
                "SECRET RAW STACK DETAIL",
                visibleText,
                StringComparison.Ordinal);
            Assert.Contains("Retry", visibleText);
        });
    }

    [Fact]
    public void ReapplyingThemeDoesNotDuplicateNavigationHandlers()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));
            int factoryCalls = 0;
            int completedCalls = 0;
            shell.RegisterPageDefinition(
                new ShellPageDefinition(
                    "customers",
                    "Customers",
                    "Work / Customers",
                    _ =>
                    {
                        factoryCalls++;
                        return ValueTask.FromResult<UserControl>(
                            new TrackingPage());
                    }));
            shell.NavigationCompleted += (_, _) => completedCalls++;

            ThemeManager.Apply(shell);
            ThemeManager.Apply(shell);
            DarkButton customerButton = Descendants(shell)
                .OfType<DarkButton>()
                .Single(button =>
                    string.Equals(
                        button.Tag as string,
                        "customers",
                        StringComparison.Ordinal));
            customerButton.PerformClick();
            Application.DoEvents();

            Assert.Equal(1, factoryCalls);
            Assert.Equal(1, completedCalls);
        });
    }

    [Fact]
    public void CollapsedNavigationRetainsKeyboardArrowMovement()
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(1440, 900));
            shell.SetSidebarCollapsed(true);
            Application.DoEvents();
            DarkButton[] buttons = Descendants(shell)
                .OfType<DarkButton>()
                .Where(button => button.IsNavigationItem)
                .ToArray();
            Assert.True(buttons[0].Focus());

            MethodInfo onKeyDown = typeof(Control).GetMethod(
                "OnKeyDown",
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException(
                    "Could not invoke keyboard navigation.");
            _ = onKeyDown.Invoke(
                buttons[0],
                [new KeyEventArgs(Keys.Down)]);

            Assert.True(buttons[1].Focused);
            Assert.True(buttons[1].TabStop);
            Assert.False(string.IsNullOrWhiteSpace(
                buttons[1].AccessibleDescription));
        });
    }

    [Theory]
    [InlineData(96, false, "expanded")]
    [InlineData(120, false, "expanded")]
    [InlineData(144, false, "expanded")]
    [InlineData(96, true, "compact")]
    [InlineData(120, true, "compact")]
    [InlineData(144, true, "compact")]
    public void ShellStatesRenderAtApprovedDpi(
        int deviceDpi,
        bool collapsed,
        string stateName)
    {
        RunInSta(() =>
        {
            Size logicalSize = collapsed
                ? new Size(
                    UiDimensions.MinimumWindowWidth,
                    UiDimensions.MinimumWindowHeight)
                : new Size(1440, 900);
            using MainShellForm shell = CreateShell(
                logicalSize,
                show: false);
            shell.SetSidebarCollapsed(collapsed);
            shell.UpdateBackupStatus(
                new BackupStatusSnapshot(
                    BackupHealthState.Healthy,
                    "Healthy"));
            _ = shell.ShowNotification(
                new ShellNotification(
                    "Shell notification remains non-blocking.",
                    ShellNotificationSeverity.Information));
            ScaleForTest(shell, deviceDpi);
            shell.Show();
            Application.DoEvents();
            shell.PerformLayout();

            Assert.Equal(collapsed, shell.IsSidebarCollapsed);
            Assert.True(shell.IsTimerStripVisible);
            Assert.Empty(ThemeManager.FindUnthemedControls(shell));
            CaptureIfRequested(
                shell,
                $"main-shell-{stateName}-{deviceDpi}dpi.png");
        });
    }

    [Theory]
    [InlineData(96)]
    [InlineData(120)]
    [InlineData(144)]
    public void ContentLoadingRendersWithoutCoveringShellControls(
        int deviceDpi)
    {
        RunInSta(() =>
        {
            using MainShellForm shell = CreateShell(
                new Size(
                    UiDimensions.MinimumWindowWidth,
                    UiDimensions.MinimumWindowHeight),
                show: false);
            shell.SetSidebarCollapsed(true);
            var started = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            shell.RegisterPageDefinition(
                new ShellPageDefinition(
                    "render-loading",
                    "Preview page",
                    "Tests / Loading preview",
                    async cancellationToken =>
                    {
                        started.TrySetResult(true);
                        await Task.Delay(
                            Timeout.InfiniteTimeSpan,
                            cancellationToken);
                        return new TrackingPage();
                    }));
            ScaleForTest(shell, deviceDpi);
            shell.Show();
            Application.DoEvents();

            Task<bool> navigation =
                shell.NavigateAsync("render-loading");
            PumpUntil(() => started.Task.IsCompleted);
            shell.PerformLayout();

            Assert.True(shell.IsPageLoading);
            Assert.True(shell.IsTimerStripVisible);
            Assert.All(
                Descendants(shell)
                    .OfType<DarkButton>()
                    .Where(button => button.IsNavigationItem),
                button => Assert.True(button.Enabled));
            CaptureIfRequested(
                shell,
                $"main-shell-loading-{deviceDpi}dpi.png");

            shell.CancelPageLoading();
            PumpUntil(() => navigation.IsCompleted);
            Assert.False(navigation.GetAwaiter().GetResult());
        });
    }

    private static MainShellForm CreateShell(
        Size size,
        bool show = true)
    {
        var shell = new MainShellForm(
            new AvailableDatabaseHealthService())
        {
            WindowState = FormWindowState.Normal,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32_000, -32_000),
            ShowInTaskbar = false,
            Size = size,
        };

        if (show)
        {
            shell.Show();
            Application.DoEvents();
            shell.PerformLayout();
        }

        return shell;
    }

    private static IEnumerable<Control> Descendants(Control root)
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

    private static IEnumerable<Control> Ancestors(Control control)
    {
        for (Control? parent = control.Parent;
            parent is not null;
            parent = parent.Parent)
        {
            yield return parent;
        }
    }

    private static void PumpUntil(
        Func<bool> condition,
        int timeoutMilliseconds = 5000)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(
            timeoutMilliseconds);

        while (!condition())
        {
            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException(
                    "The UI operation did not finish in time.");
            }

            Application.DoEvents();
            Thread.Sleep(1);
        }

        Application.DoEvents();
    }

    private static void ScaleForTest(Form form, int deviceDpi)
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
                "PBM_SHELL_CAPTURE_DIR",
                EnvironmentVariableTarget.Process);

        if (string.IsNullOrWhiteSpace(captureDirectory))
        {
            return;
        }

        Directory.CreateDirectory(captureDirectory);
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(
            bitmap,
            new Rectangle(Point.Empty, bitmap.Size));
        bitmap.Save(Path.Combine(captureDirectory, fileName));
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
                new DatabaseHealthResult(true, "Available"));
        }
    }

    private class TrackingPage : UserControl
    {
        public bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WasDisposed = true;
            }

            base.Dispose(disposing);
        }
    }

    private sealed class StatefulTrackingPage
        : TrackingPage, IShellNavigationStatefulPage
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public string State { get; set; } = string.Empty;

        public object? CaptureNavigationState()
        {
            return State;
        }

        public void RestoreNavigationState(object? state)
        {
            State = state as string ?? string.Empty;
        }
    }

    private sealed class GuardedTrackingPage
        : TrackingPage, IShellNavigationGuard
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public bool AllowNavigation { get; set; }

        public ValueTask<bool> CanNavigateAwayAsync(
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(AllowNavigation);
        }
    }
}
