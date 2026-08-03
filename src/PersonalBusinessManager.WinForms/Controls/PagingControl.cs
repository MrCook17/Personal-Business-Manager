using System.ComponentModel;
using PersonalBusinessManager.Core.Application.Filters;
using PersonalBusinessManager.Core.Application.Queries;
using PersonalBusinessManager.WinForms.Theming;

namespace PersonalBusinessManager.WinForms.Controls;

public sealed class PagingRequestEventArgs(PagingRequest request)
    : EventArgs
{
    public PagingRequest Request { get; } =
        request ?? throw new ArgumentNullException(nameof(request));
}

[DefaultEvent(nameof(PageRequested))]
[DesignerCategory("Code")]
public sealed class PagingControl : UserControl, IThemeAwareControl
{
    private readonly Label _rangeLabel = new();
    private readonly Label _pageLabel = new();
    private readonly Label _rowsLabel = new();
    private readonly DarkButton _previousButton = new();
    private readonly DarkButton _nextButton = new();
    private readonly DarkComboBox _pageSizeInput = new();
    private int _pageNumber = PagingRequest.DefaultPageNumber;
    private int _pageSize = PagingRequest.DefaultPageSize;
    private int _visibleItemCount;
    private long? _totalItemCount;
    private bool _hasPreviousPage;
    private bool _hasNextPage;
    private bool _updating;

