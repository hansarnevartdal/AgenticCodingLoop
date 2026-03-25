using AgenticCodingLoop.Features.Monitor;
using AgenticCodingLoop.Features.Monitor.Tools;

namespace AgenticCodingLoop.Tests;

public sealed class When_reporting_monitor_decision
{
    [Fact]
    public void Should_capture_the_reported_decision()
    {
        // Arrange
        var signal = new MonitorDecisionTool(maxParallel: 3);

        // Act
        var message = signal.ReportMonitorDecision(implementersToStart: 1, reviewersToStart: 0, hasAnyWork: true, reason: "An open issue needs implementation.");

        // Assert
        Assert.True(signal.TryGetDecision(out var decision));
        Assert.Equal(new MonitorDecision(1, 0, true), decision);
        Assert.Contains("implementers=1", message, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_capture_multiple_workers_in_decision()
    {
        // Arrange
        var signal = new MonitorDecisionTool(maxParallel: 3);

        // Act
        signal.ReportMonitorDecision(implementersToStart: 2, reviewersToStart: 3, hasAnyWork: true, reason: "Multiple issues and PRs need work.");

        // Assert
        Assert.True(signal.TryGetDecision(out var decision));
        Assert.Equal(new MonitorDecision(2, 3, true), decision);
    }

    [Fact]
    public void Should_clear_the_recorded_decision_on_reset()
    {
        // Arrange
        var signal = new MonitorDecisionTool(maxParallel: 3);
        signal.ReportMonitorDecision(implementersToStart: 0, reviewersToStart: 1, hasAnyWork: true, reason: "An open PR needs review.");

        // Act
        signal.Reset();

        // Assert
        Assert.False(signal.TryGetDecision(out _));
    }

    [Fact]
    public void Should_reject_negative_implementer_count()
    {
        // Arrange
        var signal = new MonitorDecisionTool(maxParallel: 3);

        // Act
        var action = () => signal.ReportMonitorDecision(implementersToStart: -1, reviewersToStart: 0, hasAnyWork: false, reason: "invalid");

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void Should_reject_negative_reviewer_count()
    {
        // Arrange
        var signal = new MonitorDecisionTool(maxParallel: 3);

        // Act
        var action = () => signal.ReportMonitorDecision(implementersToStart: 0, reviewersToStart: -1, hasAnyWork: false, reason: "invalid");

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void Should_reject_implementer_count_above_max_parallel()
    {
        // Arrange
        var signal = new MonitorDecisionTool(maxParallel: 2);

        // Act
        var action = () => signal.ReportMonitorDecision(implementersToStart: 3, reviewersToStart: 0, hasAnyWork: true, reason: "invalid");

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void Should_reject_reviewer_count_above_max_parallel()
    {
        // Arrange
        var signal = new MonitorDecisionTool(maxParallel: 2);

        // Act
        var action = () => signal.ReportMonitorDecision(implementersToStart: 0, reviewersToStart: 3, hasAnyWork: true, reason: "invalid");

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
}