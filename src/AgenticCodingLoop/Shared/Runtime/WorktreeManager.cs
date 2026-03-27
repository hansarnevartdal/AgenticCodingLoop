using System.Diagnostics;

namespace AgenticCodingLoop.Shared.Runtime;

internal static class WorktreeManager
{
    private const int DeleteRetryCount = 5;
    private static readonly TimeSpan DeleteRetryDelay = TimeSpan.FromMilliseconds(200);

    public static async Task<string> CreateAsync(string repoDirectory, string workerName, int workerId, CancellationToken ct)
    {
        var worktreePath = GetWorktreePath(repoDirectory, workerName, workerId);

        // Prune stale git metadata first so a previous crashed run cannot block re-adding the
        // same worktree path even when the directory is already gone.
        await TryGitAsync(repoDirectory, "worktree prune", ct);

        if (Directory.Exists(worktreePath))
        {
            await ForceRemoveWorktreeAsync(repoDirectory, worktreePath, ct);
        }

        if (Directory.Exists(worktreePath))
        {
            throw new InvalidOperationException(
                $"Failed to remove stale worktree directory '{worktreePath}'. Close any process using that folder and retry.");
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
        var repoName = Path.GetFileName(repoDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return Path.Combine(parent, "worktrees", repoName, $"{workerName}-{workerId}");
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
            await DeleteDirectoryIfPresentAsync(worktreePath, ct);
            return;
        }

        // Fallback: the directory exists but git does not recognise it as a worktree, or the
        // remove failed because a handle was still open.  Delete the directory directly and
        // prune git's worktree bookkeeping so it stops tracking the stale entry.
        await DeleteDirectoryIfPresentAsync(worktreePath, ct);

        await TryGitAsync(repoDirectory, "worktree prune", ct);
    }

    internal static async Task DeleteDirectoryIfPresentAsync(string directoryPath, CancellationToken ct)
    {
        for (var attempt = 0; attempt < DeleteRetryCount; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            try
            {
                ResetAttributes(directoryPath);
                Directory.Delete(directoryPath, recursive: true);
                return;
            }
            catch (IOException) when (attempt < DeleteRetryCount - 1)
            {
                await Task.Delay(DeleteRetryDelay, ct);
            }
            catch (UnauthorizedAccessException) when (attempt < DeleteRetryCount - 1)
            {
                await Task.Delay(DeleteRetryDelay, ct);
            }
        }
    }

    private static void ResetAttributes(string directoryPath)
    {
        var directoryInfo = new DirectoryInfo(directoryPath);

        foreach (var fileSystemInfo in directoryInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
        {
            fileSystemInfo.Attributes = FileAttributes.Normal;
        }

        directoryInfo.Attributes = FileAttributes.Normal;
    }

    private static async Task GitAsync(string workingDirectory, string arguments, CancellationToken ct)
    {
        var result = await RunGitAsync(workingDirectory, arguments, ct);
        if (!result.Success)
        {
            throw new InvalidOperationException($"git {arguments} failed (exit {result.ExitCode}): {result.StandardError}");
        }
    }

    private static async Task<bool> TryGitAsync(string workingDirectory, string arguments, CancellationToken ct)
    {
        var result = await RunGitAsync(workingDirectory, arguments, ct);
        return result.Success;
    }

    private static async Task<GitCommandResult> RunGitAsync(string workingDirectory, string arguments, CancellationToken ct)
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

        return new GitCommandResult(process.ExitCode == 0, process.ExitCode, stderr.Trim());
    }

    private sealed record GitCommandResult(bool Success, int ExitCode, string StandardError);
}