using AgenticCodingLoop.Agents;
using AgenticCodingLoop.Models;
using AgenticCodingLoop.Services;
using GitHub.Copilot.SDK;

// ── Validate inputs ──────────────────────────────────────────────────────────
if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: AgenticCodingLoop <repositoryPath> <prdPath>");
    return 1;
}

var repositoryPath = Path.GetFullPath(args[0]);
var prdPath = Path.GetFullPath(args[1]);

if (!Directory.Exists(repositoryPath))
{
    Console.Error.WriteLine($"Repository path not found: {repositoryPath}");
    return 1;
}

if (!File.Exists(prdPath))
{
    Console.Error.WriteLine($"PRD file not found: {prdPath}");
    return 1;
}

Console.WriteLine($"Repository: {repositoryPath}");
Console.WriteLine($"PRD:        {prdPath}");
Console.WriteLine();

// ── Initialize trackers ──────────────────────────────────────────────────────
var issueTracker = new MarkdownIssueTracker(repositoryPath);
var prTracker = new MarkdownPullRequestTracker(repositoryPath);

var issueTools = AgentTools.CreateIssueTools(issueTracker);
var prTools = AgentTools.CreatePullRequestTools(prTracker);
var allTools = issueTools.Concat(prTools).ToList();

// ── Start Copilot client ─────────────────────────────────────────────────────
await using var client = new CopilotClient(new CopilotClientOptions
{
    Cwd = repositoryPath
});
await client.StartAsync();

Console.WriteLine("Copilot client started.");
Console.WriteLine();

// ── Helper: send a prompt and wait for idle ──────────────────────────────────
async Task<string> SendAndWait(CopilotSession session, string prompt)
{
    var done = new TaskCompletionSource();
    var response = "";

    using var subscription = session.On(evt =>
    {
        switch (evt)
        {
            case AssistantMessageEvent msg:
                response = msg.Data.Content;
                break;
            case SessionErrorEvent err:
                Console.Error.WriteLine($"  [Error] {err.Data.Message}");
                done.TrySetResult();
                break;
            case SessionIdleEvent:
                done.TrySetResult();
                break;
        }
    });

    await session.SendAsync(new MessageOptions { Prompt = prompt });
    await done.Task;
    return response;
}

// ── Helper: ask_user handler for CLI interaction ─────────────────────────────
Task<UserInputResponse> HandleUserInput(UserInputRequest request, UserInputInvocation _)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"  Agent asks: {request.Question}");
    Console.ResetColor();

    if (request.Choices?.Count > 0)
    {
        for (var i = 0; i < request.Choices.Count; i++)
        {
            Console.WriteLine($"    [{i + 1}] {request.Choices[i]}");
        }
    }

    Console.Write("  Your answer: ");
    var answer = Console.ReadLine() ?? "";

    return Task.FromResult(new UserInputResponse
    {
        Answer = answer,
        WasFreeform = true
    });
}

// ── Phase 1: Planner — refine PRD and create issues ─────────────────────────
Console.WriteLine("═══ Phase 1: Planning ═══");
Console.WriteLine();

await using var plannerSession = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5.4",
    SystemMessage = new SystemMessageConfig
    {
        Mode = SystemMessageMode.Append,
        Content = AgentPrompts.Planner
    },
    Tools = allTools,
    OnUserInputRequest = HandleUserInput
});

var prdContent = await File.ReadAllTextAsync(prdPath);

var planResult = await SendAndWait(plannerSession,
    $"""
    Here is the PRD to analyze and plan:

    ---
    {prdContent}
    ---

    The target repository is at: {repositoryPath}

    Please:
    1. Analyze this PRD and ask any clarifying questions if needed.
    2. Create a refined execution plan.
    3. Break it into ordered issues using the create_issue tool.
    4. Summarize the plan when done.
    """);

Console.WriteLine();
Console.WriteLine(planResult);
Console.WriteLine();

// Save the refined plan
var plansDir = Path.Combine(repositoryPath, ".agent-loop", "plans");
Directory.CreateDirectory(plansDir);
await File.WriteAllTextAsync(
    Path.Combine(plansDir, "refined-plan.md"),
    $"# Refined Plan\n\n{planResult}\n");

// ── Phase 2: Process issues sequentially ─────────────────────────────────────
Console.WriteLine("═══ Phase 2: Implementation ═══");
Console.WriteLine();

// Create implementer session (GPT-5.4 for coding)
await using var implementerSession = await client.CreateSessionAsync(new SessionConfig
{
    Model = "gpt-5.4",
    SystemMessage = new SystemMessageConfig
    {
        Mode = SystemMessageMode.Append,
        Content = AgentPrompts.Implementer
    },
    Tools = allTools
});

// Create reviewer session (Claude Opus 4.6 for review quality)
await using var reviewerSession = await client.CreateSessionAsync(new SessionConfig
{
    Model = "claude-opus-4.6",
    SystemMessage = new SystemMessageConfig
    {
        Mode = SystemMessageMode.Append,
        Content = AgentPrompts.Reviewer
    },
    Tools = allTools
});

var issues = issueTracker.List();
Console.WriteLine($"Processing {issues.Count} issue(s) sequentially.");
Console.WriteLine();

