<!-- Sent to the monitor session once per polling cycle. -->

Check the repository state and decide how many worker loops should run.

The maximum number of parallel workers per type is {{maxParallel}}.
Use the `report_key_event` tool for major findings only so the host can log them to the main console. Report concise, high-signal events such as finding implementer work, finding reviewer work, or confirming that the repository is idle. Do not use it for every command.
Call `get_worker_loop_state` before deciding how many workers to start. It returns the current count of running implementer and reviewer workers.

1. Run `gh issue list --state open --json number,title,labels --limit 50`.
2. Run `gh pr list --state open --json number,title,isDraft,reviewDecision,labels,closingIssuesReferences --limit 50`.
3. For each open PR that is not approved, inspect its reviews, review comments, issue comments, and unresolved review threads. Use `gh pr view <number> --json reviews,comments,latestReviews,reviewDecision` and any other GitHub CLI queries you need.
4. Treat human comments, bot comments, review comments, and unresolved review threads as implementation feedback if they ask for changes, point out a defect, or remain unaddressed. Do not require the PR to have review decision `CHANGES_REQUESTED` or label `status:needs-work` before treating that PR as implementer work.
5. Determine whether each open issue already has an active PR. Treat an open PR as active work for an issue if the PR closes that issue, references that issue, or is clearly the PR for that issue.
6. Exclude any issue or PR that has the `claimed` label - another worker is already handling it.
7. Count how many unclaimed implementation work items exist. An item is implementation work if:
   - it is an open issue that does not already have an active PR
   - it is an open PR that is approved and ready to merge
   - it is an open PR with review decision `CHANGES_REQUESTED`
   - it is an open PR with label `status:approved` or `status:needs-work`
   - it is a non-approved open PR with unresolved or unanswered feedback from humans or bots that still requires code or PR updates
8. Set `implementersToStart` to the number of new implementer workers needed: the number of unclaimed implementation work items, minus the number of already-running implementers (from `get_worker_loop_state`), capped at {{maxParallel}} minus the running count.
9. An open issue with no active PR is always implementation work — start an implementer for it. The only exception is when an issue already has an open PR that is simply waiting for review with no unresolved feedback; in that case, do not start a duplicate implementer for that issue.
10. Count how many unclaimed PRs are ready for review (for example, PRs with label `status:ready-for-review` that do not have the `claimed` label).
11. Set `reviewersToStart` to the number of new reviewer workers needed: the number of unclaimed review-ready PRs, minus the number of already-running reviewers, capped at {{maxParallel}} minus the running count.
12. Do not start a reviewer for PRs that are approved, marked `status:needs-work`, or otherwise not ready for review yet.
13. Set `hasAnyWork` to true if any workers should start now, or if any workers are already running.

Do not reply with JSON.
Before reporting the final decision, emit at least one `report_key_event` call that explains the primary reason for the decision. If work exists but workers are already running at capacity, say that explicitly.
After you have made the decision, call the `report_monitor_decision` tool exactly once with the final values for `implementersToStart`, `reviewersToStart`, and `hasAnyWork`, plus a short reason.
Use the tool even when all values are zero.