using System.Text;
using AgenticCodingLoop.Models;

namespace AgenticCodingLoop.Services;

public sealed class MarkdownIssueTracker(string basePath) : IIssueTracker
{
    private readonly string issuesDir = Path.Combine(basePath, ".agent-loop", "issues");
    private int nextId;

    public TrackedIssue Create(string title, string description, string acceptanceCriteria)
    {
        Directory.CreateDirectory(issuesDir);
        var id = $"ISSUE-{Interlocked.Increment(ref nextId):D3}";
        var issue = new TrackedIssue
        {
            Id = id,
            Title = title,
            Description = description,
            AcceptanceCriteria = acceptanceCriteria
        };
        Save(issue);
        return issue;
    }

    public IReadOnlyList<TrackedIssue> List()
    {
        if (!Directory.Exists(issuesDir)) { return []; }

        return Directory.GetFiles(issuesDir, "ISSUE-*.md")
            .Order()
            .Select(Parse)
            .Where(i => i is not null)
            .Cast<TrackedIssue>()
            .ToList();
    }

    public TrackedIssue? Get(string id)
    {
        var path = Path.Combine(issuesDir, $"{id}.md");
        return File.Exists(path) ? Parse(path) : null;
    }

    public void Comment(string id, string comment)
    {
        var issue = Get(id) ?? throw new InvalidOperationException($"Issue {id} not found.");
        issue.Comments.Add($"[{DateTime.UtcNow:u}] {comment}");
        Save(issue);
    }

    public void SetStatus(string id, IssueStatus status)
    {
        var issue = Get(id) ?? throw new InvalidOperationException($"Issue {id} not found.");
        issue.Status = status;
        issue.Comments.Add($"[{DateTime.UtcNow:u}] Status changed to {status}");
        Save(issue);
    }

    private void Save(TrackedIssue issue)
    {
        Directory.CreateDirectory(issuesDir);
        var sb = new StringBuilder();
        sb.AppendLine($"# {issue.Id}: {issue.Title}");
        sb.AppendLine();
        sb.AppendLine($"**Status:** {issue.Status}");
        if (issue.LinkedPullRequestId is not null)
        {
            sb.AppendLine($"**Linked PR:** {issue.LinkedPullRequestId}");
        }
        sb.AppendLine();
        sb.AppendLine("## Description");
        sb.AppendLine();
        sb.AppendLine(issue.Description);
        sb.AppendLine();
        sb.AppendLine("## Acceptance Criteria");
        sb.AppendLine();
        sb.AppendLine(issue.AcceptanceCriteria);
        sb.AppendLine();
        if (issue.Comments.Count > 0)
        {
            sb.AppendLine("## Comments");
            sb.AppendLine();
            foreach (var c in issue.Comments)
            {
                sb.AppendLine($"- {c}");
            }
            sb.AppendLine();
        }
        File.WriteAllText(Path.Combine(issuesDir, $"{issue.Id}.md"), sb.ToString());

        UpdateNextId(issue.Id);
    }

    private TrackedIssue? Parse(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 1) { return null; }

        var heading = lines[0];
        if (!heading.StartsWith("# ")) { return null; }

        var colonIndex = heading.IndexOf(':', StringComparison.Ordinal);
        if (colonIndex < 0) { return null; }

        var id = heading[2..colonIndex].Trim();
        var title = heading[(colonIndex + 1)..].Trim();

        var status = IssueStatus.Open;
        string? linkedPr = null;
        var description = "";
        var acceptanceCriteria = "";
        List<string> comments = [];

        var currentSection = "";
        var sectionContent = new StringBuilder();

        void FlushSection()
        {
            var content = sectionContent.ToString().Trim();
            switch (currentSection)
            {
                case "Description":
                    description = content;
                    break;
                case "Acceptance Criteria":
                    acceptanceCriteria = content;
                    break;
                case "Comments":
                    comments = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.TrimStart('-', ' '))
                        .Where(l => l.Length > 0)
                        .ToList();
                    break;
            }
            sectionContent.Clear();
        }

        foreach (var line in lines.Skip(1))
        {
            if (line.StartsWith("**Status:**"))
            {
                var val = line.Replace("**Status:**", "").Trim();
                if (Enum.TryParse<IssueStatus>(val.Replace(" ", ""), out var s)) { status = s; }
            }
            else if (line.StartsWith("**Linked PR:**"))
            {
                linkedPr = line.Replace("**Linked PR:**", "").Trim();
            }
            else if (line.StartsWith("## "))
            {
                FlushSection();
                currentSection = line[3..].Trim();
            }
            else
            {
                sectionContent.AppendLine(line);
            }
        }
        FlushSection();

        var issue = new TrackedIssue
        {
            Id = id,
            Title = title,
            Description = description,
            AcceptanceCriteria = acceptanceCriteria,
            Status = status,
            LinkedPullRequestId = linkedPr,
            Comments = comments
        };

        UpdateNextId(id);
        return issue;
    }

    private void UpdateNextId(string id)
    {
        if (id.StartsWith("ISSUE-") && int.TryParse(id[6..], out var num))
        {
            int current;
            do
            {
                current = nextId;
                if (num <= current) { break; }
            }
            while (Interlocked.CompareExchange(ref nextId, num, current) != current);
        }
    }
}
