using AgenticCodingLoop.Shared.Prompts;

namespace AgenticCodingLoop.Tests;

public sealed class When_loading_prompts
{
    [Theory]
    [InlineData("Features.Implementer.implementation-loop.prompt")]
    [InlineData("Features.Reviewer.review-loop.prompt")]
    [InlineData("Features.Monitor.monitor-loop.prompt")]
    [InlineData("Features.Bootstrap.repository-setup.prompt")]
    public void Should_load_embedded_prompt_resource(string promptName)
    {
        var text = PromptLoader.Load(promptName);

        Assert.False(string.IsNullOrWhiteSpace(text));
    }

    [Fact]
    public void Should_replace_placeholders()
    {
        // Arrange / Act
        var text = PromptLoader.Load("Features.Monitor.monitor-loop.prompt", ("maxParallel", "5"));

        // Assert
        Assert.Contains("parallel workers per type is 5", text, StringComparison.Ordinal);
        Assert.DoesNotContain("{{maxParallel}}", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_reference_the_implementer_specific_stop_tool()
    {
        // Arrange / Act
        var text = PromptLoader.Load("Features.Implementer.implementation-loop.prompt");

        // Assert
        Assert.Contains("signal_no_more_implementation_work", text, StringComparison.Ordinal);
        Assert.DoesNotContain("signal_no_more_work", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_reference_the_reviewer_specific_stop_tool()
    {
        // Arrange / Act
        var text = PromptLoader.Load("Features.Reviewer.review-loop.prompt");

        // Assert
        Assert.Contains("signal_no_more_review_work", text, StringComparison.Ordinal);
        Assert.DoesNotContain("signal_no_more_work", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_throw_for_unknown_prompt()
    {
        // Arrange / Act
        var ex = Assert.Throws<InvalidOperationException>(() => PromptLoader.Load("nonexistent.prompt"));

        // Assert
        Assert.Contains("not found", ex.Message, StringComparison.Ordinal);
    }
}
