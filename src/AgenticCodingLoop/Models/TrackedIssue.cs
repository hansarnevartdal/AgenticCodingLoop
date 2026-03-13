namespace AgenticCodingLoop.Models;

public sealed record TrackedIssue
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string AcceptanceCriteria { get; init; }
    public IssueStatus Status { get; set; } = IssueStatus.Open;
    public string? LinkedPullRequestId { get; set; }
    public List<string> Comments { get; init; } = [];
}
