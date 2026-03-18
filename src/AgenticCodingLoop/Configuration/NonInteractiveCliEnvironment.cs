namespace AgenticCodingLoop.Configuration;

internal static class NonInteractiveCliEnvironment
{
    public static IReadOnlyDictionary<string, string> Create()
    {
        var environment = new Dictionary<string, string>(
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        foreach (var entry in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>())
        {
            if (entry.Key is not string key || entry.Value is null)
            {
                continue;
            }

            environment[key] = entry.Value.ToString() ?? string.Empty;
        }

        environment["GH_PROMPT_DISABLED"] = "1";
        environment["GIT_TERMINAL_PROMPT"] = "0";

        return environment;
    }
}