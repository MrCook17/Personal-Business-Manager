using System.Security.Cryptography;
using System.Text;
using Dapper;
using MySqlConnector;
using PersonalBusinessManager.Infrastructure.Database.Migrations;

namespace PersonalBusinessManager.IntegrationTests;

[Collection(MariaDbTestGroup.Name)]
public sealed class ExistingSchemaBaselineIntegrationTests
{
    [MariaDbMigrationTestFact]
    public async Task ExistingSchemaCopyCanBeBaselinedWithoutMigrationReplay()
    {
        string connectionString = MariaDbTestEnvironment
            .GetRequiredMigrationConnectionString();
        var catalog = new MigrationCatalog();
        var verifier = new BaselineSchemaVerifier(
            connectionString,
            catalog);
        BaselineVerificationResult before =
            await verifier.VerifyCurrentAsync(
                CancellationToken.None);

        Assert.True(before.Succeeded);

        List<MigrationHistoryRow> originalHistory;
        SchemaInformationMetadata originalMetadata;

        await using (var connection = new MySqlConnection(
            connectionString))
        {
            await connection.OpenAsync();
            originalHistory = (await connection.QueryAsync<
                MigrationHistoryRow>(
                """
                SELECT
                    `version` AS Version,
                    `applied_on_utc` AS AppliedOnUtc,
                    `description` AS Description
                FROM `schema_migrations`
                ORDER BY `version`;
                """)).AsList();
            originalMetadata = await connection.QuerySingleAsync<
                SchemaInformationMetadata>(
                """
                SELECT
                    `schema_version` AS SchemaVersion,
                    `last_verified_utc` AS LastVerifiedUtc,
                    `date_updated_utc` AS DateUpdatedUtc
                FROM `schema_information`
                WHERE `record_id` = 1;
                """);
        }

        Assert.Equal(
            Enumerable.Range(1, 13).Select(
                version => (long)version),
            originalHistory.Select(row => row.Version));

        string backupPath = Path.Combine(
            Path.GetTempPath(),
            $"pbm-p208-baseline-{Guid.NewGuid():N}.txt");
        string evidence =
            $"P2-08 disposable test evidence{Environment.NewLine}"
            + $"schema={before.Schema.FingerprintSha256}{Environment.NewLine}"
            + $"data={before.Data.FingerprintSha256}{Environment.NewLine}";
        byte[] evidenceBytes = Encoding.UTF8.GetBytes(evidence);
        string evidenceSha256 = Convert.ToHexString(
                SHA256.HashData(evidenceBytes))
            .ToLowerInvariant();
        await File.WriteAllBytesAsync(
            backupPath,
            evidenceBytes);

        try
        {
            await ClearMigrationHistoryAsync(connectionString);

            var service = new BaselineRegistrationService(
                connectionString,
                catalog,
                verifier);
            BaselineRegistrationResult result =
                await service.RegisterExistingAsync(
                    BaselineSchemaManifest.Version,
                    "BASELINE personal_business_manager_test TO 13",
                    backupPath,
                    evidenceSha256,
                    CancellationToken.None);

            Assert.True(result.Succeeded, result.Message);
            Assert.Equal(
                Enumerable.Range(1, 13).Select(
                    version => (long)version),
                result.RegisteredVersions);
            Assert.NotNull(result.Verification);
            Assert.Equal(
                before.Schema.FingerprintSha256,
                result.Verification.Schema.FingerprintSha256);
            Assert.Equal(
                before.Data.FingerprintSha256,
                result.Verification.Data.FingerprintSha256);
            Assert.Equal(
                before.Data.FingerprintSha256,
                result.BeforeData?.FingerprintSha256);
            Assert.Equal(
                before.Data.FingerprintSha256,
                result.AfterData?.FingerprintSha256);
        }
        finally
        {
            await RestoreMetadataAsync(
                connectionString,
                originalHistory,
                originalMetadata);
            File.Delete(backupPath);
        }

        BaselineVerificationResult restored =
            await verifier.VerifyCurrentAsync(
                CancellationToken.None);

        Assert.True(restored.Succeeded);
        Assert.Equal(
            before.Schema.FingerprintSha256,
            restored.Schema.FingerprintSha256);
        Assert.Equal(
            before.Data.FingerprintSha256,
            restored.Data.FingerprintSha256);
    }

    private static async Task ClearMigrationHistoryAsync(
        string connectionString)
    {
        await using var connection = new MySqlConnection(
            connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "DELETE FROM `schema_migrations`;");
    }

    private static async Task RestoreMetadataAsync(
        string connectionString,
        IReadOnlyList<MigrationHistoryRow> originalHistory,
        SchemaInformationMetadata originalMetadata)
    {
        await using var connection = new MySqlConnection(
            connectionString);
        await connection.OpenAsync();
        await using MySqlTransaction transaction =
            await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(
                "DELETE FROM `schema_migrations`;",
                transaction: transaction);

            const string insertHistorySql =
                """
                INSERT INTO `schema_migrations` (
                    `version`,
                    `applied_on_utc`,
                    `description`)
                VALUES (
                    @Version,
                    @AppliedOnUtc,
                    @Description);
                """;
            await connection.ExecuteAsync(
                insertHistorySql,
                originalHistory,
                transaction);

            await connection.ExecuteAsync(
                """
                UPDATE `schema_information`
                SET
                    `schema_version` = @SchemaVersion,
                    `last_verified_utc` = @LastVerifiedUtc,
                    `date_updated_utc` = @DateUpdatedUtc
                WHERE `record_id` = 1;
                """,
                originalMetadata,
                transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private sealed class MigrationHistoryRow
    {
        public long Version { get; init; }

        public DateTime? AppliedOnUtc { get; init; }

        public string? Description { get; init; }
    }

    private sealed class SchemaInformationMetadata
    {
        public long SchemaVersion { get; init; }

        public DateTime? LastVerifiedUtc { get; init; }

        public DateTime DateUpdatedUtc { get; init; }
    }
}
