using AgenticCodingLoop.Configuration;
using System.Text.Json;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Loops;

internal sealed class MonitorLoop : IAsyncDisposable
{
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(2);

    private readonly CopilotSession session;

    private MonitorLoop(CopilotSession session)
    {
        this.session = session;
    }

    public static async Task<MonitorLoop> CreateAsync(CopilotClient client, string sourceSkills)
    {
        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Model = CopilotModels.Free,
            SkillDirectories = [sourceSkills],
            OnPermissionRequest = PermissionHandler.ApproveAll
        });

        return new MonitorLoop(session);
    }

    public async Task<MonitorDecision> CheckAsync(CancellationToken ct)
    {
        var assistantMessage = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = """
                    Check the repository state and decide whether worker loops should run.

                    1. Run `gh issue list --state open --limit 1`.
                    2. Run `gh pr list --state open --json reviewDecision,labels --limit 50`.
                    3. Set `startImplementer` to true if there is at least one open issue, or any PR with review decision `APPROVED` or `CHANGES_REQUESTED`, or labels `status:approved` or `status:needs-work`.
                    4. Set `startReviewer` to true if any PR has the label `status:ready-for-review`.
                    5. Set `hasAnyWork` to true if either worker should run.

                    Reply with compact JSON only:
                    {"startImplementer":true,"startReviewer":false,"hasAnyWork":true}
                    """
        }, PromptTimeout, ct);
        var response = assistantMessage?.Data?.Content ?? string.Empty;

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