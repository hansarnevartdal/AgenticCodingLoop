using GitHub.Copilot.SDK;

namespace AgenticCodingLoop;

internal sealed class SessionDebugConsole(bool enabled)
{
    private static readonly object SyncRoot = new();

    public bool IsEnabled => enabled;

    public async Task<string> SendAndReadContent(
        CopilotSession session,
        string agentName,
        ConsoleColor color,
        MessageOptions options,
        TimeSpan timeout,
        CancellationToken ct)
    {
        if (enabled)
        {
            WriteBlock(agentName, color, "prompt", options.Prompt ?? string.Empty);
        }

        var response = await session.SendAndWaitAsync(options, timeout, ct);
        var content = response?.Data?.Content ?? string.Empty;

        if (enabled)
        {
            WriteBlock(agentName, color, "response", content);
        }

        return content;
    }

    private static void WriteBlock(string agentName, ConsoleColor color, string label, string content)
    {
        lock (SyncRoot)
        {
            var previousColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {agentName} {label}");

                foreach (var line in Normalize(content).Split('\n'))
                {
                    Console.WriteLine($"    {line}");
                }

                Console.WriteLine();
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }
    }

    private static string Normalize(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "(empty)";
        }

        return content.TrimEnd().ReplaceLineEndings("\n");
    }
}