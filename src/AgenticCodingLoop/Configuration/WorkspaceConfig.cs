namespace AgenticCodingLoop.Configuration;

internal sealed record WorkspaceConfig(string GitHubRepoUrl, string TempFolder, bool Debug)
{
    private const string AppFolderName = "AgenticCodingLoop";

    public string RepoDirectory => Path.Combine(TempFolder, ExtractRepoName(GitHubRepoUrl));

    public static WorkspaceConfig? Parse(string[] args)
    {
        var debug = false;
        var positionalArgs = new List<string>(capacity: 2);

        foreach (var arg in args)
        {
            if (arg.Equals("--debug", StringComparison.OrdinalIgnoreCase))
            {
                debug = true;
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"Unknown option: {arg}");
                Console.Error.WriteLine("Usage: AgenticCodingLoop [--debug] <githubRepoUrl> [tempFolder]");
                return null;
            }

            positionalArgs.Add(arg);
        }

        if (positionalArgs.Count is < 1 or > 2)
        {
            Console.Error.WriteLine("Usage: AgenticCodingLoop [--debug] <githubRepoUrl> [tempFolder]");
            return null;
        }

        var repoUrl = positionalArgs[0];
        var tempFolder = positionalArgs.Count == 2
            ? Path.GetFullPath(positionalArgs[1])
            : GetDefaultTempFolder();

        if (!Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri) ||
            !uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"Invalid GitHub repository URL: {repoUrl}");
            return null;
        }

        Console.WriteLine($"Repository: {repoUrl}");
        Console.WriteLine($"Working Folder: {tempFolder}");
        if (debug)
        {
            Console.WriteLine("Debug Mode: enabled");
        }
        Console.WriteLine();

        return new WorkspaceConfig(repoUrl, tempFolder, debug);
    }

    internal static string GetDefaultTempFolder()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, AppFolderName);
    }

    internal static string ExtractRepoName(string url)
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath.TrimEnd('/');
        var lastSegment = path.Split('/')[^1];

        return lastSegment.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? lastSegment[..^4]
            : lastSegment;
    }
}