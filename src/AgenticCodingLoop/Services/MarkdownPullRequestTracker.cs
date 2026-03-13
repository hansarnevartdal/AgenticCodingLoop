using System.Text;
using AgenticCodingLoop.Models;

namespace AgenticCodingLoop.Services;

public sealed class MarkdownPullRequestTracker(string basePath) : IPullRequestTracker
{
    private readonly string prsDir = Path.Combine(basePath, ".agent-loop", "pull-requests");
    private int nextId;

    public TrackedPullRequest Create(string title, string linkedIssueId, string summary)
    {
        Directory.CreateDirectory(prsDir);
        var id = $"PR-{Interlocked.Increment(ref nextId):D3}";
        var pr = new TrackedPullRequest
        {
            Id = id,
            Title = title,
            LinkedIssueId = linkedIssueId,
            Summary = summary
        };
        Save(pr);
        return pr;
    }

    public IReadOnlyList<TrackedPullRequest> List()
    {
        if (!Directory.Exists(prsDir)) { return []; }

        return Directory.GetFiles(prsDir, "PR-*.md")
            .Order()
            .Select(Parse)
            .Where(pr => pr is not null)
            .Cast<TrackedPullRequest>()
            .ToList();
    }

    public TrackedPullRequest? Get(string id)
    {
        var path = Path.Combine(prsDir, $"{id}.md");
        return File.Exists(path) ? Parse(path) : null;
    }

    public void Comment(string id, string comment)
    {
        var pr = Get(id) ?? throw new InvalidOperationException($"Pull request {id} not found.");
        pr.Comments.Add($"[{DateTime.UtcNow:u}] {comment}");
        Save(pr);
    }

    public void SetStatus(string id, PullRequestStatus status)
    {
        var pr = Get(id) ?? throw new InvalidOperationException($"Pull request {id} not found.");
        pr.Status = status;
        pr.Comments.Add($"[{DateTime.UtcNow:u}] Status changed to {status}");
        Save(pr);
    }

    public void RecordReview(string id, ReviewDecision decision, string comment)
    {
        var pr = Get(id) ?? throw new InvalidOperationException($"Pull request {id} not found.");
        pr.ReviewDecision = decision;
        pr.Status = decision is ReviewDecision.Approved
            ? PullRequestStatus.Approved
            : PullRequestStatus.NeedsWork;
        pr.Comments.Add($"[{DateTime.UtcNow:u}] Review: {decision} — {comment}");
        Save(pr);
    }

    private void Save(TrackedPullRequest pr)
    {
        Directory.CreateDirectory(prsDir);
        var sb = new StringBuilder();
        sb.AppendLine($"# {pr.Id}: {pr.Title}");
        sb.AppendLine();
        sb.AppendLine($"**Status:** {pr.Status}");
        sb.AppendLine($"**Linked Issue:** {pr.LinkedIssueId}");
        if (pr.ReviewDecision is not null)
        {
            sb.AppendLine($"**Review Decision:** {pr.ReviewDecision}");
        }
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine(pr.Summary);
        sb.AppendLine();
        if (pr.Comments.Count > 0)
        {
            sb.AppendLine("## Comments");
            sb.AppendLine();
            foreach (var c in pr.Comments)
            {
                sb.AppendLine($"- {c}");
            }
            sb.AppendLine();
        }
        File.WriteAllText(Path.Combine(prsDir, $"{pr.Id}.md"), sb.ToString());

        UpdateNextId(pr.Id);
    }

    private TrackedPullRequest? Parse(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 1) { return null; }

        var heading = lines[0];
        if (!heading.StartsWith("# ")) { return null; }

        var colonIndex = heading.IndexOf(':', StringComparison.Ordinal);
        if (colonIndex < 0) { return null; }

        var id = heading[2..colonIndex].Trim();
        var title = heading[(colonIndex + 1)..].Trim();

        var status = PullRequestStatus.Draft;
        var linkedIssueId = "";
        ReviewDecision? reviewDecision = null;
        var summary = "";
        List<string> comments = [];

        var currentSection = "";
        var sectionContent = new StringBuilder();

        void FlushSection()
        {
            var content = sectionContent.ToString().Trim();
            switch (currentSection)
            {
                case "Summary":
                    summary = content;
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
                if (Enum.TryParse<PullRequestStatus>(val.Replace(" ", ""), out var s)) { status = s; }
            }
            else if (line.StartsWith("**Linked Issue:**"))
            {
                linkedIssueId = line.Replace("**Linked Issue:**", "").Trim();
            }
            else if (line.StartsWith("**Review Decision:**"))
            {
                var val = line.Replace("**Review Decision:**", "").Trim();
                if (Enum.TryParse<ReviewDecision>(val, out var d)) { reviewDecision = d; }
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

        var pr = new TrackedPullRequest
        {
            Id = id,
            Title = title,
            LinkedIssueId = linkedIssueId,
            Summary = summary,
            Status = status,
            ReviewDecision = reviewDecision,
            Comments = comments
        };

        UpdateNextId(id);
        return pr;
    }

    private void UpdateNextId(string id)
    {
        if (id.StartsWith("PR-") && int.TryParse(id[3..], out var num))
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
