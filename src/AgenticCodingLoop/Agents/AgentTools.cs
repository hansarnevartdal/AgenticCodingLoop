using System.ComponentModel;
using System.Text.Json;
using AgenticCodingLoop.Models;
using AgenticCodingLoop.Services;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Agents;

public static class AgentTools
{
    public static IReadOnlyList<AIFunction> CreateIssueTools(IIssueTracker tracker) =>
    [
        AIFunctionFactory.Create(
            ([Description("Issue title")] string title,
             [Description("Detailed description of the work")] string description,
             [Description("Specific, testable acceptance criteria")] string acceptanceCriteria) =>
            {
                var issue = tracker.Create(title, description, acceptanceCriteria);
                return JsonSerializer.Serialize(new { issue.Id, issue.Title, issue.Status });
            },
            "create_issue",
            "Create a new issue in the tracker"),

        AIFunctionFactory.Create(
            () =>
            {
                var issues = tracker.List();
                return JsonSerializer.Serialize(issues.Select(i => new { i.Id, i.Title, Status = i.Status.ToString() }));
            },
            "list_issues",
            "List all issues with their status"),

        AIFunctionFactory.Create(
            ([Description("Issue identifier (e.g., ISSUE-001)")] string id) =>
            {
                var issue = tracker.Get(id);
                return issue is null
                    ? $"Issue {id} not found."
                    : JsonSerializer.Serialize(new
                    {
                        issue.Id,
                        issue.Title,
                        issue.Description,
                        issue.AcceptanceCriteria,
                        Status = issue.Status.ToString(),
                        issue.LinkedPullRequestId,
                        issue.Comments
                    });
            },
            "read_issue",
            "Read full details of an issue"),

        AIFunctionFactory.Create(
            ([Description("Issue identifier")] string id,
             [Description("Comment text")] string comment) =>
            {
                tracker.Comment(id, comment);
                return $"Comment added to {id}.";
            },
            "comment_on_issue",
            "Add a comment to an issue"),

        AIFunctionFactory.Create(
            ([Description("Issue identifier")] string id,
             [Description("New status: Open, InProgress, ReadyForReview, Done")] string status) =>
            {
                if (!Enum.TryParse<IssueStatus>(status, ignoreCase: true, out var parsed))
                {
                    return $"Invalid status '{status}'. Valid values: Open, InProgress, ReadyForReview, Done";
                }
                tracker.SetStatus(id, parsed);
                return $"{id} status set to {parsed}.";
            },
            "set_issue_status",
            "Update the status of an issue"),
    ];

    public static IReadOnlyList<AIFunction> CreatePullRequestTools(IPullRequestTracker tracker) =>
    [
        AIFunctionFactory.Create(
            ([Description("PR title")] string title,
             [Description("Linked issue identifier (e.g., ISSUE-001)")] string linkedIssueId,
             [Description("Summary of changes made")] string summary) =>
            {
                var pr = tracker.Create(title, linkedIssueId, summary);
                return JsonSerializer.Serialize(new { pr.Id, pr.Title, pr.Status });
            },
            "create_pull_request",
            "Create a new pull request linked to an issue"),

        AIFunctionFactory.Create(
            () =>
            {
                var prs = tracker.List();
                return JsonSerializer.Serialize(prs.Select(p => new { p.Id, p.Title, Status = p.Status.ToString(), p.LinkedIssueId }));
            },
            "list_pull_requests",
            "List all pull requests with their status"),

        AIFunctionFactory.Create(
            ([Description("Pull request identifier (e.g., PR-001)")] string id) =>
            {
                var pr = tracker.Get(id);
                return pr is null
                    ? $"Pull request {id} not found."
                    : JsonSerializer.Serialize(new
                    {
                        pr.Id,
                        pr.Title,
                        pr.LinkedIssueId,
                        pr.Summary,
                        Status = pr.Status.ToString(),
                        ReviewDecision = pr.ReviewDecision?.ToString(),
                        pr.Comments
                    });
            },
            "read_pull_request",
            "Read full details of a pull request"),

        AIFunctionFactory.Create(
            ([Description("Pull request identifier")] string id,
             [Description("Comment text")] string comment) =>
            {
                tracker.Comment(id, comment);
                return $"Comment added to {id}.";
            },
            "comment_on_pull_request",
            "Add a comment to a pull request"),

        AIFunctionFactory.Create(
            ([Description("Pull request identifier")] string id,
             [Description("New status: Draft, ReadyForReview, NeedsWork, Approved, Merged")] string status) =>
            {
                if (!Enum.TryParse<PullRequestStatus>(status, ignoreCase: true, out var parsed))
                {
                    return $"Invalid status '{status}'. Valid values: Draft, ReadyForReview, NeedsWork, Approved, Merged";
                }
                tracker.SetStatus(id, parsed);
                return $"{id} status set to {parsed}.";
            },
            "set_pull_request_status",
            "Update the status of a pull request"),

        AIFunctionFactory.Create(
            ([Description("Pull request identifier")] string id,
             [Description("Review decision: Approved or ChangesRequested")] string decision,
             [Description("Review comment explaining the decision")] string comment) =>
            {
                if (!Enum.TryParse<ReviewDecision>(decision, ignoreCase: true, out var parsed))
                {
                    return $"Invalid decision '{decision}'. Valid values: Approved, ChangesRequested";
                }
                tracker.RecordReview(id, parsed, comment);
                return $"Review recorded on {id}: {parsed}";
            },
            "record_review",
            "Record a review decision on a pull request"),
    ];
}
