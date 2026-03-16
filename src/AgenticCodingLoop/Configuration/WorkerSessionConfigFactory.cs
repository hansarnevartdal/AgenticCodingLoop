using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Configuration;

internal static class WorkerSessionConfigFactory
{
    public static SessionConfig Create(
        string model,
        string sourceGitHub,
        string sourceSkills,
        string reasoningEffort,
        ICollection<AIFunction> tools) => new()
        {
            Model = model,
            ConfigDir = sourceGitHub,
            Tools = tools,
            SkillDirectories = [sourceSkills],
            ReasoningEffort = reasoningEffort,
            OnPermissionRequest = PermissionHandler.ApproveAll
        };
}