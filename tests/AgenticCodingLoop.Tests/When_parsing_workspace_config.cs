using AgenticCodingLoop.Host;

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
        Assert.False(result.Debug);
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
        Assert.False(result.Debug);
    }

    [Fact]
    public void Should_enable_debug_mode_when_flag_is_present()
    {
        // Arrange
        var args = new[] { "--debug", "https://github.com/owner/my-repo" };

        // Act
        var result = WorkspaceConfig.Parse(args);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Debug);
    }

    [Fact]
    public void Should_allow_debug_flag_between_repo_and_temp_folder()
    {
        // Arrange
        var args = new[] { "https://github.com/owner/my-repo", "--debug", ".\\tmp" };

        // Act
        var result = WorkspaceConfig.Parse(args);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Debug);
        Assert.Equal(Path.GetFullPath(".\\tmp"), result.TempFolder);
    }

    [Fact]
    public void Should_default_max_parallel_to_one()
    {
        // Arrange
        var args = new[] { "https://github.com/owner/my-repo" };

        // Act
        var result = WorkspaceConfig.Parse(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.MaxParallel);
    }

    [Fact]
    public void Should_parse_max_parallel_option()
    {
        // Arrange
        var args = new[] { "--max-parallel", "3", "https://github.com/owner/my-repo" };

        // Act
        var result = WorkspaceConfig.Parse(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.MaxParallel);
    }

    [Fact]
    public void Should_reject_max_parallel_of_zero()
    {
        // Arrange
        var args = new[] { "--max-parallel", "0", "https://github.com/owner/my-repo" };

        // Act
        var result = WorkspaceConfig.Parse(args);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Should_reject_negative_max_parallel()
    {
        // Arrange
        var args = new[] { "--max-parallel", "-1", "https://github.com/owner/my-repo" };

        // Act
        var result = WorkspaceConfig.Parse(args);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Should_reject_max_parallel_without_value()
    {
        // Arrange
        var args = new[] { "https://github.com/owner/my-repo", "--max-parallel" };

        // Act
        var result = WorkspaceConfig.Parse(args);

        // Assert
        Assert.Null(result);
    }
}