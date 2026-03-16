using System.ComponentModel;
using System.Threading;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Loops;

internal sealed class LoopStopSignal(string workType)
{
    private int noMoreWorkSignaled;

    public bool IsNoMoreWorkSignaled => Volatile.Read(ref noMoreWorkSignaled) is 1;

    public void Reset()
    {
        Interlocked.Exchange(ref noMoreWorkSignaled, 0);
    }

    public ICollection<AIFunction> CreateTools()
    {
        return [AIFunctionFactory.Create(SignalNoMoreWork, "signal_no_more_work", $"Report that no more {workType} work is currently available. Use this when this worker should go idle until new {workType} work appears. Provide a short reason.")];
    }

    public string SignalNoMoreWork([Description("Why no more work of this type is currently available.")] string reason)
    {
        Interlocked.Exchange(ref noMoreWorkSignaled, 1);

        return string.IsNullOrWhiteSpace(reason)
            ? $"No more {workType} work is currently available. This worker can go idle until new work appears."
            : $"No more {workType} work is currently available. This worker can go idle until new work appears. Reason: {reason}";
    }
}