    public PagingControl()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        Dock = DockStyle.Bottom;
        Height = UiDimensions.PagingFooterHeight;
        MinimumSize = new Size(0, UiDimensions.PagingFooterHeight);
        Margin = Padding.Empty;
        Padding = new Padding(
            UiSpacing.Space16,
            UiSpacing.Space4,
            UiSpacing.Space16,
            UiSpacing.Space4);
        TabStop = false;
        AccessibleRole = AccessibleRole.Grouping;
        AccessibleName = "Paging controls";

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));
        for (int index = 1; index < layout.ColumnCount; index++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        }

        ConfigureLabel(_rangeLabel, "No records");
        _rangeLabel.AccessibleName = "Visible record range";

        _previousButton.Text = "Previous";
        _previousButton.SizeVariant = ControlSize.Compact;
        _previousButton.AccessibleName = "Previous page";
        _previousButton.Margin = new Padding(
            UiSpacing.Space8,
            0,
            UiSpacing.Space8,
            0);
        _previousButton.Click += (_, _) => RequestPage(
            Math.Max(PagingRequest.DefaultPageNumber, _pageNumber - 1),
            _pageSize);

        ConfigureLabel(_pageLabel, "Page 1");
        _pageLabel.AccessibleName = "Current page";

        _nextButton.Text = "Next";
        _nextButton.SizeVariant = ControlSize.Compact;
        _nextButton.AccessibleName = "Next page";
        _nextButton.Margin = new Padding(
            UiSpacing.Space8,
            0,
            UiSpacing.Space16,
            0);
        _nextButton.Click += (_, _) => RequestPage(
            checked(_pageNumber + 1),
            _pageSize);

        ConfigureLabel(_rowsLabel, "Rows");
        _rowsLabel.AccessibleName = "Rows per page label";
        _rowsLabel.Margin = new Padding(0, 0, UiSpacing.Space8, 0);

        _pageSizeInput.Width = 80;
        _pageSizeInput.AccessibleName = "Rows per page";
        _pageSizeInput.Margin = Padding.Empty;
        _pageSizeInput.SelectedIndexChanged += (_, _) =>
            PageSizeInput_SelectedIndexChanged();

        layout.Controls.Add(_rangeLabel, 0, 0);
        layout.Controls.Add(_previousButton, 1, 0);
        layout.Controls.Add(_pageLabel, 2, 0);
        layout.Controls.Add(_nextButton, 3, 0);
        layout.Controls.Add(_rowsLabel, 4, 0);
        layout.Controls.Add(_pageSizeInput, 5, 0);
        Controls.Add(layout);

        SetAllowedPageSizes([50, 100, 200]);
        UpdatePresentation();
        ApplyTheme();
    }

    public event EventHandler<PagingRequestEventArgs>? PageRequested;

    [Browsable(false)]
    public PagingRequest CurrentRequest =>
        new(_pageNumber, _pageSize);

    [Browsable(false)]
    public string RangeText => _rangeLabel.Text;

    [Browsable(false)]
    public string PageText => _pageLabel.Text;

    [Browsable(false)]
    public bool PreviousPageEnabled => _previousButton.Enabled;

    [Browsable(false)]
    public bool NextPageEnabled => _nextButton.Enabled;

    [Browsable(false)]
    public IReadOnlyList<int> AllowedPageSizes =>
        _pageSizeInput.Items.Cast<int>().ToArray();

    public void SetAllowedPageSizes(IEnumerable<int> pageSizes)
    {
        ArgumentNullException.ThrowIfNull(pageSizes);
        int[] validatedSizes = pageSizes
            .Distinct()
            .Order()
            .ToArray();

        if (validatedSizes.Length == 0)
        {
            throw new ArgumentException(
                "At least one page size is required.",
                nameof(pageSizes));
        }

        foreach (int pageSize in validatedSizes)
        {
            _ = new PagingRequest(pageSize: pageSize);
        }

        _updating = true;
        try
        {
            _pageSizeInput.Items.Clear();
            _pageSizeInput.Items.AddRange(
                validatedSizes.Cast<object>().ToArray());
            _pageSize = validatedSizes.Contains(_pageSize)
                ? _pageSize
                : validatedSizes[0];
            _pageSizeInput.SelectedItem = _pageSize;
        }
        finally
        {
            _updating = false;
        }
    }

    public void ApplyResult<T>(PagedResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        _pageNumber = result.PageNumber;
        _pageSize = result.PageSize;
        _visibleItemCount = result.Items.Count;
        _totalItemCount = result.TotalItemCount;
        _hasPreviousPage = result.HasPreviousPage;
        _hasNextPage = result.HasNextPage;

        if (!_pageSizeInput.Items.Contains(_pageSize))
        {
            _pageSizeInput.Items.Add(_pageSize);
        }

        _updating = true;
        try
        {
            _pageSizeInput.SelectedItem = _pageSize;
        }
        finally
        {
            _updating = false;
        }

        UpdatePresentation();
    }

    public void ApplyTheme()
    {
        ControlStyler.StylePanel(this, ThemeSurface.Panel);
        foreach (Control child in Controls)
        {
            ThemeManager.ApplyControlTree(child);
        }

        Invalidate(true);
    }

    private static void ConfigureLabel(Label label, string text)
    {
        label.AutoSize = true;
        label.Anchor = AnchorStyles.Left;
        label.Text = text;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Margin = Padding.Empty;
        ControlStyler.StyleLabel(
            label,
            ThemeTextRole.Label,
            ThemePalette.SecondaryText);
    }

    private void PageSizeInput_SelectedIndexChanged()
    {
        if (_updating || _pageSizeInput.SelectedItem is not int pageSize)
        {
            return;
        }

        RequestPage(PagingRequest.DefaultPageNumber, pageSize);
    }

    private void RequestPage(int pageNumber, int pageSize)
    {
        PageRequested?.Invoke(
            this,
            new PagingRequestEventArgs(
                new PagingRequest(pageNumber, pageSize)));
    }

    private void UpdatePresentation()
    {
        long firstItem = _visibleItemCount == 0
            ? 0
            : checked((long)(_pageNumber - 1) * _pageSize) + 1;
        long lastItem = _visibleItemCount == 0
            ? 0
            : firstItem + _visibleItemCount - 1;

        _rangeLabel.Text = _visibleItemCount == 0
            ? "No records"
            : _totalItemCount.HasValue
                ? $"{firstItem:N0}–{lastItem:N0} of "
                    + $"{_totalItemCount.Value:N0}"
                : $"{firstItem:N0}–{lastItem:N0}";
        _pageLabel.Text = $"Page {_pageNumber:N0}";
        _previousButton.Enabled = _hasPreviousPage;
        _nextButton.Enabled = _hasNextPage;
        AccessibleDescription =
            $"{_rangeLabel.Text}. {_pageLabel.Text}.";
    }
}
