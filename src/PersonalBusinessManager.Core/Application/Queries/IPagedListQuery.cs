using PersonalBusinessManager.Core.Application.Filters;

namespace PersonalBusinessManager.Core.Application.Queries;

public interface IPagedListQuery<in TFilter, TListItem>
    where TFilter : ListFilter
{
    Task<PagedResult<TListItem>> ExecuteAsync(
        TFilter filter,
        CancellationToken cancellationToken);
}
