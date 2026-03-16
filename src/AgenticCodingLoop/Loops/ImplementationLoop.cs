using AgenticCodingLoop.Configuration;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Loops;

internal sealed class ImplementationLoop : IAsyncDisposable
{
    private const string ImplementerAgent = "implementer";
    private const string ReasoningEffort = "high";
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(10);

    private readonly CopilotSession session;
    private readonly LoopStopSignal stopSignal;

    private ImplementationLoop(CopilotSession session, LoopStopSignal stopSignal)
    {
        this.session = session;
        this.stopSignal = stopSignal;
    }

    public static async Task<ImplementationLoop> CreateAsync(CopilotClient client, string sourceGitHub, string sourceSkills)
    {
        var stopSignal = new LoopStopSignal("implementation");
        var session = await client.CreateSessionAsync(WorkerSessionConfigFactory.Create(
            CopilotModels.Coder,
            sourceGitHub,
            sourceSkills,
            ReasoningEffort,
            stopSignal.CreateTools()));

        return new ImplementationLoop(session, stopSignal);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        stopSignal.Reset();

        while (!ct.IsCancellationRequested && !stopSignal.IsNoMoreWorkSignaled)
        {
            Console.WriteLine("  Implementer checking for work...");

            var response = await session.SendAndWaitAsync(new MessageOptions
            {
                Prompt = """
                         Check the repository state and perform one implementation iteration.

                         Use the loaded `github-cli` and `git` skills for command details and conventions.

                         Start by deciding whether any implementation work is actually available right now.
                         If there is no implementation work available, call the `signal_no_more_work` tool immediately with a short reason instead of only stating that there is nothing to do.

                         Your priorities are:

                         1. Merge any pull request that is ready to merge because it is approved.
                         2. Pick up any pull request that needs more work after review, apply the requested fixes, push the update, and move it back to ready-for-review.
                         3. If there is no higher-priority PR work, pick the next open issue that does not already have an active PR, implement it on a branch, open a PR, and mark it ready for review.

                         Follow the repository's labels and GitHub review state as the source of truth.
                         Keep the work scoped to one iteration only, then stop and report what you did.
                    """,
                Mode = ImplementerAgent
            }, PromptTimeout, ct);
            var result = response?.Data?.Content ?? string.Empty;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Implementer: {result}");
            Console.ResetColor();
            Console.WriteLine();
        }

        if (stopSignal.IsNoMoreWorkSignaled)
        {
            Console.WriteLine("  Implementer signaled that no more implementation work is available right now.");
            Console.WriteLine();
        }
    }

    public ValueTask DisposeAsync()
    {
        return session.DisposeAsync();
    }
}