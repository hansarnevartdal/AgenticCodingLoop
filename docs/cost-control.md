# Cost Control and Monitor Loop

The monitor loop exists to keep the demo affordable and to make session-level cost boundaries easy to explain.

## The Key Constraint

In the Copilot SDK, the model is chosen per session. That means cost control is mostly about deciding:

- which work deserves its own session
- which model each session should use
- which sessions should stay alive continuously

## Current Model Split

| Session | Model | Cost Intent |
|---------|-------|-------------|
| Setup | `gpt-5-mini` | Free or low-cost operational work |
| Monitor | `gpt-5-mini` | Cheap continuous polling |
| Implementer | `gpt-5.4` | Spend on code generation and repo mutation |
| Reviewer | `claude-opus-4.6` | Spend on higher-judgment review work |

## Why the Monitor Loop Matters

Without the monitor loop, the simplest design would be to leave the implementer and reviewer sessions polling GitHub forever. That would be easy to code, but wasteful:

- expensive models would stay active while idle
- the demo would blur the difference between coordination work and value-producing work
- the cost story would be harder to justify

The monitor loop fixes that by separating coordination from execution.

## What the Monitor Actually Does

The monitor loop is intentionally narrow. It only:

1. checks for open issues or implementation-related PR state
2. checks for ready-for-review PRs
3. returns a compact decision about whether workers should run

It does not review code, write code, or merge anything.

## Why This Is a Good Demo Pattern

The monitor loop teaches a practical orchestration lesson: not every agent task deserves a premium model.

In many systems, you can split work into two categories:

- cheap state observation
- expensive reasoning or content creation

This repo makes that split explicit.

## Worker Loops as Burst Compute

The implementer and reviewer behave more like burst workers than background daemons:

- they start only when the monitor sees work
- they do one iteration at a time
- they continue immediately while same-type work still exists
- they stop when they signal that no more work of their type is currently available
- they can be restarted later by the monitor

That pattern is a useful teaching example beyond this specific repo.

## Cost Control Lessons

### 1. Keep Idle Logic Cheap

Continuous monitoring is almost always simpler than event-driven orchestration in a demo. If you must poll, poll with the cheapest capable model.

### 2. Put Expensive Models Behind Clear Gates

The monitor loop is the gate. If it does not see work, expensive sessions do not run.

### 3. Split Sessions by Responsibility

Setup, monitoring, implementation, and review are separate sessions because they have different cost and capability needs.

### 4. Prefer Small, Deterministic Monitor Prompts

The monitor prompt is basically a decision procedure that returns JSON. That makes it cheap, stable, and easy to explain.

## Tradeoff

This design does add one more moving part: the monitor loop itself. For a demo, that tradeoff is worth it because it makes the cost model legible. For a tiny one-shot automation, you might accept a simpler but less efficient design.

## Demo Sound Bite

The monitor loop is the budget guardrail: a free session keeps watch so the premium sessions only wake up when there is real work to do.