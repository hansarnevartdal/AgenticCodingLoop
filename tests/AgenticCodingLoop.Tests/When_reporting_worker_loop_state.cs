using AgenticCodingLoop.Features.Monitor.Tools;

namespace AgenticCodingLoop.Tests;

public sealed class When_reporting_worker_loop_state
{
    [Fact]
    public void Should_report_both_workers_as_idle_by_default()
    {
        var signal = new MonitorWorkerStateTool();

        var state = signal.GetWorkerLoopState();

        Assert.Equal(new WorkerLoopState(0, 0), state);
    }

    [Fact]
    public void Should_report_each_worker_count_after_increment()
    {
        var signal = new MonitorWorkerStateTool();
        signal.IncrementImplementer();
        signal.IncrementReviewer();

        var state = signal.GetWorkerLoopState();

        Assert.Equal(new WorkerLoopState(1, 1), state);
    }

    [Fact]
    public void Should_report_zero_after_increment_then_decrement()
    {
        var signal = new MonitorWorkerStateTool();
        signal.IncrementImplementer();
        signal.IncrementReviewer();

        signal.DecrementImplementer();
        signal.DecrementReviewer();

        var state = signal.GetWorkerLoopState();

        Assert.Equal(new WorkerLoopState(0, 0), state);
    }

    [Fact]
    public void Should_track_multiple_concurrent_workers()
    {
        var signal = new MonitorWorkerStateTool();
        signal.IncrementImplementer();
        signal.IncrementImplementer();
        signal.IncrementReviewer();
        signal.IncrementReviewer();
        signal.IncrementReviewer();

        var state = signal.GetWorkerLoopState();

        Assert.Equal(new WorkerLoopState(2, 3), state);

        signal.DecrementImplementer();
        signal.DecrementReviewer();

        state = signal.GetWorkerLoopState();

        Assert.Equal(new WorkerLoopState(1, 2), state);
    }
}