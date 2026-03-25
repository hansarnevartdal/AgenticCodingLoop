using AgenticCodingLoop.Shared.Runtime;

namespace AgenticCodingLoop.Tests;

public sealed class When_building_worktree_paths
{
    [Fact]
    public void Should_place_worktrees_in_a_repo_specific_folder_next_to_the_repo()
    {
        var repoDirectory = Path.Combine("C:", "repos", "sample-repo");

        var result = WorktreeManager.GetWorktreePath(repoDirectory, "implementer", 7);

        var expected = Path.Combine("C:", "repos", "worktrees", "sample-repo", "implementer-7");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Should_not_collide_when_two_repos_use_the_same_worker_id()
    {
        var firstRepoDirectory = Path.Combine("C:", "repos", "alpha");
        var secondRepoDirectory = Path.Combine("C:", "repos", "beta");

        var firstPath = WorktreeManager.GetWorktreePath(firstRepoDirectory, "implementer", 0);
        var secondPath = WorktreeManager.GetWorktreePath(secondRepoDirectory, "implementer", 0);

        Assert.NotEqual(firstPath, secondPath);
    }
}