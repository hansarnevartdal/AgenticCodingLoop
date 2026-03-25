using System.Reflection;
using AgenticCodingLoop.Shared.Runtime;

namespace AgenticCodingLoop.Tests;

[Collection("Console")]
public sealed class When_writing_session_debug_output
{
    [Fact]
    public void Should_format_agent_output_as_an_indented_block()
    {
        // Arrange
        using var output = new StringWriter();
        var previousOutput = Console.Out;
        Console.SetOut(output);
        var writeBlock = typeof(SessionDebugConsole).GetMethod("WriteBlock", BindingFlags.NonPublic | BindingFlags.Static);

        try
        {
            Assert.NotNull(writeBlock);

            // Act
            writeBlock!.Invoke(null, ["Monitor", ConsoleColor.Blue, "response", "line one\r\nline two\r\n"]);

            // Assert
            var text = output.ToString();
            Assert.Contains("Monitor response", text, StringComparison.Ordinal);
            Assert.Contains("    line one", text, StringComparison.Ordinal);
            Assert.Contains("    line two", text, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(previousOutput);
        }
    }

    [Fact]
    public void Should_render_empty_content_with_placeholder()
    {
        // Arrange
        using var output = new StringWriter();
        var previousOutput = Console.Out;
        Console.SetOut(output);
        var writeBlock = typeof(SessionDebugConsole).GetMethod("WriteBlock", BindingFlags.NonPublic | BindingFlags.Static);

        try
        {
            Assert.NotNull(writeBlock);

            // Act
            writeBlock!.Invoke(null, ["Monitor", ConsoleColor.Blue, "prompt", "   "]);

            // Assert
            Assert.Contains("    (empty)", output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(previousOutput);
        }
    }
}