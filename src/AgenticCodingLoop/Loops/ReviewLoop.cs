using AgenticCodingLoop;
using AgenticCodingLoop.Configuration;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Loops;

internal sealed class ReviewLoop : IAsyncDisposable
{
    private const string ReviewerAgent = "reviewer";
    private const string ReasoningEffort = "high";
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(5);
    private const string AgentName = "Reviewer";
    private const ConsoleColor AgentColor = ConsoleColor.Magenta;

    private readonly CopilotSession session;
    private readonly LoopStopSignal stopSignal;
    private readonly SessionDebugConsole debugConsole;

    private ReviewLoop(CopilotSession session, LoopStopSignal stopSignal, SessionDebugConsole debugConsole)
    {
        this.session = session;
        this.stopSignal = stopSignal;
        this.debugConsole = debugConsole;
    }

    public static async Task<ReviewLoop> CreateAsync(CopilotClient client, string sourceGitHub, string sourceSkills, SessionDebugConsole debugConsole)
    {
        var stopSignal = new LoopStopSignal("review");
        var session = await client.CreateSessionAsync(WorkerSessionConfigFactory.Create(
            CopilotModels.Reviewer,
            sourceGitHub,
            sourceSkills,
            ReasoningEffort,
            stopSignal.CreateTools()));

        return new ReviewLoop(session, stopSignal, debugConsole);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        stopSignal.Reset();

        while (!ct.IsCancellationRequested && !stopSignal.IsNoMoreWorkSignaled)
        {
            Console.WriteLine("  Reviewer checking for work...");

            var result = await debugConsole.SendAndReadContent(session, AgentName, AgentColor, new MessageOptions
            {
                Prompt = """
                                Check the repository for pull requests that are ready for review and perform one review iteration.

                                Use the loaded `github-cli` skill for command details and review workflow.

                                Start by deciding whether any review work is actually available right now.
                                If there is no review work available, call the `signal_no_more_work` tool immediately with a short reason instead of only stating that there is nothing to do.

                                Your job is to:

                                1. Find pull requests that are marked ready for review.
                                2. Read the pull request and any linked issue context needed to judge the work correctly.
                                3. Review the change for correctness, completeness, and code quality.
                                4. If the work is acceptable, submit a formal GitHub approval review, ensure the PR has the label `status:approved`, and remove `status:ready-for-review` and `status:needs-work` if they are present.
                                5. Do not stop at writing a comment like "looks good", "no issues", or "still good to merge". Those comments are not enough by themselves. When the PR is acceptable, you must leave it in an explicitly approved state that the implementer can detect.
                                6. If the work needs changes, request changes with specific actionable feedback, ensure the PR has the label `status:needs-work`, and remove `status:ready-for-review` and `status:approved` if they are present.

                                Review one iteration only, then re-check whether any review work still remains right now.
                                If no review work remains after your iteration, call the `signal_no_more_work` tool before you finish.
                                If more review work still remains, do not call the tool. Just stop and report what you did in this iteration.
                                If nothing is ready for review at the start, call the `signal_no_more_work` tool immediately instead of only saying so.
                        """,
                Mode = ReviewerAgent
            }, PromptTimeout, ct);

            if (!debugConsole.IsEnabled)
            {
                Console.ForegroundColor = AgentColor;
                Console.WriteLine($"  Reviewer: {result}");
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        if (stopSignal.IsNoMoreWorkSignaled)
        {
            Console.WriteLine("  Reviewer signaled that no more review work is available right now.");
            Console.WriteLine();
        }
    }

    public ValueTask DisposeAsync()
    {
        return session.DisposeAsync();
    }
}