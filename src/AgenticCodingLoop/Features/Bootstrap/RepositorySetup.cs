using AgenticCodingLoop.Host;
using AgenticCodingLoop.Shared.Prompts;
using AgenticCodingLoop.Shared.Runtime;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Features.Bootstrap;

internal static class RepositorySetup
{
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(5);
    private const string AgentName = "Setup";
    private const ConsoleColor AgentColor = ConsoleColor.Yellow;

    public static async Task ExecuteAsync(
        CopilotClient client,
        WorkspaceConfig config,
        string sourceSkills,
        SessionDebugConsole debugConsole,
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
            // The demo runs unattended, so the SDK should approve terminal/tool requests automatically.
            OnPermissionRequest = PermissionHandler.ApproveAll
        });

        var repoDirectory = config.RepoDirectory;
        var cloneStep = Directory.Exists(Path.Combine(repoDirectory, ".git"))
            ? $"""Run `git -C "{repoDirectory}" fetch --all && git -C "{repoDirectory}" pull` to get the latest changes."""
            : $"""Clone the repository: `git clone {config.GitHubRepoUrl} "{repoDirectory}"`""";

        var result = await debugConsole.SendAndReadContent(session, AgentName, AgentColor, new MessageOptions
        {
            Prompt = PromptLoader.Load("Features.Bootstrap.repository-setup.prompt", ("cloneStep", cloneStep))
        }, PromptTimeout, ct);

        if (!debugConsole.IsEnabled)
        {
            Console.WriteLine($"  Setup: {result}");
            Console.WriteLine();
        }
    }
}