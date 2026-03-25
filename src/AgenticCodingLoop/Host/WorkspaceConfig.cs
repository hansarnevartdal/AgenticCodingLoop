namespace AgenticCodingLoop.Host;

internal sealed record WorkspaceConfig(string GitHubRepoUrl, string TempFolder, bool Debug, int MaxParallel = 1)
{
    private const string AppFolderName = "AgenticCodingLoop";
    private const string UsageLine = "Usage: AgenticCodingLoop [--debug] [--max-parallel <N>] <githubRepoUrl> [tempFolder]";

    public string RepoDirectory => Path.Combine(TempFolder, ExtractRepoName(GitHubRepoUrl));

    public static WorkspaceConfig? Parse(string[] args)
    {
        var debug = false;
        var maxParallel = 1;
        var positionalArgs = new List<string>(capacity: 2);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.Equals("--debug", StringComparison.OrdinalIgnoreCase))
            {
                debug = true;
                continue;
            }

            if (arg.Equals("--max-parallel", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out maxParallel) || maxParallel < 1)
                {
                    Console.Error.WriteLine("--max-parallel requires a positive integer value.");
                    Console.Error.WriteLine(UsageLine);
                    return null;
                }

                i++;
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"Unknown option: {arg}");
                Console.Error.WriteLine(UsageLine);
                return null;
            }

            positionalArgs.Add(arg);
        }

        if (positionalArgs.Count is < 1 or > 2)
        {
            Console.Error.WriteLine(UsageLine);
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
        Console.WriteLine($"Max Parallel Workers: {maxParallel}");
        if (debug)
        {
            Console.WriteLine("Debug Mode: enabled");
        }
        Console.WriteLine();

        return new WorkspaceConfig(repoUrl, tempFolder, debug, maxParallel);
    }

    internal static string GetDefaultTempFolder()
    {
        var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
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