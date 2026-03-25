using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Features.Monitor.Tools;

internal sealed class MonitorWorkerStateTool
{
    private int implementerCount;
    private int reviewerCount;

    public int ImplementerCount => Volatile.Read(ref implementerCount);

    public int ReviewerCount => Volatile.Read(ref reviewerCount);

    public ICollection<AIFunction> CreateTools()
    {
        return [AIFunctionFactory.Create(GetWorkerLoopState, "get_worker_loop_state", "Get the current host state showing how many implementer and reviewer workers are running. Call this before deciding how many new workers to request so you do not exceed the maximum parallel capacity.")];
    }

    public WorkerLoopState GetWorkerLoopState()
    {
        return new WorkerLoopState(ImplementerCount, ReviewerCount);
    }

    public void IncrementImplementer() => Interlocked.Increment(ref implementerCount);

    public void DecrementImplementer() => Interlocked.Decrement(ref implementerCount);

    public void IncrementReviewer() => Interlocked.Increment(ref reviewerCount);

    public void DecrementReviewer() => Interlocked.Decrement(ref reviewerCount);
}

internal sealed record WorkerLoopState(int ImplementerCount, int ReviewerCount);