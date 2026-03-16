using AgenticCodingLoop.Configuration;

namespace AgenticCodingLoop.Tests;

public sealed class When_parsing_workspace_config
{
    [Fact]
    public void Should_use_local_appdata_when_temp_folder_is_not_provided()
    {
        // Arrange
        var args = new[] { "https://github.com/owner/my-repo" };

        // Act
        var result = WorkspaceConfig.Parse(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(WorkspaceConfig.GetDefaultTempFolder(), result.TempFolder);
    }

    [Fact]
    public void Should_use_explicit_temp_folder_when_provided()
    {
        // Arrange
        var args = new[] { "https://github.com/owner/my-repo", ".\\tmp" };

        // Act
        var result = WorkspaceConfig.Parse(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Path.GetFullPath(".\\tmp"), result.TempFolder);
    }
}