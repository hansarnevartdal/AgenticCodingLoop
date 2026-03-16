using AgenticCodingLoop.Configuration;

namespace AgenticCodingLoop.Tests;

public sealed class When_extracting_repo_name
{
    [Theory]
    [InlineData("https://github.com/owner/my-repo", "my-repo")]
    [InlineData("https://github.com/owner/my-repo.git", "my-repo")]
    [InlineData("https://github.com/owner/my-repo/", "my-repo")]
    public void Should_extract_repository_name_from_url(string url, string expected)
    {
        // Act
        var result = WorkspaceConfig.ExtractRepoName(url);

        // Assert
        Assert.Equal(expected, result);
    }
}
