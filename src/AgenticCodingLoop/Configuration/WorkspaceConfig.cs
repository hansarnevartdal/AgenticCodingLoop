namespace AgenticCodingLoop.Configuration;

internal sealed record WorkspaceConfig(string GitHubRepoUrl, string TempFolder)
{
    private const string AppFolderName = "AgenticCodingLoop";

    public string RepoDirectory => Path.Combine(TempFolder, ExtractRepoName(GitHubRepoUrl));

    public static WorkspaceConfig? Parse(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: AgenticCodingLoop <githubRepoUrl> [tempFolder]");
            return null;
        }

        var repoUrl = args[0];
        var tempFolder = args.Length >= 2
            ? Path.GetFullPath(args[1])
            : GetDefaultTempFolder();

        if (!Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri) ||
            !uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"Invalid GitHub repository URL: {repoUrl}");
            return null;
        }

        Console.WriteLine($"Repository: {repoUrl}");
        Console.WriteLine($"Working Folder: {tempFolder}");
        Console.WriteLine();

        return new WorkspaceConfig(repoUrl, tempFolder);
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