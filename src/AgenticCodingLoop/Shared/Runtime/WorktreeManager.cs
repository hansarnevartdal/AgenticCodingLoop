using System.Diagnostics;

namespace AgenticCodingLoop.Shared.Runtime;

internal static class WorktreeManager
{
    public static async Task<string> CreateAsync(string repoDirectory, string workerName, int workerId, CancellationToken ct)
    {
        var worktreePath = GetWorktreePath(repoDirectory, workerName, workerId);

        if (Directory.Exists(worktreePath))
        {
            await GitAsync(repoDirectory, $"worktree remove \"{worktreePath}\" --force", ct);
        }

        // The primary repo keeps `main` checked out, so worker worktrees start from a detached
        // checkout of `main` and let the agent create a fresh branch when it begins new work.
        await GitAsync(repoDirectory, $"worktree add \"{worktreePath}\" --detach main", ct);
        return worktreePath;
    }

    public static async Task RemoveAsync(string repoDirectory, string worktreePath, CancellationToken ct)
    {
        if (Directory.Exists(worktreePath))
        {
            await GitAsync(repoDirectory, $"worktree remove \"{worktreePath}\" --force", ct);
        }
    }

    public static Task PruneAsync(string repoDirectory, CancellationToken ct) =>
        GitAsync(repoDirectory, "worktree prune", ct);

    internal static string GetWorktreePath(string repoDirectory, string workerName, int workerId)
    {
        var parent = Path.GetDirectoryName(repoDirectory) ?? repoDirectory;
        return Path.Combine(parent, "worktrees", $"{workerName}-{workerId}");
    }

    private static async Task GitAsync(string workingDirectory, string arguments, CancellationToken ct)
    {
        var psi = new ProcessStartInfo("git", arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start git.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        _ = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"git {arguments} failed (exit {process.ExitCode}): {stderr}");
        }
    }
}