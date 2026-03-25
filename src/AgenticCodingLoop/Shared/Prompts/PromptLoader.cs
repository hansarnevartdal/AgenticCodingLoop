using System.Reflection;

namespace AgenticCodingLoop.Shared.Prompts;

internal static class PromptLoader
{
    private static readonly Assembly Assembly = typeof(PromptLoader).Assembly;

    internal static string Load(string promptName)
    {
        var resourceName = promptName.StartsWith("Features.", StringComparison.Ordinal)
            ? $"AgenticCodingLoop.{promptName}.md"
            : $"AgenticCodingLoop.Prompts.{promptName}.md";
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded prompt resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    internal static string Load(string promptName, params (string key, string value)[] replacements)
    {
        var text = Load(promptName);
        foreach (var (key, value) in replacements)
        {
            text = text.Replace($"{{{{{key}}}}}", value, StringComparison.Ordinal);
        }

        return text;
    }
}