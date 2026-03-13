# Agentic Coding Loop

A demo application that uses the [GitHub Copilot SDK](https://www.nuget.org/packages/GitHub.Copilot.SDK) to simulate a software development team working through a Product Requirements Document (PRD).

## How It Works

Three specialized agents collaborate through a sequential workflow:

| Session | Agent | Model | Role |
|---------|-------|-------|------|
| Planner | `planner` | GPT-5.4 (1×) | Analyzes PRD, asks clarifying questions, creates issues |
| Implementer | `implementer` | GPT-5.4 (1×) | Writes code, creates pull requests |
| Reviewer | `reviewer` | Claude Opus 4.6 (3×) | Reviews PRs, approves or requests changes |

### Workflow

1. **Planning** — The planner reads the PRD, asks clarifying questions via CLI, and creates ordered issues.
2. **Implementation** — For each issue (sequentially):
   - The implementer writes code and creates a PR.
   - The reviewer approves or requests changes.
   - If changes requested, the implementer revises (up to 3 cycles).
   - On approval, the PR is merged and the issue is marked done.
3. **Summary** — Final status of all issues and PRs is printed and saved.

All state is stored as markdown files in `.agent-loop/` for full auditability.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [GitHub Copilot CLI](https://docs.github.com/en/copilot) installed and authenticated

## Usage

```bash
dotnet run --project src/AgenticCodingLoop -- <repositoryPath> <prdPath>
```

**Example:**

```bash
dotnet run --project src/AgenticCodingLoop -- ./my-repo ./docs/PRD.md
```

## Output Structure

```
<repositoryPath>/
  .agent-loop/
    issues/
      ISSUE-001.md
      ISSUE-002.md
    pull-requests/
      PR-001.md
    plans/
      refined-plan.md
    logs/
      session-summary.md
```

## Architecture

```
src/AgenticCodingLoop/
  Program.cs                          # Main orchestration loop
  Models/
    IssueStatus.cs                    # Open → InProgress → ReadyForReview → Done
    PullRequestStatus.cs              # Draft → ReadyForReview → NeedsWork → Approved → Merged
    ReviewDecision.cs                 # Approved | ChangesRequested
    TrackedIssue.cs                   # Issue domain model
    TrackedPullRequest.cs             # PR domain model
  Services/
    IIssueTracker.cs                  # Issue operations interface
    IPullRequestTracker.cs            # PR operations interface
    MarkdownIssueTracker.cs           # Markdown-backed issue storage
    MarkdownPullRequestTracker.cs     # Markdown-backed PR storage
  Agents/
    AgentPrompts.cs                   # System prompts per role
    AgentTools.cs                     # Tool definitions for issue/PR operations
```
