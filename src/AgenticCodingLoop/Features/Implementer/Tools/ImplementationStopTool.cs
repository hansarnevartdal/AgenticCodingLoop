using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Features.Implementer.Tools;

internal sealed class ImplementationStopTool
{
    private int noMoreWorkSignaled;

    public bool IsNoMoreWorkSignaled => Volatile.Read(ref noMoreWorkSignaled) is 1;

    public void Reset()
    {
        Interlocked.Exchange(ref noMoreWorkSignaled, 0);
    }

    public ICollection<AIFunction> CreateTools()
    {
        return [AIFunctionFactory.Create(SignalNoMoreWork, "signal_no_more_implementation_work", "Report that no more implementation work is currently available. Use this when this worker should go idle until new implementation work appears. Provide a short reason.")];
    }

    public string SignalNoMoreWork([Description("Why no more implementation work is currently available.")] string reason)
    {
        Interlocked.Exchange(ref noMoreWorkSignaled, 1);

        return string.IsNullOrWhiteSpace(reason)
            ? "No more implementation work is currently available. This worker can go idle until new work appears."
            : $"No more implementation work is currently available. This worker can go idle until new work appears. Reason: {reason}";
    }
}