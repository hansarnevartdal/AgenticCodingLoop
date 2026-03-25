using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Features.Monitor.Tools;

internal sealed class MonitorDecisionTool
{
    private readonly int maxParallel;
    private readonly object syncRoot = new();
    private MonitorDecision? decision;

    public MonitorDecisionTool(int maxParallel = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxParallel);
        this.maxParallel = maxParallel;
    }

    public void Reset()
    {
        lock (syncRoot)
        {
            decision = null;
        }
    }

    public ICollection<AIFunction> CreateTools()
    {
        return [AIFunctionFactory.Create(ReportMonitorDecision, "report_monitor_decision", "Report the monitor's final decision about how many implementer and reviewer workers to start. Call this exactly once after inspecting the repository state and provide a short reason.")];
    }

    public string ReportMonitorDecision(
        [Description("Number of new implementer workers to start (0 to max).")]
        int implementersToStart,
        [Description("Number of new reviewer workers to start (0 to max).")]
        int reviewersToStart,
        [Description("Whether any work exists at all.")]
        bool hasAnyWork,
        [Description("Short explanation of why this decision was made.")]
        string reason)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(implementersToStart);
        ArgumentOutOfRangeException.ThrowIfNegative(reviewersToStart);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(implementersToStart, maxParallel);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(reviewersToStart, maxParallel);

        lock (syncRoot)
        {
            decision = new MonitorDecision(implementersToStart, reviewersToStart, hasAnyWork);
        }

        return string.IsNullOrWhiteSpace(reason)
            ? $"Monitor decision recorded: implementers={implementersToStart}, reviewers={reviewersToStart}, hasAnyWork={hasAnyWork}."
            : $"Monitor decision recorded: implementers={implementersToStart}, reviewers={reviewersToStart}, hasAnyWork={hasAnyWork}. Reason: {reason}";
    }

    public bool TryGetDecision(out MonitorDecision value)
    {
        lock (syncRoot)
        {
            if (decision is null)
            {
                value = new MonitorDecision(0, 0, false);
                return false;
            }

            value = decision;
            return true;
        }
    }
}