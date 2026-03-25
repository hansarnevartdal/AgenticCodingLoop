using AgenticCodingLoop.Features.Implementer.Tools;
using AgenticCodingLoop.Features.Reviewer.Tools;

namespace AgenticCodingLoop.Tests;

public sealed class When_requesting_loop_stop
{
    [Fact]
    public void Should_mark_that_no_more_work_is_available()
    {
        var signal = new ImplementationStopTool();

        var message = signal.SignalNoMoreWork("No more issues remain.");

        Assert.True(signal.IsNoMoreWorkSignaled);
        Assert.Contains("No more implementation work is currently available", message, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_clear_the_signal_on_reset()
    {
        var signal = new ReviewStopTool();
        signal.SignalNoMoreWork("No more PRs remain.");

        signal.Reset();

        Assert.False(signal.IsNoMoreWorkSignaled);
    }
}