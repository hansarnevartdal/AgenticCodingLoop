using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;

namespace AgenticCodingLoop.Shared.Runtime;

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
            // The worker sessions are non-interactive by design, so permissions are auto-approved.
            OnPermissionRequest = PermissionHandler.ApproveAll
        };
}