using System.Text.RegularExpressions;

namespace PersonalBusinessManager.Infrastructure.Configuration;

/// <summary>
/// Loads local development settings from the nearest repository .env file.
/// </summary>
public static partial class EnvironmentFileLoader
{
    public const string DefaultFileName = ".env";

    private const string SolutionFileName =
        "PersonalBusinessManager.slnx";

    /// <summary>
    /// Finds and loads the nearest .env file, starting from the application
    /// directory and then the current working directory.
    /// </summary>
    /// <returns>The loaded file path, or <see langword="null"/> when absent.</returns>
    public static string? Load()
    {
        string? filePath = FindNearestFile(
            AppContext.BaseDirectory)
            ?? FindNearestFile(Directory.GetCurrentDirectory());

        if (filePath is null)
        {
            return null;
        }

        Load(filePath);
        return filePath;
    }

    /// <summary>
    /// Loads variables from a specific dotenv file into the current process.
    /// Existing explicit process values take precedence. On Windows, values
    /// merely inherited from the user environment can be replaced by .env.
    /// </summary>
    public static void Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        int lineNumber = 0;

        foreach (string sourceLine in File.ReadLines(filePath))
        {
            lineNumber++;
            string line = sourceLine.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.Ordinal))
            {
                line = line[7..].TrimStart();
            }

            int separatorIndex = line.IndexOf('=');

            if (separatorIndex <= 0)
            {
                throw InvalidLine(filePath, lineNumber);
            }

            string variableName = line[..separatorIndex].Trim();

            if (!VariableNamePattern().IsMatch(variableName))
            {
                throw InvalidLine(filePath, lineNumber);
            }

            string value = ParseValue(
                line[(separatorIndex + 1)..].Trim(),
                filePath,
                lineNumber);

            if (CanSetProcessValue(variableName))
            {
                Environment.SetEnvironmentVariable(
                    variableName,
                    value,
                    EnvironmentVariableTarget.Process);
            }
        }
    }

    private static string? FindNearestFile(string startDirectory)
    {
        var directory = new DirectoryInfo(
            Path.GetFullPath(startDirectory));
        bool isStartDirectory = true;

        while (directory is not null)
        {
            string candidatePath = Path.Combine(
                directory.FullName,
                DefaultFileName);
            bool isRepositoryRoot = File.Exists(
                Path.Combine(
                    directory.FullName,
                    SolutionFileName));

            if (File.Exists(candidatePath)
                && (isStartDirectory || isRepositoryRoot))
            {
                return candidatePath;
            }

            if (isRepositoryRoot)
            {
                return null;
            }

            directory = directory.Parent;
            isStartDirectory = false;
        }

        return null;
    }

    private static bool CanSetProcessValue(string variableName)
    {
        string? processValue = Environment.GetEnvironmentVariable(
            variableName,
            EnvironmentVariableTarget.Process);

        if (string.IsNullOrWhiteSpace(processValue))
        {
            return true;
        }

        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        string? userValue = Environment.GetEnvironmentVariable(
            variableName,
            EnvironmentVariableTarget.User);

        return !string.IsNullOrWhiteSpace(userValue)
            && string.Equals(
                processValue,
                userValue,
                StringComparison.Ordinal);
    }

    private static string ParseValue(
        string value,
        string filePath,
        int lineNumber)
    {
        if (value.Length == 0)
        {
            return string.Empty;
        }

        char quote = value[0];

        if (quote is not ('\'' or '"'))
        {
            return value;
        }

        if (value.Length < 2 || value[^1] != quote)
        {
            throw InvalidLine(filePath, lineNumber);
        }

        string unquotedValue = value[1..^1];

        return quote == '"'
            ? Regex.Unescape(unquotedValue)
            : unquotedValue;
    }

    private static FormatException InvalidLine(
        string filePath,
        int lineNumber)
    {
        return new FormatException(
            $"Invalid dotenv syntax in {filePath} at line {lineNumber}.");
    }

    [GeneratedRegex(
        "^[A-Za-z_][A-Za-z0-9_]*$",
        RegexOptions.CultureInvariant)]
    private static partial Regex VariableNamePattern();
}
