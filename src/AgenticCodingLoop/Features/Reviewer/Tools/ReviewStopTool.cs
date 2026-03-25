using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Features.Reviewer.Tools;

internal sealed class ReviewStopTool
{
    private int noMoreWorkSignaled;

    public bool IsNoMoreWorkSignaled => Volatile.Read(ref noMoreWorkSignaled) is 1;

    public void Reset()
    {
        Interlocked.Exchange(ref noMoreWorkSignaled, 0);
    }

    public ICollection<AIFunction> CreateTools()
    {
        return [AIFunctionFactory.Create(SignalNoMoreWork, "signal_no_more_review_work", "Report that no more review work is currently available. Use this when this worker should go idle until new review work appears. Provide a short reason.")];
    }

    public string SignalNoMoreWork([Description("Why no more review work is currently available.")] string reason)
    {
        Interlocked.Exchange(ref noMoreWorkSignaled, 1);

        return string.IsNullOrWhiteSpace(reason)
            ? "No more review work is currently available. This worker can go idle until new work appears."
            : $"No more review work is currently available. This worker can go idle until new work appears. Reason: {reason}";
    }
}