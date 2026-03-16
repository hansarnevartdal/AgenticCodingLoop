# GitHub Interaction

The application interacts with GitHub as both a collaboration surface and a state machine. That is the core architectural choice behind the demo.

## Why GitHub Is the Source of Truth

Instead of storing workflow state in local files or a custom database, the app relies on existing GitHub concepts:

- issues describe work to be done
- pull requests carry proposed changes
- labels signal workflow state
- reviews capture approval or change requests
- merge state closes the delivery loop

This means a human can inspect the entire workflow without looking inside the app.

## What the App Does Through GitHub

### Repository Setup

The bootstrap session uses Git and `gh` to:

- clone the target repository
- fetch and pull existing clones
- ensure required labels exist
- check whether open issues exist

### Issue Discovery

The monitor and implementer use `gh issue list` and `gh issue view` to determine whether new implementation work exists and to read issue details.

### Pull Request Discovery

The monitor, implementer, and reviewer use `gh pr list` and `gh pr view` to inspect open PRs, review decisions, labels, and linked issue context.

### Review Execution

The reviewer uses `gh pr diff` and `gh pr review` to inspect and evaluate ready-for-review changes.

### Merge Execution

The implementer performs real merges through `gh pr merge`. This is important for the demo because it shows an end-to-end workflow using actual repository state, not simulated transitions.

## Workflow Labels

These labels are created if missing during bootstrap:

| Label | Meaning |
|-------|---------|
| `status:in-progress` | An issue or related work item is actively being implemented |
| `status:ready-for-review` | A PR is waiting for reviewer attention |
| `status:needs-work` | A PR received review feedback and needs another implementation pass |
| `status:approved` | A PR is approved and ready to merge |

The labels are not just cosmetic. They are part of the workflow contract that the loops use when deciding what to do next.

## Review State and Label State Work Together

The runtime checks both:

- GitHub review decisions such as `APPROVED` or `CHANGES_REQUESTED`
- explicit labels such as `status:approved` or `status:needs-work`

That combination makes the demo more robust because it can react to the native GitHub review state while still exposing a simple visual state machine in the UI.

## Why There Is No GitHub Service Layer

The project intentionally does not wrap GitHub in a custom abstraction layer. For a demo, that is a benefit:

- the agent prompts can work directly with real `gh` commands
- the visible orchestration remains small
- there is less code to explain
- the demo stays close to how a developer would investigate a repo manually

For a product, you might add stronger abstractions or typed integrations. For a demo, the directness is useful.

## How the Local Clone and GitHub Stay Aligned

The working client is rooted inside the cloned repository, so when an agent runs Git or GitHub commands it operates in the correct repository context. That avoids accidental cross-repo state and keeps branch, commit, push, review, and merge operations grounded in the actual local clone.

## Demo Talking Point

The strongest message here is simple: the app does not invent a parallel workflow system. It plugs agents into the workflow developers already understand in GitHub.