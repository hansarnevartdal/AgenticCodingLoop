using AgenticCodingLoop.Configuration;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Bootstrap;

internal static class RepositorySetup
{
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(5);

    public static async Task RunAsync(
        CopilotClient client,
        WorkspaceConfig config,
        string sourceSkills,
        CancellationToken ct)
    {
        Console.WriteLine("═══ Repository Setup ═══");
        Console.WriteLine();

        await using var session = await client.CreateSessionAsync(new SessionConfig
        {
            Model = CopilotModels.Free,
            SystemMessage = new SystemMessageConfig
            {
                Mode = SystemMessageMode.Append,
                Content = "You are a helper that sets up git repositories. Use the terminal to run git and gh commands."
            },
            SkillDirectories = [sourceSkills],
            OnPermissionRequest = PermissionHandler.ApproveAll
        });

        var repoDirectory = config.RepoDirectory;
        var cloneStep = Directory.Exists(Path.Combine(repoDirectory, ".git"))
            ? $"""Run `git -C "{repoDirectory}" fetch --all && git -C "{repoDirectory}" pull` to get the latest changes."""
            : $"""Clone the repository: `git clone {config.GitHubRepoUrl} "{repoDirectory}"`""";

        var response = await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = $"""
                    Set up the repository for development.

                    1. {cloneStep}
                    2. Ensure these GitHub labels exist on the repo (create any that are missing):
                       - `status:in-progress` (color: 1d76db, description: "Issue is being implemented")
                       - `status:ready-for-review` (color: 0e8a16, description: "PR is ready for review")
                       - `status:needs-work` (color: e11d48, description: "PR needs changes after review")
                       - `status:approved` (color: 28a745, description: "PR approved and ready to merge")

                       Use `gh label list --json name` (inside the repo directory) to check, then `gh label create` for missing ones.
                    3. List open issues: `gh issue list --state open` and report whether there is work to do.
                    """
        }, PromptTimeout, ct);
        var result = response?.Data?.Content ?? string.Empty;

        Console.WriteLine($"  Setup: {result}");
        Console.WriteLine();
    }
}