# Product Requirements Document

## Product Name

Agentic Coding Loop

## Summary

Build a small example application that uses the GitHub Copilot SDK and a simple outer orchestration loop to simulate a software development team working through a product requirement document. The application should coordinate multiple custom agents, manage work through markdown-based issues and pull requests, and iterate until the PRD is considered complete.

The project is a demo and teaching tool first. The main value is showing the agent loop clearly, not reproducing the full GitHub platform.

## Problem Statement

The GitHub Copilot SDK supports custom agents, but many examples stop at isolated agent interactions. There is a need for a concrete example that demonstrates how multiple specialized agents can collaborate over time through lightweight shared state, with a visible workflow that resembles a real development team.

## Goals

- Demonstrate a practical multi-agent workflow built on the GitHub Copilot SDK.
- Define distinct custom agent roles for planner, implementer, and reviewer.
- Use **different models per session** to demonstrate cost optimization (e.g., GPT-4.1 for routine tasks, GPT-5.4 for coding, Claude Opus 4.6 for review).
- Use markdown files as the backing store for issues, pull requests, and review comments.
- Keep the main orchestration loop simple and visible in Program.cs for demo value.
- Allow the app to operate on an existing local repository path and a PRD markdown file path.
- Show a complete path from PRD refinement to planning, issue creation, implementation, review, and merge.

## Non-Goals

- Full GitHub feature parity.
- Multi-user collaboration or remote synchronization.
- A web UI.
- Complex branching strategies.
- Parallel issue execution.
- Sophisticated project management features such as epics, labels, milestones, assignees, or notifications.

## Target Users

- Developers learning the GitHub Copilot SDK.
- Teams exploring agent-based development workflows.
- Demo audiences who need a concrete, understandable example.

## Core User Journey

1. The user runs the application and provides:
   - A path to a target repository or working folder.
   - A path to a PRD markdown file.
2. The planner agent reads the PRD, identifies gaps, and asks clarifying questions through the CLI.
3. The planner refines the PRD into an actionable plan and creates a sequence of issues.
4. The implementer works the first open issue and creates or updates a pull request record for that work.
5. The reviewer reviews the pull request and either:
   - Approves it, allowing it to be merged.
   - Requests changes, sending the work back to the implementer.
6. The loop continues until the current issue is merged.
7. Only then does the system move to the next issue.
8. The process ends when all issues required to satisfy the PRD are merged and the PRD is marked complete.

## Product Principles

- Keep the loop obvious.
- Prefer explicit state transitions over hidden heuristics.
- Store collaboration history in human-readable markdown.
- Favor deterministic, inspectable behavior over automation depth.
- Keep infrastructure minimal so the agent workflow remains the focus.

## Functional Requirements

### 1. Application Inputs

The application must accept at minimum:

- `repositoryPath`: path to the folder or repository the agents will work in.
- `prdPath`: path to the input PRD markdown file.

The application should validate both inputs before starting the workflow.

### 2. Custom Agent Roles

The system must define custom agents based on the GitHub Copilot SDK custom agents capability.

Required initial agents:

- `planner`
- `implementer`
- `reviewer`

#### Planner Responsibilities

- Read and interpret the PRD.
- Ask clarifying questions through the CLI when requirements are ambiguous or incomplete.
- Refine the PRD into a concrete implementation plan.
- Break the plan into ordered issues.
- Create initial issue records in the markdown-backed tracker.
- Decide when the overall PRD is complete.

#### Implementer Responsibilities

- Select the current active issue.
- Make code changes in the target repository.
- Create or update a pull request record tied to the issue.
- Mark the pull request as ready for review.
- Respond to reviewer feedback.
- Merge approved work into the working branch or mainline strategy chosen for the demo.

#### Reviewer Responsibilities

- Review pull request context and changed files.
- Approve work that satisfies the issue requirements.
- Request changes when requirements are unmet, broken, or incomplete.
- Leave review comments in the markdown-backed pull request record.

### 3. PRD Refinement Workflow

Before implementation begins, the system must:

1. Load the PRD markdown file.
2. Let the planner analyze it.
3. Ask follow-up questions to the user through the CLI if needed.
4. Produce a refined execution plan.
5. Generate all initial issues needed for MVP completion.

The refined plan may be persisted as markdown for traceability, but the original PRD should remain the primary input artifact.

### 4. Markdown-Backed Issue Tracking

The system must provide a very basic issue tracker backed by `.md` files.

Required issue operations:

- Create new issue.
- List all issues.
- Read issue.
- Comment on issue.
- Set status of issue.

Each issue should have:

- Unique identifier.
- Title.
- Description.
- Status.
- Acceptance criteria.
- Comments/history.
- Related PR identifier, if one exists.

Recommended initial issue statuses:

- `Open`
- `In Progress`
- `Ready for Review`
- `Done`

### 5. Markdown-Backed Pull Request Tracking

The system must provide a matching basic pull request and review tracker backed by `.md` files.

Required pull request operations:

