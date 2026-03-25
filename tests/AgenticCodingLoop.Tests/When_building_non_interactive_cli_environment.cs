using AgenticCodingLoop.Shared.HostEnvironment;

namespace AgenticCodingLoop.Tests;

public sealed class When_building_non_interactive_cli_environment
{
    [Fact]
    public void Should_include_non_interactive_overrides()
    {
        // Act
        var environment = NonInteractiveCliEnvironment.Create();

        // Assert
        Assert.Equal("1", environment["GH_PROMPT_DISABLED"]);
        Assert.Equal("0", environment["GIT_TERMINAL_PROMPT"]);
    }

    [Fact]
    public void Should_preserve_existing_path_variable()
    {
        // Act
        var environment = NonInteractiveCliEnvironment.Create();

        // Assert
        Assert.True(environment.ContainsKey("PATH"));
        Assert.False(string.IsNullOrWhiteSpace(environment["PATH"]));
    }
}