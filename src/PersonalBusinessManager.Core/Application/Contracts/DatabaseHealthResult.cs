namespace PersonalBusinessManager.Core.Application.Contracts;

public sealed record DatabaseHealthResult(
    bool IsAvailable,
    string Message);