# Runtime Flow

This document explains the runtime flow in the order it happens so the demo can be narrated step by step.

## 1. Parse Inputs

The process starts with one required input and one optional input:

- a GitHub repository URL
- an optional local working folder override

`Host/WorkspaceConfig` validates that the URL points at GitHub and derives the local clone path from the repository name. If no working folder is provided, it uses `%LOCALAPPDATA%/AgenticCodingLoop`.

## 2. Execute the Bootstrap Feature

Before the outer loop starts, the bootstrap feature locates the app's own `.github` folder. That folder provides:

- agent definitions in `.github/agents`
- shared skills in `.github/skills`

This is important because the target repository can be any GitHub repo; it does not need to carry the orchestration setup itself.

## 3. Start the Bootstrap Client

The first `CopilotClient` is rooted at the working folder. This client runs a free-model bootstrap session that can either:

- clone the repository into the working folder
- or fetch and pull if the repository already exists locally

The same bootstrap session also ensures the required labels exist.

## 4. Prepare GitHub Labels

The bootstrap step makes sure these labels are present:

- `status:in-progress`
- `status:ready-for-review`
- `status:needs-work`
- `status:approved`

Those labels are the shared language between the monitor, implementer, and reviewer.

## 5. Start the Working Client

After bootstrap, the app creates a second `CopilotClient` rooted inside the cloned repository. This matters because the worker sessions must run Git and GitHub commands from the actual repo directory.

Steps 2 through 5 are owned by `BootstrapContext`, so `Program.cs` does not need to manage those startup details directly.

## 6. Create Sessions by Role

The app then creates three long-lived sessions:

- monitor session on `gpt-5-mini`
- implementer session on `gpt-5.4`
- reviewer session on `claude-opus-4.6`

The implementer and reviewer sessions load agent definitions through `ConfigDir` and shared skills through `SkillDirectories`.

## 7. Enter the Outer Monitoring Loop

The main loop in `Program.cs` keeps running until cancellation.

Each pass does three things:

1. Observe whether an existing worker task completed or failed.
2. Ask the monitor loop whether implementation or review work exists.
3. Start the missing worker loop if the monitor says it should run.

This is the core orchestration pattern: cheap polling outside, expensive work only on demand.

## 8. Monitor Decides What Should Run

The monitor loop asks GitHub two simple questions:

- are there open issues or PRs that imply implementation work?
- are there PRs labeled `status:ready-for-review`?

It returns a decision object instead of acting directly. That keeps the monitor focused and predictable.

## 9. Implementer Runs One Iteration at a Time

When implementation work exists, the implementer loop wakes up and performs one iteration. The current priority order is:

1. Merge an approved PR.
2. Fix a PR that needs work.
3. Start work on the next open issue without an active PR.

After each iteration, it reports back and immediately decides whether another implementation iteration is still justified.

## 10. Reviewer Runs One Iteration at a Time

When review work exists, the reviewer loop wakes up and performs one review iteration:

1. find a PR labeled `status:ready-for-review`
2. inspect the PR and linked issue context
3. approve it or request changes
4. update the PR status label accordingly

It then immediately decides whether another review iteration is still justified.

## 11. Worker Loops Stop Themselves

The worker loops are not permanent daemons. Each one exits when its own kind of work disappears.

That behavior is intentional:

- the monitor stays alive all the time
- the expensive sessions do not sit idle on their own timer
- the monitor can restart them later if new work appears

## 12. Cancellation and Exit

When the user cancels the process, the app:

- stops starting new work
- lets the current operation wind down or abort cleanly
- exits without trying to do extra end-of-run work accounting

That keeps the demo shutdown behavior simple and easy to explain.

## One Issue-to-Merge Example

1. A GitHub issue exists and is open.
2. The monitor sees open issue work and starts the implementer loop.
3. The implementer creates a branch, writes code, pushes it, and opens a PR.
4. The implementer marks the PR `status:ready-for-review`.
5. The monitor sees review work and starts the reviewer loop.
6. The reviewer inspects the PR.
7. If the PR is good, the reviewer approves it and marks it `status:approved`.
8. The monitor sees approved implementation work and wakes the implementer again.
9. The implementer merges the PR.
10. GitHub closes the linked issue through the PR body.

That closed loop is the main story the demo is trying to teach.