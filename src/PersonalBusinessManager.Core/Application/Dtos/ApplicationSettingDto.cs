namespace PersonalBusinessManager.Core.Application.Dtos;

public sealed record ApplicationSettingDto
{
    public ulong RecordId { get; init; }

    public required string SettingKey { get; init; }

    public string? SettingValue { get; init; }

    public required string ValueTypeCode { get; init; }

    public bool IsSensitive { get; init; }

    public DateTime DateUpdatedUtc { get; init; }

    public ulong? UpdatedByUserId { get; init; }
}
