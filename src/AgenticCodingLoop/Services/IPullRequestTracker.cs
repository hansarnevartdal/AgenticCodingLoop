using AgenticCodingLoop.Models;

namespace AgenticCodingLoop.Services;

public interface IPullRequestTracker
{
    TrackedPullRequest Create(string title, string linkedIssueId, string summary);
    IReadOnlyList<TrackedPullRequest> List();
    TrackedPullRequest? Get(string id);
    void Comment(string id, string comment);
    void SetStatus(string id, PullRequestStatus status);
    void RecordReview(string id, ReviewDecision decision, string comment);
}
