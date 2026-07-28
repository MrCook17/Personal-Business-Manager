namespace PersonalBusinessManager.Core.Application.Contracts;

public interface IDatabaseHealthService
{
    Task<DatabaseHealthResult> CheckAsync(
        CancellationToken cancellationToken = default);
}