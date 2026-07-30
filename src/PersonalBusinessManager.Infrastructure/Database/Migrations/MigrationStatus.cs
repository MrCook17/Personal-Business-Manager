namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public sealed record MigrationStatus(
    string ServerHost,
    string DatabaseName,
    string ServerVersion,
    string DatabaseAccount,
    bool MigrationHistoryTableExists,
    int ApplicationTableCount,
    IReadOnlyList<MigrationDescriptor> AvailableMigrations,
    IReadOnlyList<long> AppliedVersions,
    IReadOnlyList<MigrationDescriptor> PendingMigrations,
    long? SchemaInformationVersion)
{
    public long? HighestAppliedVersion =>
        AppliedVersions.Count == 0
            ? null
            : AppliedVersions.Max();

    public bool HistoryAndSchemaInformationAgree =>
        HighestAppliedVersion is not null
        && SchemaInformationVersion == HighestAppliedVersion;
}
