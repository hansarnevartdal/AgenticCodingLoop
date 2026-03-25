namespace AgenticCodingLoop.Shared.HostEnvironment;

internal static class CopilotCliLocator
{
    private const string OverrideVariable = "COPILOT_CLI_PATH";

    public static string Find()
    {
        return Find(System.Environment.GetEnvironmentVariable(OverrideVariable), System.Environment.GetEnvironmentVariable("PATH"));
    }

    internal static string Find(string? overridePath, string? pathVariable)
    {
        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            var fullOverridePath = Path.GetFullPath(overridePath);
            if (!File.Exists(fullOverridePath))
            {
                throw new InvalidOperationException($"The configured Copilot CLI path does not exist: {fullOverridePath}");
            }

            return fullOverridePath;
        }

        foreach (var pathEntry in EnumeratePathEntries(pathVariable))
        {
            foreach (var fileName in GetFileNames())
            {
                var candidate = Path.Combine(pathEntry, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        throw new InvalidOperationException(
            "Could not find the GitHub Copilot CLI on PATH. Install it and ensure `copilot` is available, or set COPILOT_CLI_PATH.");
    }

    private static IEnumerable<string> EnumeratePathEntries(string? pathVariable)
    {
        if (string.IsNullOrWhiteSpace(pathVariable))
        {
            yield break;
        }

        foreach (var pathEntry in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(pathEntry))
            {
                yield return pathEntry;
            }
        }
    }

    private static string[] GetFileNames()
    {
        return OperatingSystem.IsWindows()
            ? ["copilot.exe", "copilot.cmd", "copilot.bat"]
            : ["copilot"];
    }
}