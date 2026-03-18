# Agentic Coding Loop

Agentic Coding Loop is a demo application that uses the [GitHub Copilot SDK](https://www.nuget.org/packages/GitHub.Copilot.SDK) to show a small multi-agent delivery workflow running against a real GitHub repository.

The demo is intentionally simple: GitHub is the system of record, the orchestration stays visible in `Program.cs`, and the monitor loop keeps the expensive sessions dormant until there is real work to do.

## Run It

Command:

```bash
dotnet run --project src/AgenticCodingLoop -- [--debug] <githubRepoUrl> [workingFolder]
```

Example:

```bash
dotnet run --project src/AgenticCodingLoop -- https://github.com/owner/my-repo
```

Debug mode:

```bash
dotnet run --project src/AgenticCodingLoop -- --debug https://github.com/owner/my-repo
```

Optional override:

```bash
dotnet run --project src/AgenticCodingLoop -- https://github.com/owner/my-repo ./tmp
```

What happens:

1. The app clones or refreshes the target repository into `%LOCALAPPDATA%/AgenticCodingLoop/<repo-name>/` by default.
2. It ensures the workflow labels exist in GitHub.
3. It starts a cheap monitor loop.
4. It starts implementer or reviewer loops only when GitHub state says there is work.

What you need before running:

- a GitHub repository with open issues to work on
- `gh` authenticated against GitHub
- Copilot CLI installed on PATH and authenticated for SDK-backed sessions

Stop the app with `Ctrl+C`.

## Current Setup

The current runtime is split into four session types:

| Session | Model | Purpose |
|---------|-------|---------|
| Bootstrap | GPT-5 mini | Clone or refresh the target repository and ensure labels exist |
| Monitor | GPT-5 mini | Watch GitHub state and decide when worker loops should run |
| Implementer | GPT-5.4 | Merge approved PRs, fix needs-work PRs, or implement open issues |
| Reviewer | Claude Opus 4.6 | Review ready-for-review PRs and approve or request changes |

The current repo setup is:

- the target repository is cloned into a Local AppData working folder by default
- orchestration stays in `src/AgenticCodingLoop/Program.cs`
- agents are loaded from this repo's `.github/agents`
- shared skills are loaded from this repo's `.github/skills`
- GitHub issues, pull requests, labels, and review decisions are the workflow state
- the runtime is non-interactive and disables terminal prompts for `git` and `gh`

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [GitHub Copilot CLI](https://docs.github.com/en/copilot) installed and authenticated
- [GitHub CLI (`gh`)](https://cli.github.com/) installed and authenticated

Recommended auth checks:

```bash
gh auth status
copilot auth status
```

If the target repository already exists under the working folder, the bootstrap session runs fetch and pull instead of cloning again.

## Documentation

- [Architecture](docs/architecture.md)
- [Runtime Flow](docs/flow.md)
- [GitHub Interaction](docs/github-integration.md)
- [Cost Control and Monitor Loop](docs/cost-control.md)
- [Demo Notes and Key Learnings](docs/demo-notes.md)

## Quick Overview

All work state lives in GitHub through issues, pull requests, labels, and review decisions.

## For Demo Presenters

If you only need the speaking points:

1. Start with [docs/architecture.md](docs/architecture.md) to explain the moving parts.
2. Use [docs/flow.md](docs/flow.md) to narrate one complete issue-to-merge cycle.
3. Use [docs/github-integration.md](docs/github-integration.md) to explain why GitHub is the state store.
4. Use [docs/cost-control.md](docs/cost-control.md) to explain why the monitor loop matters.
5. Close with [docs/demo-notes.md](docs/demo-notes.md) for tradeoffs, limitations, and lessons learned.
