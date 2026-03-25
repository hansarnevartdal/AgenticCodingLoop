using AgenticCodingLoop.Features.Implementer.Tools;
using AgenticCodingLoop.Shared.Runtime;

namespace AgenticCodingLoop.Features.Implementer;

internal sealed class ImplementerFeature : IWorkerFeature
{
    private readonly WorkerLoop workerLoop;

    private ImplementerFeature(WorkerLoop workerLoop)
    {
        this.workerLoop = workerLoop;
    }

    internal static WorkerLoopDefinition Definition => new(
        AgentDisplayName: "Implementer",
        AgentMode: "implementer",
        WorkType: "implementation",
        Model: CopilotModels.Implementer,
        PromptName: "Features.Implementer.implementation-loop.prompt",
        PromptTimeout: TimeSpan.FromMinutes(60),
        AgentColor: ConsoleColor.Cyan,
        CreateRuntime: static (agentName, color) =>
        {
            var eventTool = new ImplementerEventTool(agentName, color);
            var stopTool = new ImplementationStopTool();
            var tools = new List<Microsoft.Extensions.AI.AIFunction>();
            tools.AddRange(eventTool.CreateTools());
            tools.AddRange(stopTool.CreateTools());
            return new WorkerLoopRuntime(tools, stopTool.Reset, () => stopTool.IsNoMoreWorkSignaled);
        });

    public static async Task<ImplementerFeature> CreateAsync(
        string cliPath,
        IReadOnlyDictionary<string, string> clientEnvironment,
        string worktreePath,
        string sourceGitHub,
        string sourceSkills,
        SessionDebugConsole debugConsole,
        int workerId)
    {
        var workerLoop = await WorkerLoop.CreateAsync(
            cliPath,
            clientEnvironment,
            worktreePath,
            sourceGitHub,
            sourceSkills,
            debugConsole,
            workerId,
            Definition);

        return new ImplementerFeature(workerLoop);
    }

    public Task ExecuteAsync(CancellationToken ct) => workerLoop.ExecuteAsync(ct);

    public ValueTask DisposeAsync() => workerLoop.DisposeAsync();
}