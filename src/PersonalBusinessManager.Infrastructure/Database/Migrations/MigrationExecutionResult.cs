namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public sealed record MigrationExecutionResult(
    bool Succeeded,
    string Message,
    MigrationStatus? Status = null);
