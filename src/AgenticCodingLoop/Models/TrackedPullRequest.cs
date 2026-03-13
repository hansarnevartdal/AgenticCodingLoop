namespace AgenticCodingLoop.Models;

public sealed record TrackedPullRequest
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string LinkedIssueId { get; init; }
    public required string Summary { get; init; }
    public PullRequestStatus Status { get; set; } = PullRequestStatus.Draft;
    public ReviewDecision? ReviewDecision { get; set; }
    public List<string> Comments { get; init; } = [];
}
