using System.Security.Cryptography;
using PersonalBusinessManager.Infrastructure.Database.Migrations;

namespace PersonalBusinessManager.IntegrationTests;

public sealed class BaselineRegistrationTests
{
    [Fact]
    public void BaselineVersionIsFixedAtThirteen()
    {
        Assert.Equal(13, BaselineSchemaManifest.Version);
    }

    [Fact]
    public void BaselineSelectionNeverIncludesLaterMigrations()
    {
        MigrationDescriptor[] migrations =
        [
            new(1, "Initial"),
            new(13, "Baseline final"),
            new(14, "Later migration"),
        ];

        IReadOnlyList<MigrationDescriptor> selected =
            BaselineRegistrationService
                .SelectBaselineMigrations(migrations);

        Assert.Equal(
            new long[] { 1, 13 },
            selected.Select(migration => migration.Version));
    }

    [Fact]
    public void ManifestMismatchIsRejected()
    {
        var snapshot = new BaselineSchemaSnapshot(
            new string('0', 64),
            BaselineSchemaManifest.ApplicationTableCount,
            BaselineSchemaManifest.ColumnCount,
            BaselineSchemaManifest.ConstraintCount,
            BaselineSchemaManifest.CheckConstraintCount,
            BaselineSchemaManifest.ForeignKeyColumnCount,
            BaselineSchemaManifest.IndexColumnCount,
            BaselineSchemaManifest.MetadataRecordCount);

        IReadOnlyList<string> problems =
            BaselineSchemaVerifier.GetManifestProblems(
                snapshot);

        Assert.Contains(
            problems,
            problem => problem.Contains(
                "fingerprint",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task BackupEvidenceRequiresMatchingSha256()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"pbm-baseline-test-{Guid.NewGuid():N}.sql");
        byte[] content = "safe disposable backup content"u8.ToArray();

        try
        {
            await File.WriteAllBytesAsync(path, content);
            string expectedHash = Convert.ToHexString(
                    SHA256.HashData(content))
                .ToLowerInvariant();

            (VerifiedBackupEvidence? evidence, string? error) =
                await BackupEvidenceVerifier.VerifyAsync(
                    path,
                    expectedHash,
                    CancellationToken.None);

            Assert.Null(error);
            Assert.NotNull(evidence);
            Assert.Equal(
                Path.GetFileName(path),
                evidence.FileName);
            Assert.Equal(content.Length, evidence.SizeBytes);
            Assert.Equal(expectedHash, evidence.Sha256);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task BackupEvidenceRejectsHashMismatch()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"pbm-baseline-test-{Guid.NewGuid():N}.sql");

        try
        {
            await File.WriteAllTextAsync(
                path,
                "safe disposable backup content");

            (VerifiedBackupEvidence? evidence, string? error) =
                await BackupEvidenceVerifier.VerifyAsync(
                    path,
                    new string('0', 64),
                    CancellationToken.None);

            Assert.Null(evidence);
            Assert.Contains(
                "does not match",
                error,
                StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
