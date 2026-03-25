<!-- Sent to the implementer session for one work iteration. -->

Check the repository state and perform one implementation iteration.

Use the loaded `github-cli` and `git` skills for command details and conventions.
If the issue affects user-facing UI or includes screenshots of a UI bug, use the loaded `playwright-cli` skill to test the running app and verify the UI behavior before handoff. If you change UI files, follow the loaded `ui-changes` skill requirements for screenshot verification.

Use the `report_key_event` tool for major milestones only so the host can log them to the main console. Report concise, high-signal events such as selecting an issue or PR, posting the plan comment, opening or updating a PR, merging a PR, closing an administrative issue, or finding no actionable work. Do not use it for every shell command.

Start by deciding whether any implementation work is actually available right now.
Skip any issue or PR that has the `claimed` label — another worker is already handling it.
Treat an issue as implementation work only if it does not already have an active open PR. If an issue is already represented by an open PR that is awaiting review or currently under review, that is not new implementation work.
For open PRs, do not rely only on labels or review decision. You must also inspect reviews, review comments, issue comments, and unresolved review threads to detect feedback from humans or bots that still needs action.
If there is no implementation work available, first report an `idle` key event, then call the `signal_no_more_implementation_work` tool immediately with a short reason instead of only stating that there is nothing to do.

## Claiming Work

When you select an issue or PR to work on, immediately add the `claimed` label to it BEFORE starting any code work. This prevents other parallel workers from picking up the same item.
When you finish working on an item (whether completing the work, calling `signal_no_more_implementation_work`, or moving on), remove the `claimed` label from any issue or PR you claimed this iteration.

Your priorities are:

1. Merge any pull request that is ready to merge because it is approved. When merging, post a comment on the linked issue summarizing the changes that were implemented and the outcome, then close the issue.
2. Inspect open PRs that are not approved for unresolved or unanswered feedback from humans or bots. If a PR has comments, review comments, or unresolved threads that still require changes, pick that PR up even if there is no `CHANGES_REQUESTED` review decision and no `status:needs-work` label.
3. When working on PR feedback, address comments that are still unresolved or unanswered, push the update, and move the PR back to ready-for-review.
4. If there is no higher-priority PR work, pick the next open issue that does not already have an active PR, update your local `main`, create a fresh branch from `main` for that issue, implement the work there, open a PR, and mark it ready for review.
5. If an issue is an administrative task that does not require code changes or a PR (e.g., planning, creating new issues, updating documentation outside the repo), complete the task directly, post a comment summarizing what was done, and close the issue. No branch or PR is needed for these.

## Issue Comments

When you pick up a new issue for implementation, post a comment on the issue with your proposed plan before writing any code. The comment should outline the approach, files to change, and any design decisions. Use `gh issue comment <number> --body "..."` for this.
When you merge a PR that completes an issue, post a closing comment on the issue summarizing what was done, what changed, and the PR number. Then close the issue.
When you complete an administrative issue that did not require a PR, post a closing comment summarizing the outcome, then close the issue directly with `gh issue close <number>`.

## Label Mutual Exclusivity

The `status:*` labels are mutually exclusive. When you set a new status label, ALWAYS remove ALL other `status:*` labels first. Use `gh issue edit` or `gh pr edit` with `--remove-label` before `--add-label`. The set of status labels is: `status:in-progress`, `status:ready-for-review`, `status:needs-work`, `status:approved`.
For example, when marking an issue `status:in-progress`, first remove any of the other three status labels that may be present.
When marking a PR `status:ready-for-review`, first remove `status:in-progress`, `status:needs-work`, and `status:approved` if present.
Note: The `claimed` label is NOT a status label. It is independent and must be managed separately.

## Branch and Label Rules

Each worker starts in a detached checkout of `main` for isolation. Before making code changes for a new issue, create and switch to a fresh branch from `main` in this worktree.
For every new issue, always create a new branch based on the current `main`. Do not branch from an older issue branch, a review-fix branch, or any branch other than `main`.
When a PR has outstanding feedback that you are picking up, make sure the PR carries the label `status:needs-work` while you are working on it (and remove all other status labels).
When you create or update a PR that should wait for review, make sure the PR carries the label `status:ready-for-review` (and remove all other status labels).
When you are actively working on an issue, make sure the issue carries the label `status:in-progress` (and remove all other status labels).
Follow the repository's labels and GitHub review state as the source of truth.
Treat already-answered comments and already-resolved threads as done. Do not repeat work that has already been addressed.
Do not pick up the same issue again just because the issue remains open while its PR is being reviewed.
Keep the work scoped to one iteration only, then re-check whether any implementation work still remains right now.
If no implementation work remains after your iteration, report an `idle` key event and call the `signal_no_more_implementation_work` tool before you finish.
If more implementation work still remains, do not call the tool. Just stop and report what you did in this iteration.