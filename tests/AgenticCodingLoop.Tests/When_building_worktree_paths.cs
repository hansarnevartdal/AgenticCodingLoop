using AgenticCodingLoop.Shared.Runtime;

namespace AgenticCodingLoop.Tests;

public sealed class When_building_worktree_paths
{
    [Fact]
    public void Should_place_worktrees_next_to_the_repo_folder()
    {
        var repoDirectory = Path.Combine("C:", "repos", "sample-repo");

        var result = WorktreeManager.GetWorktreePath(repoDirectory, "implementer", 7);

        var expected = Path.Combine("C:", "repos", "worktrees", "implementer-7");
        Assert.Equal(expected, result);
    }
}