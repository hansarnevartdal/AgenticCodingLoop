namespace AgenticCodingLoop.Configuration;

internal static class NonInteractiveCliEnvironment
{
    public static IReadOnlyDictionary<string, string> Values { get; } = new Dictionary<string, string>
    {
        ["GH_PROMPT_DISABLED"] = "1",
        ["GIT_TERMINAL_PROMPT"] = "0"
    };
}