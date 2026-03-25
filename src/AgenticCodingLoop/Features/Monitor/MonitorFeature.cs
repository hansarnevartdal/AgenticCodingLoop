using AgenticCodingLoop.Features.Monitor.Tools;
using AgenticCodingLoop.Shared.Prompts;
using AgenticCodingLoop.Shared.Runtime;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Features.Monitor;

internal sealed class MonitorFeature : IAsyncDisposable
{
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(2);
    private const string AgentName = "Monitor";
    private const ConsoleColor AgentColor = ConsoleColor.Blue;

    private readonly CopilotSession session;
    private readonly MonitorDecisionTool decisionTool;
    private readonly SessionDebugConsole debugConsole;
    private readonly int maxParallel;

    private MonitorFeature(CopilotSession session, MonitorDecisionTool decisionTool, SessionDebugConsole debugConsole, int maxParallel)
    {
        this.session = session;
        this.decisionTool = decisionTool;
        this.debugConsole = debugConsole;
        this.maxParallel = maxParallel;
    }

    public static async Task<MonitorFeature> CreateAsync(CopilotClient client, string sourceSkills, MonitorWorkerStateTool workerStateTool, SessionDebugConsole debugConsole, int maxParallel)
    {
        var decisionTool = new MonitorDecisionTool(maxParallel);
        var eventTool = new MonitorEventTool(AgentName, AgentColor);
        var tools = new List<AIFunction>();
        tools.AddRange(eventTool.CreateTools());
        tools.AddRange(decisionTool.CreateTools());
        tools.AddRange(workerStateTool.CreateTools());

        var session = await client.CreateSessionAsync(new SessionConfig
        {
            Model = CopilotModels.Free,
            Tools = tools,
            SkillDirectories = [sourceSkills],
            // The monitor should stay fully automatic so it can cheaply poll without user input.
            OnPermissionRequest = PermissionHandler.ApproveAll
        });

        return new MonitorFeature(session, decisionTool, debugConsole, maxParallel);
    }

    public async Task<MonitorDecision> ExecuteAsync(CancellationToken ct)
    {
        decisionTool.Reset();

        var response = await debugConsole.SendAndReadContent(session, AgentName, AgentColor, new MessageOptions
        {
            Prompt = BuildPrompt(maxParallel)
        }, PromptTimeout, ct);

        if (!decisionTool.TryGetDecision(out var decision))
        {
            throw new InvalidOperationException($"Monitor did not report a decision via tool: {response}");
        }

        return decision;
    }

    public ValueTask DisposeAsync() => session.DisposeAsync();

    internal static string BuildPrompt(int maxParallel)
    {
        return PromptLoader.Load("Features.Monitor.monitor-loop.prompt", ("maxParallel", maxParallel.ToString()));
    }
}

internal sealed record MonitorDecision(int ImplementersToStart, int ReviewersToStart, bool HasAnyWork);