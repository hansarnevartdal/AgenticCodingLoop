using AgenticCodingLoop;
using AgenticCodingLoop.Configuration;
using System.Text.Json;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Loops;

internal sealed class MonitorLoop : IAsyncDisposable
{
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(2);
    private const string AgentName = "Monitor";
    private const ConsoleColor AgentColor = ConsoleColor.Blue;

    private readonly CopilotSession session;
    private readonly SessionDebugConsole debugConsole;

    private MonitorLoop(CopilotSession session, SessionDebugConsole debugConsole)
    {
        this.session = session;
        this.debugConsole = debugConsole;
    }

    public static async Task<MonitorLoop> CreateAsync(CopilotClient client, string sourceSkills, SessionDebugConsole debugConsole)
    {
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Model = CopilotModels.Free,
            SkillDirectories = [sourceSkills],
            OnPermissionRequest = PermissionHandler.ApproveAll
        });

        return new MonitorLoop(session, debugConsole);
    }

    public async Task<MonitorDecision> CheckAsync(CancellationToken ct)
    {
        var response = await debugConsole.SendAndReadContent(session, AgentName, AgentColor, new MessageOptions
        {
            Prompt = """
                    Check the repository state and decide whether worker loops should run.

                    1. Run `gh issue list --state open --json number,title --limit 50`.
                    2. Run `gh pr list --state open --json number,title,isDraft,reviewDecision,labels,closingIssuesReferences --limit 50`.
                    3. For each open PR that is not approved, inspect its reviews, review comments, issue comments, and unresolved review threads. Use `gh pr view <number> --json reviews,comments,latestReviews,reviewDecision` and any other GitHub CLI queries you need.
                    4. Treat human comments, bot comments, review comments, and unresolved review threads as implementation feedback if they ask for changes, point out a defect, or remain unaddressed. Do not require the PR to have review decision `CHANGES_REQUESTED` or label `status:needs-work` before treating that PR as implementer work.
                    5. Determine whether each open issue already has an active PR. Treat an open PR as active work for an issue if the PR closes that issue, references that issue, or is clearly the PR for that issue.
                    6. Set `startImplementer` to true only if at least one of these is true:
                       - there is an open issue that does not already have an active PR
                       - any open PR is approved and ready to merge
                       - any open PR has review decision `CHANGES_REQUESTED`
                       - any open PR has label `status:approved` or `status:needs-work`
                       - any non-approved open PR has unresolved or unanswered feedback from humans or bots that still requires code or PR updates
                    7. Do not start the implementer only because an issue is still open when that issue is already represented by an open PR that is simply waiting for review with no unresolved feedback.
                    8. Set `startReviewer` to true if there is any open pull request at all. Treat the label `status:ready-for-review` as an especially clear signal, but do not require that label before starting the reviewer.
                    9. Set `hasAnyWork` to true if either worker should run.

                    Reply with compact JSON only:
                    {"startImplementer":true,"startReviewer":false,"hasAnyWork":true}
                    """
        }, PromptTimeout, ct);

        return Parse(response);
    }

    public ValueTask DisposeAsync()
    {
        return session.DisposeAsync();
    }

    private static MonitorDecision Parse(string response)
    {
        var jsonStart = response.IndexOf('{');
        var jsonEnd = response.LastIndexOf('}');

        if (jsonStart < 0 || jsonEnd <= jsonStart)
        {
            throw new InvalidOperationException($"Monitor did not return JSON: {response}");
        }

        var json = response[jsonStart..(jsonEnd + 1)];
        var decision = JsonSerializer.Deserialize<MonitorDecision>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (decision is null)
        {
            throw new InvalidOperationException($"Monitor returned invalid JSON: {response}");
        }

        return decision;
    }
}

internal sealed record MonitorDecision(bool StartImplementer, bool StartReviewer, bool HasAnyWork);