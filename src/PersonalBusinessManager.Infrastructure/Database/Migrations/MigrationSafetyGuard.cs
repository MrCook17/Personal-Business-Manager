namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public static class MigrationSafetyGuard
{
    public static string? GetBlockReason(MigrationStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        if (string.IsNullOrWhiteSpace(status.DatabaseName))
        {
            return "The migration connection must select an explicit database.";
        }

        if (status.ApplicationTableCount > 0
            && status.AppliedVersions.Count == 0)
        {
            return
                "The target contains an existing unversioned schema. "
                + "No migrations were executed. Verify and register the "
                + "approved baseline with the controlled baseline tool first.";
        }

        long expectedVersion = 1;

        foreach (long appliedVersion in status.AppliedVersions.Distinct())
        {
            if (appliedVersion != expectedVersion)
            {
                return
                    "Migration history is not a contiguous sequence "
                    + "starting at version 1. No migrations were executed.";
            }

            expectedVersion++;
        }

        HashSet<long> availableVersions = status
            .AvailableMigrations
            .Select(migration => migration.Version)
            .ToHashSet();

        long? unknownAppliedVersion = status.AppliedVersions
            .Select(version => (long?)version)
            .FirstOrDefault(version =>
                version is not null
                && !availableVersions.Contains(version.Value));

        if (unknownAppliedVersion is not null)
        {
            return
                $"Applied migration version {unknownAppliedVersion.Value} "
                + "is not present in the migration assembly. "
                + "No migrations were executed.";
        }

        return null;
    }
}
