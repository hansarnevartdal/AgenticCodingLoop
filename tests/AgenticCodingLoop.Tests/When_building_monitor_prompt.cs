using AgenticCodingLoop.Features.Monitor;

namespace AgenticCodingLoop.Tests;

public sealed class When_building_monitor_prompt
{
    [Fact]
    public void Should_only_start_reviewers_for_ready_for_review_pull_requests()
    {
        var prompt = MonitorFeature.BuildPrompt(maxParallel: 2);

        Assert.Contains("reviewersToStart", prompt, StringComparison.Ordinal);
        Assert.Contains("Do not start a reviewer for PRs that are approved, marked `status:needs-work`, or otherwise not ready for review yet.", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_include_max_parallel_in_prompt()
    {
        var prompt = MonitorFeature.BuildPrompt(maxParallel: 3);

        Assert.Contains("3", prompt, StringComparison.Ordinal);
        Assert.Contains("maximum number of parallel workers per type is 3", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_instruct_to_exclude_claimed_items()
    {
        var prompt = MonitorFeature.BuildPrompt(maxParallel: 1);

        Assert.Contains("claimed", prompt, StringComparison.Ordinal);
        Assert.Contains("Exclude any issue or PR that has the `claimed` label", prompt, StringComparison.Ordinal);
    }
}