- Create new pull request.
- List all pull requests.
- Read pull request.
- Comment on pull request.
- Set status of pull request.
- Record review outcome.
- Associate pull request to an issue.

Each pull request should have:

- Unique identifier.
- Title.
- Linked issue identifier.
- Summary of changes.
- Status.
- Review comments/history.
- Review decision.

Recommended initial pull request statuses:

- `Draft`
- `Ready for Review`
- `Needs Work`
- `Approved`
- `Merged`

### 6. Main Orchestration Loop

The main outer loop must live in Program.cs to keep the demo easy to understand.

The loop should:

1. Initialize paths, services, and Copilot SDK clients.
2. Load the PRD.
3. Invoke the planner to refine requirements and create issues.
4. Process issues one at a time in order.
5. For the current issue:
   - Hand off to the implementer.
   - Wait for the issue to reach `Ready for Review`.
   - Hand off to the reviewer.
   - If approved, merge and mark the issue done.
   - If changes are requested, return work to the implementer.
6. Repeat until all issues are done.
7. Emit a final summary.

The tracker logic and markdown storage implementation should be abstracted into separate types so Program.cs remains focused on the workflow.

### 7. Sequential Execution Constraint

For the first version, only one issue may be active at a time.

Rules:

- The next issue must not start until the current issue is merged.
- Only one pull request may be in an active non-merged state at a time.
- This constraint should be explicit in the orchestration logic and documented in the tracker state rules.

### 8. CLI-Based Human Interaction

The application should use the command line interface for user interaction when clarification is required.

CLI interactions should support:

- Displaying planner questions.
- Capturing user answers.
- Showing issue and PR progression.
- Emitting a concise final completion report.

### 9. Auditability and Traceability

The system should make it easy to inspect what happened.

Minimum audit trail requirements:

- Issue history is preserved in markdown.
- Pull request review history is preserved in markdown.
- Planner clarifications and decisions are persisted or logged.
- Final status of each issue and PR is visible from the file system.

## Storage Model

The application should use a simple folder-based storage structure rooted in the working repository or a dedicated working data folder.

Illustrative structure:

