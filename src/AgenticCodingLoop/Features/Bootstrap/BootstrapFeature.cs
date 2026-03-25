using AgenticCodingLoop.Host;
using AgenticCodingLoop.Shared.HostEnvironment;
using AgenticCodingLoop.Shared.Runtime;
using GitHub.Copilot.SDK;

namespace AgenticCodingLoop.Features.Bootstrap;

internal sealed class BootstrapFeature : IAsyncDisposable
{
    public CopilotClient Client { get; }

    public string SourceGitHub { get; }

    public string SourceSkills { get; }

    public string CliPath { get; }

    public IReadOnlyDictionary<string, string> ClientEnvironment { get; }

    private BootstrapFeature(
        CopilotClient client,
        string sourceGitHub,
        string sourceSkills,
        string cliPath,
        IReadOnlyDictionary<string, string> clientEnvironment)
    {
        Client = client;
        SourceGitHub = sourceGitHub;
        SourceSkills = sourceSkills;
        CliPath = cliPath;
        ClientEnvironment = clientEnvironment;
    }

    public static async Task<BootstrapFeature> ExecuteAsync(WorkspaceConfig config, SessionDebugConsole debugConsole, CancellationToken ct)
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

        await RepositorySetup.ExecuteAsync(setupClient, config, sourceSkills, debugConsole, ct);

        await WorktreeManager.PruneAsync(config.RepoDirectory, ct);

        var client = new CopilotClient(new CopilotClientOptions
        {
            CliPath = cliPath,
            Cwd = config.RepoDirectory,
            Environment = clientEnvironment
        });
        await client.StartAsync();

        return new BootstrapFeature(client, sourceGitHub, sourceSkills, cliPath, clientEnvironment);
    }

    public ValueTask DisposeAsync() => Client.DisposeAsync();
}