using PersonalBusinessManager.Core.Application.Dtos;

namespace PersonalBusinessManager.Core.Application.Contracts;

public interface IApplicationSettingRepository
{
    Task<ApplicationSettingDto?> GetByKeyAsync(
        string settingKey,
        CancellationToken cancellationToken = default);

    Task<ulong> InsertAsync(
        ApplicationSettingDto setting,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        ApplicationSettingDto setting,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteByKeyAsync(
        string settingKey,
        CancellationToken cancellationToken = default);
}
