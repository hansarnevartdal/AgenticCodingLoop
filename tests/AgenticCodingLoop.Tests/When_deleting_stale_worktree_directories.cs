using AgenticCodingLoop.Shared.Runtime;

namespace AgenticCodingLoop.Tests;

public sealed class When_deleting_stale_worktree_directories : IDisposable
{
    private readonly string directoryPath = Path.Combine(Path.GetTempPath(), $"AgenticCodingLoop-{Guid.NewGuid():N}");

    [Fact]
    public async Task Should_remove_read_only_files()
    {
        var nestedDirectory = Path.Combine(directoryPath, "nested");
        Directory.CreateDirectory(nestedDirectory);

        var filePath = Path.Combine(nestedDirectory, "leftover.lock");
        await File.WriteAllTextAsync(filePath, "stale");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        await WorktreeManager.DeleteDirectoryIfPresentAsync(directoryPath, CancellationToken.None);

        Assert.False(Directory.Exists(directoryPath));
    }

    public void Dispose()
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        foreach (var fileSystemInfo in new DirectoryInfo(directoryPath).EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
        {
            fileSystemInfo.Attributes = FileAttributes.Normal;
        }

        Directory.Delete(directoryPath, recursive: true);
    }
}