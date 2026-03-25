using AgenticCodingLoop.Features.Implementer.Tools;

namespace AgenticCodingLoop.Tests;

[Collection("Console")]
public sealed class When_reporting_loop_key_events
{
    [Fact]
    public void Should_return_a_summary_message_for_the_logged_event()
    {
        // Arrange
        using var output = new StringWriter();
        var previousOutput = Console.Out;
        Console.SetOut(output);

        var signal = new ImplementerEventTool("Implementer", ConsoleColor.Cyan);

        try
        {
            // Act
            var message = signal.ReportKeyEvent("opened-pr", "Opened PR #42 for issue #17.");

            // Assert
            Assert.Contains("Implementer", message, StringComparison.Ordinal);
            Assert.Contains("opened-pr", message, StringComparison.Ordinal);
            Assert.Contains("Opened PR #42 for issue #17.", message, StringComparison.Ordinal);
            Assert.Contains("Implementer opened-pr: Opened PR #42 for issue #17.", output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(previousOutput);
        }
    }
}