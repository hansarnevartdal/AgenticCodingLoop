using AgenticCodingLoop.Features.Implementer;
using AgenticCodingLoop.Features.Reviewer;
using AgenticCodingLoop.Shared.Runtime;

namespace AgenticCodingLoop.Tests;

public sealed class When_describing_worker_loop_roles
{
    [Fact]
    public void Should_describe_the_implementer_role_explicitly()
    {
        // Arrange
        var definition = ImplementerFeature.Definition;

        // Assert
        Assert.Equal("Implementer", definition.AgentDisplayName);
        Assert.Equal("implementer", definition.AgentMode);
        Assert.Equal("implementation", definition.WorkType);
        Assert.Equal(CopilotModels.Implementer, definition.Model);
        Assert.Equal("Features.Implementer.implementation-loop.prompt", definition.PromptName);
        Assert.Equal(TimeSpan.FromMinutes(60), definition.PromptTimeout);
        Assert.Equal(ConsoleColor.Cyan, definition.AgentColor);
    }

    [Fact]
    public void Should_describe_the_reviewer_role_explicitly()
    {
        // Arrange
        var definition = ReviewerFeature.Definition;

        // Assert
        Assert.Equal("Reviewer", definition.AgentDisplayName);
        Assert.Equal("reviewer", definition.AgentMode);
        Assert.Equal("review", definition.WorkType);
        Assert.Equal(CopilotModels.Reviewer, definition.Model);
        Assert.Equal("Features.Reviewer.review-loop.prompt", definition.PromptName);
        Assert.Equal(TimeSpan.FromMinutes(60), definition.PromptTimeout);
        Assert.Equal(ConsoleColor.Magenta, definition.AgentColor);
    }
}