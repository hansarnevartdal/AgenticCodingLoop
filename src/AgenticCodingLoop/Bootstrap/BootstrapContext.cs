using AgenticCodingLoop;
using AgenticCodingLoop.Configuration;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Bootstrap;

internal sealed class BootstrapContext : IAsyncDisposable
{
    public CopilotClient Client { get; }

    public string SourceGitHub { get; }

    public string SourceSkills { get; }

    private BootstrapContext(CopilotClient client, string sourceGitHub, string sourceSkills)
    {
        Client = client;
        SourceGitHub = sourceGitHub;
        SourceSkills = sourceSkills;
    }

    public static async Task<BootstrapContext> CreateAsync(WorkspaceConfig config, SessionDebugConsole debugConsole, CancellationToken ct)
    {
        var sourceGitHub = SourceGitHubLocator.Find();
        if (sourceGitHub is null)
        {
            throw new InvalidOperationException("Could not find the source .github folder.");
        }

        var sourceSkills = Path.Combine(sourceGitHub, "skills");
        var clientEnvironment = NonInteractiveCliEnvironment.Create();
        var cliPath = CopilotCliLocator.Find();

        Directory.CreateDirectory(config.TempFolder);

        await using var setupClient = new CopilotClient(new CopilotClientOptions
        {
            CliPath = cliPath,
            Cwd = config.TempFolder,
            Environment = clientEnvironment
        });
        await setupClient.StartAsync();
        Console.WriteLine("Copilot client started.");
        Console.WriteLine();

        await RepositorySetup.RunAsync(setupClient, config, sourceSkills, debugConsole, ct);

        var client = new CopilotClient(new CopilotClientOptions
        {
            CliPath = cliPath,
            Cwd = config.RepoDirectory,
            Environment = clientEnvironment
        });
        await client.StartAsync();

        return new BootstrapContext(client, sourceGitHub, sourceSkills);
    }

    public ValueTask DisposeAsync()
    {
        return Client.DisposeAsync();
    }
}