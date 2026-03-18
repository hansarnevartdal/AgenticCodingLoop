using AgenticCodingLoop;
using AgenticCodingLoop.Configuration;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Loops;

internal sealed class ImplementationLoop : IAsyncDisposable
{
    private const string ImplementerAgent = "implementer";
    private const string ReasoningEffort = "high";
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(10);
    private const string AgentName = "Implementer";
    private const ConsoleColor AgentColor = ConsoleColor.Cyan;

    private readonly CopilotSession session;
    private readonly LoopStopSignal stopSignal;
    private readonly SessionDebugConsole debugConsole;

    private ImplementationLoop(CopilotSession session, LoopStopSignal stopSignal, SessionDebugConsole debugConsole)
    {
        this.session = session;
        this.stopSignal = stopSignal;
        this.debugConsole = debugConsole;
    }

    public static async Task<ImplementationLoop> CreateAsync(CopilotClient client, string sourceGitHub, string sourceSkills, SessionDebugConsole debugConsole)
    {
        var stopSignal = new LoopStopSignal("implementation");
        var session = await client.CreateSessionAsync(WorkerSessionConfigFactory.Create(
            CopilotModels.Coder,
            sourceGitHub,
            sourceSkills,
            ReasoningEffort,
            stopSignal.CreateTools()));

        return new ImplementationLoop(session, stopSignal, debugConsole);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        stopSignal.Reset();

        while (!ct.IsCancellationRequested && !stopSignal.IsNoMoreWorkSignaled)
        {
            Console.WriteLine("  Implementer checking for work...");

            var result = await debugConsole.SendAndReadContent(session, AgentName, AgentColor, new MessageOptions
            {
                Prompt = """
                         Check the repository state and perform one implementation iteration.

                         Use the loaded `github-cli` and `git` skills for command details and conventions.

                         Start by deciding whether any implementation work is actually available right now.
                         Treat an issue as implementation work only if it does not already have an active open PR. If an issue is already represented by an open PR that is awaiting review or currently under review, that is not new implementation work.
                         For open PRs, do not rely only on labels or review decision. You must also inspect reviews, review comments, issue comments, and unresolved review threads to detect feedback from humans or bots that still needs action.
                         If there is no implementation work available, call the `signal_no_more_work` tool immediately with a short reason instead of only stating that there is nothing to do.

                         Your priorities are:

                         1. Merge any pull request that is ready to merge because it is approved.
                         2. Inspect open PRs that are not approved for unresolved or unanswered feedback from humans or bots. If a PR has comments, review comments, or unresolved threads that still require changes, pick that PR up even if there is no `CHANGES_REQUESTED` review decision and no `status:needs-work` label.
                         3. When working on PR feedback, address comments that are still unresolved or unanswered, push the update, and move the PR back to ready-for-review.
                         4. If there is no higher-priority PR work, pick the next open issue that does not already have an active PR, update your local `main`, create a fresh branch from `main` for that issue, implement the work there, open a PR, and mark it ready for review.

                         For every new issue, always create a new branch based on the current `main`. Do not branch from an older issue branch, a review-fix branch, or any branch other than `main`.
                         When a PR has outstanding feedback that you are picking up, make sure the PR carries the label `status:needs-work` while you are working on it.
                         When you create or update a PR that should wait for review, make sure the PR carries the label `status:ready-for-review`.
                         When you are actively working on an issue, make sure the issue carries the label `status:in-progress`.
                         Follow the repository's labels and GitHub review state as the source of truth.
                         Treat already-answered comments and already-resolved threads as done. Do not repeat work that has already been addressed.
                         Do not pick up the same issue again just because the issue remains open while its PR is being reviewed.
                         Keep the work scoped to one iteration only, then re-check whether any implementation work still remains right now.
                         If no implementation work remains after your iteration, call the `signal_no_more_work` tool before you finish.
                         If more implementation work still remains, do not call the tool. Just stop and report what you did in this iteration.
                    """,
                Mode = ImplementerAgent
            }, PromptTimeout, ct);

            if (!debugConsole.IsEnabled)
            {
                Console.ForegroundColor = AgentColor;
                Console.WriteLine($"  Implementer: {result}");
                Console.ResetColor();
                Console.WriteLine();
            }
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