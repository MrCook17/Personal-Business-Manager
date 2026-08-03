# List, search, filter and paging conventions

**Status:** Approved implementation convention

**Applies from:** P2-12

**Last reviewed:** 3 August 2026

This document turns the approved list behaviour in the final plan, dark-theme
system and feature wireframes into a shared implementation contract. It is
foundation only: customer, job and other feature-specific filters, projections
and SQL remain owned by their later phases.

## Dependency boundary

The layers have distinct responsibilities:

1. Core owns `ListFilter`, `PagingRequest`, `SortDirection`, `PagedResult<T>`
   and `IPagedListQuery<TFilter, TListItem>`.
2. Infrastructure owns Dapper commands, parameterised SQL, MariaDB filtering,
   deterministic ordering and page materialisation.
3. WinForms owns filter controls, debouncing, cancellation coordination and
   list presentation. WinForms must contain no SQL.

Every feature list calls an application query contract and returns a lightweight
list projection. Do not bind domain object graphs or unlimited repository
results directly to a grid.

## Filter contract

Feature filters derive from `ListFilter`. The shared base provides:

- trimmed, null-normalised free-text search;
- explicit archive visibility;
- a validated `PagingRequest`.

Each feature adds typed filters and a typed sort field. User-provided column or
property names must never be concatenated into SQL. Map a feature sort enum to a
reviewed column expression and use the shared `SortDirection` only for the
whitelisted `ASC` or `DESC` keyword.

Example shape:

```csharp
public sealed record ExampleFilter : ListFilter
{
    public ExampleSort Sort { get; init; } = ExampleSort.DisplayName;

    public SortDirection Direction { get; init; } =
        SortDirection.Ascending;
}
```

Changing search text, filters, sorting or page size resets the request to page
one. Back-navigation may restore a previously captured filter and paging value.

## Page sizes and result limits

The initial approved feature defaults remain:

- 100 rows: customers, jobs, tasks, financial accounts and account snapshots;
- 50 rows: time, invoices, expenses, audit history and backups.

`PagingRequest` defaults to 100 and rejects values above 200. The application
may later expose a reviewed subset such as 50, 100 and 200 in Settings, but the
core maximum remains authoritative.

Repositories request `PageSize + 1` rows. The extra row determines whether a
next page exists and is not returned to WinForms. `PagedResult<T>` refuses to
contain more items than the validated page size.

## Keyset pagination

Prefer keyset pagination for large, stable lists. Every order must be explicit
and deterministic. Add the immutable primary key as the final tie-breaker even
when the visible sort column is unique in normal data.

Descending example:

```sql
WHERE
    (@AfterSortValue IS NULL)
    OR sort_value < @AfterSortValue
    OR (
        sort_value = @AfterSortValue
        AND record_id < @AfterRecordId
    )
ORDER BY sort_value DESC, record_id DESC
LIMIT @QueryRowLimit;
```

For ascending order, reverse both comparison operators and both order
directions. Keep all cursor parts together. A nullable cursor means the first
page; it must not silently mean an unbounded query.

Offset paging is allowed only when direct page-number navigation is genuinely
required and the expected volume remains manageable. It must still use a
validated page size and a deterministic `ORDER BY` with the record ID as the
final tie-breaker.

## Projections and commands

Select only the columns required by the list. Use names such as
`CustomerListItem`, `JobListItem` or `AuditRecordListItem`; fetch full detail
through the detail query after the user opens a record.

All list SQL must be parameterised. Create its Dapper `CommandDefinition`
through `ListQueryConventions.CreateCommand`, which applies the shared 30-second
command timeout and requires a cancellation token. A feature may use a shorter
timeout. A longer timeout needs a documented reason close to the query.

`ListQueryConventions.CreateKeysetPage` consumes at most the validated
look-ahead limit and returns at most `PageSize` projections.

## Search and asynchronous loading

Use one `DebouncedSearchCoordinator` per free-text search workflow. Its default
delay is 300 ms and its supported range is 250–400 ms. Queueing a newer search
cancels both the pending debounce and any obsolete active search.

Every query API accepts `CancellationToken` as its final, required parameter.
Repositories pass it into the Dapper command. Cancellation is an expected
outcome and must not be shown as an error.

`PagedListView.LoadAsync` returns to the message loop while I/O is pending,
cancels an older load when a newer request starts, and applies a result only if
it still owns the current request. Failures raise `LoadFailed` for application
logging while displaying only a safe message and optional correlation reference.

## WinForms presentation

Compose list pages from:

- `FilterBar` above the result region;
- `PagedListView` for the grid and explicit states;
- `DarkDataGridView` using ordinary paged binding by default;
- `PagingControl` below the grid.

The filter bar remains visible during loading. The list region must explicitly
show ready, loading, empty or error/retry state. Do not leave an unexplained blank
grid. Warning and error text must not rely on colour alone.

Paging controls expose range, page number, Previous, Next and a validated rows
selector. They update only after an accepted result; a failed request therefore
does not falsely move the displayed page.

Use `DataGridView.VirtualMode` only after paging has proved unsuitable and a
tested row cache exists. Feature pages must retain keyboard focus, readable
selected rows, double buffering and DPI-safe layout at 100%, 125% and 150%.

## Verification checklist for each feature list

- Query filtering, sorting and paging occur in MariaDB.
- SQL has a deterministic primary-key tie-breaker.
- The command has a bounded `LIMIT` and shared timeout.
- The returned projection contains only list columns.
- A newer search or page request cancels the obsolete request.
- The UI remains responsive while the query is pending.
- Empty, loading and safe error/retry states are explicit.
- Filters remain visible and retained during loading and retry.
- WinForms contains no SQL.
- The list is keyboard-usable and inspected at 96, 120 and 144 DPI.
