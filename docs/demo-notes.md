# Demo Notes and Key Learnings

This repository is optimized for teaching, not for hiding complexity behind infrastructure.

## What the Demo Is Good At

- showing how multiple Copilot sessions can cooperate over time
- demonstrating that GitHub can act as the workflow state store
- making model and session boundaries visible
- illustrating cost-aware orchestration with a cheap monitor loop
- keeping the main control flow understandable in one file

## Key Learnings

### 1. Session Design Is Architectural Design

With the Copilot SDK, choosing session boundaries is not just an implementation detail. It affects:

- cost
- model choice
- working directory context
- how much state each role can retain

That is why setup, monitor, implementer, and reviewer are separate.

### 2. Real Systems of Record Simplify Demos

Using GitHub issues, PRs, labels, and reviews directly removes the need for a second tracking system. That makes the demo easier to trust because the audience can verify state in the GitHub UI.

### 3. A Thin Orchestrator Is Easier To Explain

The project intentionally keeps orchestration in `Program.cs` and avoids a deep service layer. That would be questionable in some production systems, but it is the right tradeoff for a demo where visibility matters more than indirection.

### 4. Skills Are Better Than Repeating Procedures Everywhere

The worker loops use loaded skills for detailed Git and GitHub CLI behavior instead of embedding every command sequence into every prompt. That keeps prompts shorter and reduces instruction drift.

### 5. Non-Interactive Automation Needs Guardrails

The runtime disables interactive Git and GitHub prompts and does not enable SDK user input requests. That is necessary for an unattended demo loop, but it also means prompts and workflow constraints must be clear.

### 6. Tool Calls Beat Free-Text for Control Flow

Every loop communicates structured results through typed tool calls rather than expecting the model to format text in a specific way. The monitor calls `report_monitor_decision`, the workers call `signal_no_more_implementation_work` or `signal_no_more_review_work`, and all loops call `report_key_event` for console output.

This is a practical pattern worth highlighting: when the host needs to act on a model's output, a tool call with validated parameters is more reliable than parsing free-text or JSON from the response body.

## Limitations To Be Honest About

- the orchestration is polling-based, not event-driven
- there is no advanced merge-conflict handling
- only one implementer and one reviewer loop are modeled
- there is no separate planning phase or backlog management inside the app beyond the lightweight issue-plan comments in the implementation prompt
- quality still depends heavily on issue quality and repository test coverage

These are acceptable limitations because the demo is trying to teach the loop, not solve every software delivery problem.

## Suggested Demo Narrative

1. Show the user input: GitHub URL, plus the optional working-folder override if you want to mention it.
2. Show that the app clones a real repository and creates workflow labels.
3. Explain that the monitor loop runs cheaply and continuously.
4. Walk through one issue becoming one PR.
5. Show the reviewer approve or request changes in GitHub.
6. Show the implementer merge an approved PR.
7. Close by pointing out that the whole history is visible in GitHub.

## Where To Extend It Next

If this demo were expanded, the most natural next steps would be:

- event-driven wakeups instead of polling
- richer prioritization rules
- stronger failure recovery and retry handling
- typed parsing around GitHub CLI responses
- more than one worker per role

## Short Takeaway

The interesting part of the repo is not that agents can write code. It is that a small, explicit orchestration layer can turn several specialized sessions into a comprehensible delivery loop.