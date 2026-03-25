using System.ComponentModel;
using AgenticCodingLoop.Shared.Runtime;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Features.Reviewer.Tools;

internal sealed class ReviewerEventTool(string agentName, ConsoleColor color)
{
    public ICollection<AIFunction> CreateTools()
    {
        return [AIFunctionFactory.Create(ReportKeyEvent, "report_key_event", "Report a major milestone or finding for the main console log. Use this only for concise, high-signal events.")];
    }

    public string ReportKeyEvent(
        [Description("Short event type such as picked-pr, requested-changes, approved, commented, or idle.")] string eventType,
        [Description("Concise summary of the event for the main console log.")] string message)
    {
        var normalizedEventType = Normalize(eventType, "event");
        var normalizedMessage = Normalize(message, "(no details)");

        lock (ConsoleWriteLock.SyncRoot)
        {
            var previousColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {agentName} {normalizedEventType}: {normalizedMessage}");
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        return $"Logged {agentName} event '{normalizedEventType}': {normalizedMessage}";
    }

    private static string Normalize(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return value.Trim().ReplaceLineEndings(" ");
    }
}