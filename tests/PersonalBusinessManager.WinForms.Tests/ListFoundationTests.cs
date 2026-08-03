using System.Runtime.ExceptionServices;
using PersonalBusinessManager.Core.Application.Filters;
using PersonalBusinessManager.Core.Application.Queries;
using PersonalBusinessManager.WinForms.Controls;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Tests;

public sealed class ListFoundationTests
{
    [Fact]
    public void SearchDebounceUsesTheApprovedDelayRange()
    {
        using var coordinator = new DebouncedSearchCoordinator();

        Assert.Equal(
            TimeSpan.FromMilliseconds(300),
            coordinator.Delay);
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DebouncedSearchCoordinator(249));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DebouncedSearchCoordinator(401));
    }

    [Fact]
    public async Task NewSearchCancelsTheObsoleteRequest()
    {
        using var coordinator = new DebouncedSearchCoordinator(250);
        var firstStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        bool firstCancelled = false;
        bool secondRan = false;

        Task<bool> first = coordinator.QueueAsync(async token =>
        {
            firstStarted.SetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
            }
            catch (OperationCanceledException)
                when (token.IsCancellationRequested)
            {
                firstCancelled = true;
                throw;
            }
        });
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Task<bool> second = coordinator.QueueAsync(_ =>
        {
            secondRan = true;
            return Task.CompletedTask;
        });

        Assert.False(await first);
        Assert.True(await second);
        Assert.True(firstCancelled);
        Assert.True(secondRan);
    }

    [Fact]
    public void AsyncListLoadingReturnsWithoutFreezingTheUi()
    {
        RunInSta(() =>
        {
            using var list = CreateListView();
            var completion = new TaskCompletionSource<
                PagedResult<DemoListItem>>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            Task<bool> load = list.LoadAsync(
                _ => completion.Task,
                "Loading demo records…");

            Assert.False(load.IsCompleted);
            Assert.True(list.IsLoading);
            list.AccessibleName = "UI remained responsive";
            Assert.Equal("UI remained responsive", list.AccessibleName);

            completion.SetResult(CreateResult());
            WaitForTask(load);

            Assert.True(load.Result);
            Assert.Equal(PagedListState.Ready, list.State);
            Assert.False(list.IsLoading);
            SynchronizationContext.SetSynchronizationContext(null);
        });
    }

    [Fact]
    public void NewListLoadCancelsTheObsoleteRequest()
    {
        RunInSta(() =>
        {
            using var list = CreateListView();
            var firstCompletion = new TaskCompletionSource<
                PagedResult<DemoListItem>>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
            CancellationToken firstToken = default;
            Task<bool> first = list.LoadAsync(token =>
            {
                firstToken = token;
                return firstCompletion.Task.WaitAsync(token);
            });
            Task<bool> second = list.LoadAsync(
                _ => Task.FromResult(CreateResult()));
            WaitForTask(Task.WhenAll(first, second));

            Assert.True(firstToken.IsCancellationRequested);
            Assert.False(first.Result);
            Assert.True(second.Result);
            Assert.Equal(PagedListState.Ready, list.State);
            SynchronizationContext.SetSynchronizationContext(null);
        });
    }

    [Fact]
    public void UserCancellationRestoresThePriorListState()
    {
        RunInSta(() =>
        {
            using var list = CreateListView();
            list.BindResult(CreateResult());
            Task<bool> load = list.LoadAsync<DemoListItem>(
                async token =>
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    return CreateResult();
                });

            Assert.Equal(PagedListState.Loading, list.State);
            list.CancelLoading();
            WaitForTask(load);

            Assert.False(load.Result);
            Assert.Equal(PagedListState.Ready, list.State);
            Assert.False(list.IsLoading);
            Assert.True(list.Grid.Visible);
            SynchronizationContext.SetSynchronizationContext(null);
        });
    }

    [Fact]
    public void FailedListLoadShowsSafeRetryStateAndRaisesDetailsForLogging()
    {
        RunInSta(() =>
        {
            using var form = CreateOffscreenForm();
            using var list = CreateListView();
            Exception? reported = null;
            int retries = 0;
            list.LoadFailed += (_, eventArgs) =>
                reported = eventArgs.Exception;
            list.RetryRequested += (_, _) => retries++;
            form.Controls.Add(list);
            form.Show();
            Application.DoEvents();

            Task<bool> load = list.LoadAsync<DemoListItem>(
                _ => throw new InvalidOperationException(
                    "sensitive database detail"),
                errorDescription: "Check the connection and try again.",
                technicalReference: "Reference: LIST-001");
            WaitForTask(load);
            EmptyStatePanel state = Descendants(list)
                .OfType<EmptyStatePanel>()
                .Single();

            Assert.False(load.Result);
            Assert.Equal(PagedListState.Error, list.State);
            Assert.IsType<InvalidOperationException>(reported);
            Assert.DoesNotContain(
                "sensitive database detail",
                state.DescriptionText,
                StringComparison.Ordinal);
            Assert.Equal("Reference: LIST-001", state.TechnicalReference);
            Descendants(state)
                .OfType<DarkButton>()
                .Single(button => button.Text == "Retry")
                .PerformClick();
            Assert.Equal(1, retries);
        });
    }

    [Fact]
    public void PagingControlPresentsRangeAndRequestsAdjacentPage()
    {
        RunInSta(() =>
        {
            using var paging = new PagingControl();
            PagingRequest? requested = null;
            paging.PageRequested += (_, eventArgs) =>
                requested = eventArgs.Request;
            paging.ApplyResult(CreateResult(pageNumber: 2));

            Assert.Equal("51–100 of 243", paging.RangeText);
            Assert.Equal("Page 2", paging.PageText);
            Assert.True(paging.PreviousPageEnabled);
            Assert.True(paging.NextPageEnabled);

            Descendants(paging)
                .OfType<DarkButton>()
                .Single(button => button.Text == "Next")
                .PerformClick();

            Assert.Equal(new PagingRequest(3, 50), requested);
        });
    }

    [Fact]
    public void PageSizeChangeRequestsFirstPageWithValidatedSize()
    {
        RunInSta(() =>
        {
            using var paging = new PagingControl();
            paging.ApplyResult(CreateResult(pageNumber: 3));
            PagingRequest? requested = null;
            paging.PageRequested += (_, eventArgs) =>
                requested = eventArgs.Request;
            DarkComboBox pageSize = Descendants(paging)
                .OfType<DarkComboBox>()
                .Single();

            pageSize.SelectedItem = 100;

            Assert.Equal(new PagingRequest(1, 100), requested);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => paging.SetAllowedPageSizes([50, 250]));
        });
    }

    [Fact]
    public void PagedListUsesReadOnlyBufferedOrdinaryBindingDefaults()
    {
        RunInSta(() =>
        {
            using var list = CreateListView();

            Assert.True(list.Grid.ReadOnly);
            Assert.False(list.Grid.AllowUserToAddRows);
            Assert.False(list.Grid.AllowUserToDeleteRows);
            Assert.False(list.Grid.AutoGenerateColumns);
            Assert.False(list.Grid.VirtualMode);
            Assert.Equal(AccessibleRole.Table, list.Grid.AccessibleRole);
        });
    }

    [Theory]
    [InlineData(96, PagedListState.Ready, "ready")]
    [InlineData(96, PagedListState.Loading, "loading")]
    [InlineData(96, PagedListState.Empty, "empty")]
    [InlineData(96, PagedListState.Error, "error")]
    [InlineData(120, PagedListState.Ready, "ready")]
    [InlineData(120, PagedListState.Loading, "loading")]
    [InlineData(120, PagedListState.Empty, "empty")]
    [InlineData(120, PagedListState.Error, "error")]
    [InlineData(144, PagedListState.Ready, "ready")]
    [InlineData(144, PagedListState.Loading, "loading")]
    [InlineData(144, PagedListState.Empty, "empty")]
    [InlineData(144, PagedListState.Error, "error")]
    public void ListFoundationRendersApprovedStatesAtApprovedDpi(
        int deviceDpi,
        PagedListState state,
        string stateName)
    {
        RunInSta(() =>
        {
            using Form form = CreateListDemoForm(state);
            ScaleForTest(form, deviceDpi);
            form.Show();
            Application.DoEvents();
            form.PerformLayout();
            PagedListView list = Descendants(form)
                .OfType<PagedListView>()
                .Single();

            Assert.Equal(state, list.State);
            Assert.Empty(ThemeManager.FindUnthemedControls(form));
            Assert.True(
                list.Paging.Height >= UiDimensions.PagingFooterHeight);
            Assert.All(
                Descendants(form).Where(control => control.Visible),
                control => Assert.True(
                    control.Width >= 0 && control.Height >= 0));

            CaptureIfRequested(
                form,
                $"list-foundation-{stateName}-{deviceDpi}dpi.png");
        });
    }

    private static Form CreateListDemoForm(PagedListState state)
    {
        Form form = CreateOffscreenForm();
        form.Text = "List foundation preview";
        form.ClientSize = new Size(
            UiDimensions.MinimumWindowWidth,
            UiDimensions.MinimumWindowHeight);
        form.Padding = new Padding(UiSpacing.Space24);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        ControlStyler.StylePanel(layout, ThemeSurface.Application);

        var filters = new FilterBar
        {
            AccessibleName = "Demo record filters",
        };
        filters.AddFilter(new DarkTextBox
        {
            Width = 280,
            PlaceholderText = "Search company or contact",
            AccessibleName = "Search records",
        });
        var status = new DarkComboBox
        {
            Width = 160,
            AccessibleName = "Status filter",
        };
        status.Items.AddRange(["Active", "Archived", "All"]);
        status.SelectedIndex = 0;
        filters.AddFilter(status);
        filters.AddFilter(new DarkButton
        {
            Text = "Clear filters",
            Variant = ButtonVariant.Ghost,
        });

        PagedListView list = CreateListView();
        switch (state)
        {
            case PagedListState.Ready:
                list.BindResult(CreateResult());
                break;
            case PagedListState.Loading:
                list.BindResult(CreateResult());
                list.ShowLoading("Loading customer records…");
                break;
            case PagedListState.Empty:
                list.ShowEmpty(
                    "No matching customers",
                    "Change or clear the filters to see more records.",
                    secondaryActionText: "Clear filters");
                break;
            case PagedListState.Error:
                list.ShowError(
                    "Check the connection and try this operation again.",
                    "Reference: LIST-PREVIEW-001");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state));
        }

        layout.Controls.Add(filters, 0, 0);
        layout.Controls.Add(list, 0, 1);
        form.Controls.Add(layout);
        ThemeManager.Apply(form);
        return form;
    }

    private static PagedListView CreateListView()
    {
        var list = new PagedListView();
        list.Grid.AccessibleName = "Customer list preview";
        list.Grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Company",
            HeaderText = "Company",
            DataPropertyName = nameof(DemoListItem.Company),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 160F,
        });
        list.Grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Contact",
            HeaderText = "Primary contact",
            DataPropertyName = nameof(DemoListItem.Contact),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 120F,
        });
        list.Grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = "Status",
            DataPropertyName = nameof(DemoListItem.Status),
            Width = 140,
        });
        list.Grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "LastActivity",
            HeaderText = "Last activity",
            DataPropertyName = nameof(DemoListItem.LastActivity),
            Width = 160,
        });
        return list;
    }

    private static PagedResult<DemoListItem> CreateResult(
        int pageNumber = 1)
    {
        DemoListItem[] items = Enumerable.Range(1, 50)
            .Select(index => new DemoListItem(
                $"Company {index + ((pageNumber - 1) * 50):D3}",
                $"Contact {index:D2}",
                index % 5 == 0 ? "Archived" : "Active",
                $"{index:D2}/08/2026"))
            .ToArray();
        return new PagedResult<DemoListItem>(
            items,
            new PagingRequest(pageNumber, 50),
            hasNextPage: pageNumber < 5,
            totalItemCount: 243);
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

    private static void ScaleForTest(Form form, int deviceDpi)
    {
        if (deviceDpi == DpiScaler.BaselineDpi)
        {
            return;
        }

        float scale = deviceDpi / (float)DpiScaler.BaselineDpi;
        form.Scale(new SizeF(scale, scale));
    }

    private static void CaptureIfRequested(Form form, string fileName)
    {
        string? captureDirectory = Environment.GetEnvironmentVariable(
            "PBM_LIST_CAPTURE_DIR",
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

    private static void WaitForTask(Task task)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (!task.IsCompleted && DateTime.UtcNow < deadline)
        {
            Application.DoEvents();
            Thread.Sleep(1);
        }

        Assert.True(task.IsCompleted, "The asynchronous UI task timed out.");
        task.GetAwaiter().GetResult();
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

    private sealed record DemoListItem(
        string Company,
        string Contact,
        string Status,
        string LastActivity);
}
