using System.Diagnostics;

namespace AgenticCodingLoop.Shared.Runtime;

internal static class WorktreeManager
{
    public static async Task<string> CreateAsync(string repoDirectory, string workerName, int workerId, CancellationToken ct)
    {
        var worktreePath = GetWorktreePath(repoDirectory, workerName, workerId);

        if (Directory.Exists(worktreePath))
        {
            await ForceRemoveWorktreeAsync(repoDirectory, worktreePath, ct);
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
            await ForceRemoveWorktreeAsync(repoDirectory, worktreePath, ct);
        }
    }

    public static Task PruneAsync(string repoDirectory, CancellationToken ct) =>
        GitAsync(repoDirectory, "worktree prune", ct);

    internal static string GetWorktreePath(string repoDirectory, string workerName, int workerId)
    {
        var parent = Path.GetDirectoryName(repoDirectory) ?? repoDirectory;
        return Path.Combine(parent, "worktrees", $"{workerName}-{workerId}");
    }

    /// <summary>
    /// Attempts <c>git worktree remove --force</c>. If that fails (e.g. the worktree is not
    /// registered, or a process still holds a file handle), falls back to deleting the directory
    /// on disk and pruning stale worktree metadata so the next <c>git worktree add</c> succeeds.
    /// </summary>
    private static async Task ForceRemoveWorktreeAsync(string repoDirectory, string worktreePath, CancellationToken ct)
    {
        if (await TryGitAsync(repoDirectory, $"worktree remove \"{worktreePath}\" --force", ct))
        {
            return;
        }

        // Fallback: the directory exists but git does not recognise it as a worktree, or the
        // remove failed because a handle was still open.  Delete the directory directly and
        // prune git's worktree bookkeeping so it stops tracking the stale entry.
        try
        {
            Directory.Delete(worktreePath, recursive: true);
        }
        catch (IOException)
        {
            // Best-effort; the directory may already be gone or still locked.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort; the directory may have read-only files left by git.
        }

        await TryGitAsync(repoDirectory, "worktree prune", ct);
    }

    private static async Task GitAsync(string workingDirectory, string arguments, CancellationToken ct)
    {
        if (!await TryGitAsync(workingDirectory, arguments, ct))
        {
            throw new InvalidOperationException($"git {arguments} failed.");
        }
    }

    private static async Task<bool> TryGitAsync(string workingDirectory, string arguments, CancellationToken ct)
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
        _ = await stderrTask;

        return process.ExitCode == 0;
    }
}