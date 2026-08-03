using PersonalBusinessManager.Infrastructure.Configuration;

namespace PersonalBusinessManager.IntegrationTests;

public sealed class EnvironmentFileLoaderTests
{
    [Fact]
    public void LoadReadsAQuotedValueContainingEqualsSigns()
    {
        string variableName = CreateVariableName();
        string filePath = CreateEnvironmentFile(
            $"{variableName}=\"first=second;third=value\"");

        try
        {
            EnvironmentFileLoader.Load(filePath);

            Assert.Equal(
                "first=second;third=value",
                Environment.GetEnvironmentVariable(
                    variableName,
                    EnvironmentVariableTarget.Process));
        }
        finally
        {
            ClearVariable(variableName);
            DeleteEnvironmentFile(filePath);
        }
    }

    [Fact]
    public void LoadPreservesAnExplicitProcessValue()
    {
        string variableName = CreateVariableName();
        string filePath = CreateEnvironmentFile(
            $"{variableName}=file-value");

        try
        {
            Environment.SetEnvironmentVariable(
                variableName,
                "process-value",
                EnvironmentVariableTarget.Process);

            EnvironmentFileLoader.Load(filePath);

            Assert.Equal(
                "process-value",
                Environment.GetEnvironmentVariable(
                    variableName,
                    EnvironmentVariableTarget.Process));
        }
        finally
        {
            ClearVariable(variableName);
            DeleteEnvironmentFile(filePath);
        }
    }

    [Fact]
    public void LoadRejectsInvalidSyntaxWithoutEchoingTheLine()
    {
        const string secretText = "not-a-valid-secret-line";
        string filePath = CreateEnvironmentFile(secretText);

        try
        {
            FormatException exception = Assert.Throws<
                FormatException>(() =>
                    EnvironmentFileLoader.Load(filePath));

            Assert.Contains(
                "line 1",
                exception.Message,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                secretText,
                exception.Message,
                StringComparison.Ordinal);
        }
        finally
        {
            DeleteEnvironmentFile(filePath);
        }
    }

    private static string CreateVariableName()
    {
        return "PBM_ENV_FILE_TEST_"
            + Guid.NewGuid().ToString("N");
    }

    private static string CreateEnvironmentFile(params string[] lines)
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            "PersonalBusinessManager.EnvironmentFileLoaderTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);

        string filePath = Path.Combine(
            directoryPath,
            EnvironmentFileLoader.DefaultFileName);
        File.WriteAllLines(filePath, lines);

        return filePath;
    }

    private static void ClearVariable(string variableName)
    {
        Environment.SetEnvironmentVariable(
            variableName,
            null,
            EnvironmentVariableTarget.Process);
    }

    private static void DeleteEnvironmentFile(string filePath)
    {
        string? directoryPath = Path.GetDirectoryName(filePath);

        if (directoryPath is not null)
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
