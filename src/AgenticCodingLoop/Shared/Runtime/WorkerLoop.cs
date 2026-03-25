using AgenticCodingLoop.Shared.Prompts;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Shared.Runtime;

internal sealed class WorkerLoop : IAsyncDisposable
{
    private const int MaxIterationsPerExecution = 10;
    private const string ReasoningEffort = "high";

    private readonly CopilotClient client;
    private readonly CopilotSession session;
    private readonly SessionDebugConsole debugConsole;
    private readonly WorkerLoopDefinition definition;
    private readonly WorkerLoopRuntime runtime;
    private readonly string agentName;

    private WorkerLoop(
        CopilotClient client,
        CopilotSession session,
        SessionDebugConsole debugConsole,
        WorkerLoopDefinition definition,
        WorkerLoopRuntime runtime,
        string agentName)
    {
        this.client = client;
        this.session = session;
        this.debugConsole = debugConsole;
        this.definition = definition;
        this.runtime = runtime;
        this.agentName = agentName;
    }

    public static async Task<WorkerLoop> CreateAsync(
        string cliPath,
        IReadOnlyDictionary<string, string> clientEnvironment,
        string worktreePath,
        string sourceGitHub,
        string sourceSkills,
        SessionDebugConsole debugConsole,
        int workerId,
        WorkerLoopDefinition definition)
    {
        var agentName = $"{definition.AgentDisplayName}-{workerId}";
        var runtime = definition.CreateRuntime(agentName, definition.AgentColor);

        var client = new CopilotClient(new CopilotClientOptions
        {
            CliPath = cliPath,
            Cwd = worktreePath,
            Environment = clientEnvironment
        });
        await client.StartAsync();

        var session = await client.CreateSessionAsync(WorkerSessionConfigFactory.Create(
            definition.Model,
            sourceGitHub,
            sourceSkills,
            ReasoningEffort,
            runtime.Tools));

        return new WorkerLoop(client, session, debugConsole, definition, runtime, agentName);
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        runtime.ResetStopSignal();
        var iterationCount = 0;

        while (!ct.IsCancellationRequested && !runtime.IsStopSignaled())
        {
            iterationCount++;
            if (iterationCount > MaxIterationsPerExecution)
            {
                throw new InvalidOperationException($"{agentName} exceeded the maximum of {MaxIterationsPerExecution} iterations without signaling stop.");
            }

            Console.WriteLine($"  {agentName} checking for work...");

            var result = await debugConsole.SendAndReadContent(session, agentName, definition.AgentColor, new MessageOptions
            {
                Prompt = PromptLoader.Load(definition.PromptName),
                Mode = definition.AgentMode
            }, definition.PromptTimeout, ct);

            if (string.IsNullOrWhiteSpace(result) && !runtime.IsStopSignaled())
            {
                throw new InvalidOperationException($"{agentName} returned an empty response without signaling stop.");
            }

            if (!debugConsole.IsEnabled)
            {
                var previousColor = Console.ForegroundColor;

                try
                {
                    Console.ForegroundColor = definition.AgentColor;
                    Console.WriteLine($"  {agentName}: {result}");
                    Console.WriteLine();
                }
                finally
                {
                    Console.ForegroundColor = previousColor;
                }
            }
        }

        if (runtime.IsStopSignaled())
        {
            Console.WriteLine($"  {agentName} signaled that no more {definition.WorkType} work is available right now.");
            Console.WriteLine();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await session.DisposeAsync();
        await client.DisposeAsync();
    }
}

internal sealed record WorkerLoopDefinition(
    string AgentDisplayName,
    string AgentMode,
    string WorkType,
    string Model,
    string PromptName,
    TimeSpan PromptTimeout,
    ConsoleColor AgentColor,
    Func<string, ConsoleColor, WorkerLoopRuntime> CreateRuntime);

internal sealed record WorkerLoopRuntime(
    ICollection<AIFunction> Tools,
    Action ResetStopSignal,
    Func<bool> IsStopSignaled);