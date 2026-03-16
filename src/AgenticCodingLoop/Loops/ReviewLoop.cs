using AgenticCodingLoop.Configuration;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Loops;

internal sealed class ReviewLoop : IAsyncDisposable
{
    private const string ReviewerAgent = "reviewer";
    private const string ReasoningEffort = "high";
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(5);

    private readonly CopilotSession session;
    private readonly LoopStopSignal stopSignal;

    private ReviewLoop(CopilotSession session, LoopStopSignal stopSignal)
    {
        this.session = session;
        this.stopSignal = stopSignal;
    }

    public static async Task<ReviewLoop> CreateAsync(CopilotClient client, string sourceGitHub, string sourceSkills)
    {
        var stopSignal = new LoopStopSignal("review");
        var session = await client.CreateSessionAsync(WorkerSessionConfigFactory.Create(
            CopilotModels.Reviewer,
            sourceGitHub,
            sourceSkills,
            ReasoningEffort,
            stopSignal.CreateTools()));

        return new ReviewLoop(session, stopSignal);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        stopSignal.Reset();

        while (!ct.IsCancellationRequested && !stopSignal.IsNoMoreWorkSignaled)
        {
            Console.WriteLine("  Reviewer checking for work...");

            var response = await session.SendAndWaitAsync(new MessageOptions
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
                                4. If the work is acceptable, approve it and mark it approved.
                                5. If the work needs changes, request changes with specific actionable feedback and mark it as needs-work.

                                Review one iteration only, then stop and report what you did.
                                If nothing is ready for review, just say so.
                        """,
                Mode = ReviewerAgent
            }, PromptTimeout, ct);
            var result = response?.Data?.Content ?? string.Empty;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  Reviewer: {result}");
            Console.ResetColor();
            Console.WriteLine();
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