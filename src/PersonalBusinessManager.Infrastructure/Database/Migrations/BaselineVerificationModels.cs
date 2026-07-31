namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public sealed record BaselineSchemaSnapshot(
    string FingerprintSha256,
    int ApplicationTableCount,
    int ColumnCount,
    int ConstraintCount,
    int CheckConstraintCount,
    int ForeignKeyColumnCount,
    int IndexColumnCount,
    int MetadataRecordCount);

public sealed record BaselineDataSnapshot(
    string FingerprintSha256,
    int TableCount,
    long TotalRows,
    IReadOnlyDictionary<string, long> RowCounts,
    IReadOnlyDictionary<string, decimal> FinancialTotals);

public sealed record BaselineVerificationResult(
    bool Succeeded,
    string ServerHost,
    string DatabaseName,
    string ServerVersion,
    string DatabaseAccount,
    BaselineSchemaSnapshot Schema,
    BaselineDataSnapshot Data,
    IReadOnlyList<string> Problems);

public sealed record VerifiedBackupEvidence(
    string FileName,
    long SizeBytes,
    string Sha256);

public sealed record BaselineRegistrationResult(
    bool Succeeded,
    string Message,
    BaselineVerificationResult? Verification,
    VerifiedBackupEvidence? Backup,
    IReadOnlyList<long> RegisteredVersions,
    BaselineDataSnapshot? BeforeData,
    BaselineDataSnapshot? AfterData,
    string BuildIdentifier);
