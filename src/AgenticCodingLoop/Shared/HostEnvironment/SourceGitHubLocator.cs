namespace AgenticCodingLoop.Shared.HostEnvironment;

internal static class SourceGitHubLocator
{
    public static string? Find()
    {
        var appDirectory = AppContext.BaseDirectory;
        var candidate = Path.Combine(appDirectory, ".github");
        if (Directory.Exists(candidate)) { return candidate; }

        var directory = new DirectoryInfo(appDirectory);
        while (directory is not null)
        {
            candidate = Path.Combine(directory.FullName, ".github");
            if (Directory.Exists(candidate)) { return candidate; }

            directory = directory.Parent;
        }

        return null;
    }
}