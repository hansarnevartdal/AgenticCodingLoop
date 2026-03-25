using AgenticCodingLoop.Features.Reviewer.Tools;
using AgenticCodingLoop.Shared.Runtime;

namespace AgenticCodingLoop.Features.Reviewer;

internal sealed class ReviewerFeature : IWorkerFeature
{
    private readonly WorkerLoop workerLoop;

    private ReviewerFeature(WorkerLoop workerLoop)
    {
        this.workerLoop = workerLoop;
    }

    internal static WorkerLoopDefinition Definition => new(
        AgentDisplayName: "Reviewer",
        AgentMode: "reviewer",
        WorkType: "review",
        Model: CopilotModels.Reviewer,
        PromptName: "Features.Reviewer.review-loop.prompt",
        PromptTimeout: TimeSpan.FromMinutes(60),
        AgentColor: ConsoleColor.Magenta,
        CreateRuntime: static (agentName, color) =>
        {
            var eventTool = new ReviewerEventTool(agentName, color);
            var stopTool = new ReviewStopTool();
            var tools = new List<Microsoft.Extensions.AI.AIFunction>();
            tools.AddRange(eventTool.CreateTools());
            tools.AddRange(stopTool.CreateTools());
            return new WorkerLoopRuntime(tools, stopTool.Reset, () => stopTool.IsNoMoreWorkSignaled);
        });

    public static async Task<ReviewerFeature> CreateAsync(
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

        return new ReviewerFeature(workerLoop);
    }

    public Task ExecuteAsync(CancellationToken ct) => workerLoop.ExecuteAsync(ct);

    public ValueTask DisposeAsync() => workerLoop.DisposeAsync();
}