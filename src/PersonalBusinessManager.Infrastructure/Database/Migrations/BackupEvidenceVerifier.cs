using System.Security.Cryptography;

namespace PersonalBusinessManager.Infrastructure.Database.Migrations;

public static class BackupEvidenceVerifier
{
    public static async Task<(VerifiedBackupEvidence? Evidence, string? Error)>
        VerifyAsync(
            string? backupPath,
            string? expectedSha256,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(backupPath))
        {
            return (null, "A backup path is required.");
        }

        if (string.IsNullOrWhiteSpace(expectedSha256)
            || expectedSha256.Length != 64
            || expectedSha256.Any(character =>
                !Uri.IsHexDigit(character)))
        {
            return (
                null,
                "A 64-character hexadecimal backup SHA-256 is required.");
        }

        string fullPath;

        try
        {
            fullPath = Path.GetFullPath(backupPath);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or NotSupportedException
                or PathTooLongException)
        {
            return (null, "The backup path is invalid.");
        }

        if (!File.Exists(fullPath))
        {
            return (null, "The supplied backup file does not exist.");
        }

        var file = new FileInfo(fullPath);

        if (file.Length <= 0)
        {
            return (null, "The supplied backup file is empty.");
        }

        string actualHash;

        try
        {
            await using FileStream stream = new(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                1024 * 128,
                FileOptions.Asynchronous
                    | FileOptions.SequentialScan);

            byte[] hash = await SHA256.HashDataAsync(
                stream,
                cancellationToken);
            actualHash = Convert.ToHexString(hash)
                .ToLowerInvariant();
        }
        catch (Exception exception)
            when (exception is IOException
                or UnauthorizedAccessException)
        {
            return (null, "The supplied backup file could not be read.");
        }

        if (!string.Equals(
                actualHash,
                expectedSha256,
                StringComparison.OrdinalIgnoreCase))
        {
            return (
                null,
                "The supplied backup SHA-256 does not match the file.");
        }

        return (
            new VerifiedBackupEvidence(
                file.Name,
                file.Length,
                actualHash),
            null);
    }
}