```text
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

Markdown files should use a predictable format with lightweight front matter or clearly delimited sections so they are both human-readable and machine-parseable.

## Tooling Requirements

The agents should interact with the issue and pull request system through explicit tools or service methods rather than manipulating markdown files ad hoc.

At minimum, the system must expose issue tools for:

- Create issue
- List issues
- Read issue
- Comment on issue
- Set issue status

At minimum, the system must expose pull request tools for:

- Create pull request
- List pull requests
- Read pull request
- Comment on pull request
- Set pull request status
- Record review decision

## Session and Model Strategy

The GitHub Copilot SDK sets the model at the **session** level. The `CustomAgentConfig` type has `name`, `prompt`, `tools`, `description`, `mcpServers`, and `infer` — but **no `model` property**. All custom agents within a session share the session's model.

Billing is **per prompt × model multiplier**. Key multipliers (paid plans, March 2026):

| Model | Multiplier | Cost Implication |
|-------|-----------|------------------|
| GPT-4.1 | 0x | Free — no premium requests consumed |
| GPT-5.4 | 1x | 1 premium request per prompt |
| Claude Opus 4.6 | 3x | 3 premium requests per prompt |

### Recommended Session Layout

To optimize cost while demonstrating multi-model usage, use **separate sessions per role**:

| Session | Pre-selected Agent | Model | Rationale |
|---------|-------------------|-------|------------|
| Planner session | `planner` | GPT-5.4 (1x) | Needs strong reasoning for PRD analysis |
| Implementer session | `implementer` | GPT-5.4 (1x) | Needs capable model for coding tasks |
| Reviewer session | `reviewer` | Claude Opus 4.6 (3x) | Best judgment for code review |
| Utility session (optional) | — | GPT-4.1 (0x) | Free session for git ops, builds, research |

All sessions share the same `CopilotClient` instance and the same issue/PR tools (registered on each session).

### Can One Session Save Cost?

No. Swapping agents within a single session does not change the model. If you create one session on Claude Opus 4.6 (3x), **every prompt** in that session costs 3 premium requests regardless of which agent handles it. There is no way to make a single prompt cheaper by switching to a sub-agent.

The cost-optimal pattern is multiple sessions with the cheapest model that is capable enough for each role.

### VS Code Copilot vs. Copilot SDK

In **VS Code Copilot**, each `.agent.md` file can declare its own `model` field. When VS Code hands off to an agent with a different model, subsequent prompts are billed at that agent's model rate. So in VS Code, a sub-agent on a free model *does* save premium requests.

The **Copilot SDK** works differently. The SDK's `CustomAgentConfig` has no `model` property; model is per-session. Sub-agents within one SDK session share the session model and therefore share its billing rate. To use different models programmatically, create separate sessions.

This is an important distinction for demo presenters: the VS Code UI and the SDK are separate systems with different model-selection mechanics.

### Session Persistence

The SDK supports session persistence across restarts. For the MVP, sessions are created fresh each run. Session persistence can be added later to support resuming interrupted workflows.

## Technical Design Constraints

- The application should be implemented as a .NET console application.
- The main loop should remain in Program.cs.
- Issue and pull request tracking should be abstracted behind interfaces or service classes.
- Markdown parsing and persistence should be simple and robust.
- The design should allow new agent roles in the future without rewriting the orchestration model.
- Separate sessions should be used for different agent roles to enable per-role model selection.

## Success Criteria

The MVP is successful when:

- A user can point the app at a repository path and a PRD markdown file.
- The planner can refine the PRD and create a set of ordered issues.
- The implementer and reviewer can iterate on a single issue through a markdown-backed PR workflow.
- Approved work is merged before the next issue begins.
- All state transitions are visible in markdown files.
- A full end-to-end run can complete on a small demo repository.

## Out of Scope for MVP

- Real GitHub API integration.
- Real GitHub issues or pull requests.
- Parallel agents working on multiple issues.
- Advanced merge conflict handling.
- Automatic backlog reprioritization.
- Rich diff visualization.
- Fine-grained permission models.

## Milestones

### Milestone 1: Project Skeleton

- Console app bootstrapped.
- Copilot SDK integrated.
- Multiple sessions created with different models.
- Custom agents wired up (planner, implementer, reviewer).

### Milestone 2: Planner and PRD Intake

- PRD file loading.
- CLI clarification flow.
- Refined plan generation.
- Issue generation.

### Milestone 3: Issue and PR Tracker

- Markdown storage model implemented.
- Issue operations implemented.
- Pull request operations implemented.

### Milestone 4: Outer Loop

- Sequential issue processing.
- Implementer and reviewer handoff loop.
- Merge and completion logic.

### Milestone 5: Demo Readiness

- End-to-end run on a small sample repo.
- Clear logs and markdown artifacts.
- Documentation for how to run the demo.

## Risks

- Copilot SDK session orchestration may introduce complexity that obscures the demo.
- Markdown storage can become brittle if file structure is too free-form.
- Reviewer and implementer loops may stall without clear state transition rules.
- PRD refinement quality will depend on prompt quality and CLI clarification flow.

## Open Questions

- Should the system work directly on the target repository, or on a temporary clone/worktree for safety?
- Should merge be represented only in markdown state, or should the app also perform actual git merges?
- Should planner-generated issues be editable by the user before execution starts?
- Should the refined plan be written back into the original PRD or stored as a separate derived artifact?
- How much of the review should be file-diff aware versus issue/PR summary based for the MVP?

## Acceptance Criteria

1. Running the app with a repository path and PRD path starts a guided workflow.
2. The planner can ask at least one clarification question through the CLI when needed.
3. The planner creates a set of markdown issue files from the PRD.
4. The implementer can create and update a markdown pull request for the current issue.
5. The reviewer can approve or request changes on that pull request.
6. A requested-changes review sends work back to the implementer.
7. An approved pull request is marked merged and the linked issue is marked done.
8. The next issue does not begin until the previous one is merged.
9. The system can finish when all issues are complete.
10. All issue and pull request history remains inspectable as markdown files.
11. At least two different models are used across sessions to demonstrate cost optimization.

## Appendix A: Billing Reference

This section documents the billing model so demo presenters can explain cost trade-offs.

### How SDK Billing Works

- The Copilot SDK communicates with the Copilot CLI server via JSON-RPC.
- Each `SendAndWaitAsync` call counts as **one prompt**.
- Cost = 1 premium request × the session's **model multiplier**.
- Model is fixed per session. Switching agents within a session does not change the model or cost.

### Model Multiplier Table (Paid Plans, March 2026)

| Model | Multiplier | Notes |
|-------|-----------|-------|
| GPT-4.1 | 0x | Included free — ideal for simple tasks |
| GPT-4o | 0x | Included free |
| GPT-5 mini | 0x | Included free |
| GPT-5.4 | 1x | Good balance of capability and cost |
| Claude Sonnet 4.6 | 1x | Alternative 1x option |
| Claude Opus 4.6 | 3x | Premium — best for review/judgment tasks |
| Claude Haiku 4.5 | 0.33x | Cheapest premium option |

### Cost Example for One Issue Cycle

Assuming a single issue requires ~5 planner prompts, ~15 implementer prompts, and ~3 reviewer prompts:

| Role | Prompts | Model | Multiplier | Premium Requests |
|------|---------|-------|-----------|------------------|
| Planner | 5 | GPT-5.4 | 1x | 5 |
| Implementer | 15 | GPT-5.4 | 1x | 15 |
| Reviewer | 3 | Claude Opus 4.6 | 3x | 9 |
| **Total** | **23** | | | **29** |

If all 23 prompts ran on Claude Opus 4.6 in a single session, cost would be 23 × 3 = **69** premium requests — more than double.

### Key Takeaway

Use the cheapest model that is capable enough for each role. Reserve expensive models for tasks that genuinely benefit from them.