foreach (var issue in issues)
{
    Console.WriteLine($"─── Working on {issue.Id}: {issue.Title} ───");
    Console.WriteLine();

    // ── Implementer: work the issue ──────────────────────────────────────
    var implResult = await SendAndWait(implementerSession,
        $"""
        Work on the following issue:

        Issue ID: {issue.Id}
        Title: {issue.Title}
        Description: {issue.Description}
        Acceptance Criteria: {issue.AcceptanceCriteria}

        The target repository is at: {repositoryPath}

        Please:
        1. Set the issue status to InProgress.
        2. Implement the required changes.
        3. Create a pull request for your work.
        4. Set the PR status to ReadyForReview.
        5. Set the issue status to ReadyForReview.
        """);

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"  Implementer: {Truncate(implResult)}");
    Console.ResetColor();
    Console.WriteLine();

    // ── Review loop ──────────────────────────────────────────────────────
    const int maxReviewCycles = 3;
    var merged = false;

    for (var cycle = 1; cycle <= maxReviewCycles; cycle++)
    {
        // Find the PR linked to this issue
        var prs = prTracker.List();
        var activePr = prs.FirstOrDefault(p =>
            p.LinkedIssueId == issue.Id &&
            p.Status is not PullRequestStatus.Merged);

        if (activePr is null)
        {
            Console.WriteLine("  No active PR found for this issue. Skipping review.");
            break;
        }

        Console.WriteLine($"  Review cycle {cycle} for {activePr.Id}...");

        // ── Reviewer: review the PR ──────────────────────────────────
        var reviewResult = await SendAndWait(reviewerSession,
            $"""
            Review the following pull request:

            PR ID: {activePr.Id}
            Title: {activePr.Title}
            Linked Issue: {activePr.LinkedIssueId}
            Summary: {activePr.Summary}

            The target repository is at: {repositoryPath}

            Please:
            1. Read the issue requirements using read_issue.
            2. Review the code changes in the repository.
            3. Use record_review to approve or request changes.
            """);

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"  Reviewer: {Truncate(reviewResult)}");
        Console.ResetColor();
        Console.WriteLine();

        // Reload PR to check decision
        var reloadedPr = prTracker.Get(activePr.Id);
        if (reloadedPr?.ReviewDecision is ReviewDecision.Approved)
        {
            // Merge the PR and mark issue done
            prTracker.SetStatus(reloadedPr.Id, PullRequestStatus.Merged);
            issueTracker.SetStatus(issue.Id, IssueStatus.Done);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ {reloadedPr.Id} merged. {issue.Id} marked Done.");
            Console.ResetColor();
            Console.WriteLine();
            merged = true;
            break;
        }

        // Changes requested — send back to implementer
        Console.WriteLine("  Changes requested. Sending back to implementer...");

        var fixResult = await SendAndWait(implementerSession,
            $"""
            The reviewer has requested changes on {activePr!.Id} for issue {issue.Id}.

            Please:
            1. Read the PR comments using read_pull_request to understand the feedback.
            2. Make the required changes in the repository.
            3. Comment on the PR describing what you changed.
            4. Set the PR status back to ReadyForReview.
            """);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  Implementer (fix): {Truncate(fixResult)}");
        Console.ResetColor();
        Console.WriteLine();
    }

    if (!merged)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {issue.Id} could not be merged after {maxReviewCycles} review cycles.");
        Console.ResetColor();
        Console.WriteLine();
    }
}

// ── Phase 3: Final summary ───────────────────────────────────────────────────
Console.WriteLine("═══ Final Summary ═══");
Console.WriteLine();

var allIssues = issueTracker.List();
var allPrs = prTracker.List();

foreach (var issue in allIssues)
{
    var statusColor = issue.Status is IssueStatus.Done ? ConsoleColor.Green : ConsoleColor.Yellow;
    Console.ForegroundColor = statusColor;
    Console.WriteLine($"  {issue.Id}: {issue.Title} — {issue.Status}");
    Console.ResetColor();
}

Console.WriteLine();

foreach (var pr in allPrs)
{
    var statusColor = pr.Status is PullRequestStatus.Merged ? ConsoleColor.Green : ConsoleColor.Yellow;
    Console.ForegroundColor = statusColor;
    Console.WriteLine($"  {pr.Id}: {pr.Title} — {pr.Status} (Review: {pr.ReviewDecision?.ToString() ?? "Pending"})");
    Console.ResetColor();
}

Console.WriteLine();

// Save session summary
var logsDir = Path.Combine(repositoryPath, ".agent-loop", "logs");
Directory.CreateDirectory(logsDir);
await File.WriteAllTextAsync(
    Path.Combine(logsDir, "session-summary.md"),
    $"""
    # Session Summary

    **Date:** {DateTime.UtcNow:u}
    **Repository:** {repositoryPath}
    **PRD:** {prdPath}

    ## Issues

    | ID | Title | Status |
    |----|-------|--------|
    {string.Join('\n', allIssues.Select(i => $"| {i.Id} | {i.Title} | {i.Status} |"))}

    ## Pull Requests

    | ID | Title | Status | Review |
    |----|-------|--------|--------|
    {string.Join('\n', allPrs.Select(p => $"| {p.Id} | {p.Title} | {p.Status} | {p.ReviewDecision?.ToString() ?? "—"} |"))}
    """);

var doneCount = allIssues.Count(i => i.Status is IssueStatus.Done);
Console.WriteLine($"Completed {doneCount}/{allIssues.Count} issues.");
Console.WriteLine("Session artifacts saved to .agent-loop/");

return doneCount == allIssues.Count ? 0 : 1;

static string Truncate(string text, int maxLength = 200)
{
    if (string.IsNullOrEmpty(text)) { return "(no response)"; }
    return text.Length <= maxLength ? text : $"{text[..maxLength]}...";
}
