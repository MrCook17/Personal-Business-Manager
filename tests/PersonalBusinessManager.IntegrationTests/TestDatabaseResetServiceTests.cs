using Microsoft.Extensions.Logging.Abstractions;
using PersonalBusinessManager.DatabaseMigrator;

namespace PersonalBusinessManager.IntegrationTests;

public sealed class TestDatabaseResetServiceTests
{
    [Fact]
    public async Task ResetRejectsUnsafeTargetBeforeDatabaseAccess()
    {
        TestDatabaseResetService service = CreateService(
            "Server=localhost;Database=personal_business_manager;"
            + "User ID=root");

        TestDatabaseResetResult result =
            await service.ResetAsync(
                "RESET TEST DATABASE personal_business_manager",
                CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(
            "normal development database",
            result.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResetRequiresExactConfirmationBeforeDatabaseAccess()
    {
        TestDatabaseResetService service = CreateService(
            "Server=localhost;"
            + "Database=personal_business_manager_test;"
            + "User ID=personal_business_test_migrator");

        TestDatabaseResetResult result =
            await service.ResetAsync(
                "RESET TEST DATABASE another_test",
                CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(
            "Confirmation must be exactly: RESET TEST DATABASE "
            + "personal_business_manager_test",
            result.Message);
    }

    private static TestDatabaseResetService CreateService(
        string connectionString)
    {
        return new TestDatabaseResetService(
            connectionString,
            null!,
            null!,
            NullLogger<TestDatabaseResetService>.Instance);
    }
}
