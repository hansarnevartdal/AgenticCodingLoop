using AgenticCodingLoop.Models;

namespace AgenticCodingLoop.Services;

public interface IIssueTracker
{
    TrackedIssue Create(string title, string description, string acceptanceCriteria);
    IReadOnlyList<TrackedIssue> List();
    TrackedIssue? Get(string id);
    void Comment(string id, string comment);
    void SetStatus(string id, IssueStatus status);
}
