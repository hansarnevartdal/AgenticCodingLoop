using AgenticCodingLoop.Shared.HostEnvironment;

namespace AgenticCodingLoop.Tests;

public sealed class When_locating_copilot_cli : IDisposable
{
    private readonly string tempDirectory = Path.Combine(Path.GetTempPath(), $"copilot-cli-test-{Guid.NewGuid():N}");

    public When_locating_copilot_cli()
    {
        Directory.CreateDirectory(tempDirectory);
    }

    [Fact]
    public void Should_prefer_override_path_when_present()
    {
        // Arrange
        var cliPath = CreateFakeCli("custom-copilot.exe");

        // Act
        var result = CopilotCliLocator.Find(cliPath, null);

        // Assert
        Assert.Equal(cliPath, result);
    }

    [Fact]
    public void Should_find_cli_on_path_when_override_is_missing()
    {
        // Arrange
        var cliPath = CreateFakeCli(OperatingSystem.IsWindows() ? "copilot.exe" : "copilot");

        // Act
        var result = CopilotCliLocator.Find(null, tempDirectory);

        // Assert
        Assert.Equal(cliPath, result);
    }

    [Fact]
    public void Should_throw_when_override_path_does_not_exist()
    {
        // Arrange
        var missingPath = Path.Combine(tempDirectory, "missing-copilot.exe");

        // Act
        var action = () => CopilotCliLocator.Find(missingPath, null);

        // Assert
        var ex = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("does not exist", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    private string CreateFakeCli(string fileName)
    {
        var cliPath = Path.Combine(tempDirectory, fileName);
        File.WriteAllText(cliPath, "fake");
        return cliPath;
    }
}