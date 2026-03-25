<!-- Sent to the reviewer session for one review iteration. -->

Check the repository for pull requests that are ready for review and perform one review iteration.

Use the loaded `github-cli` skill for command details and review workflow.
Use the `report_key_event` tool for major milestones only so the host can log them to the main console. Report concise, high-signal events such as selecting a PR, approving a review, requesting changes, commenting on the linked issue, or finding no review work. Do not use it for every shell command.

Start by deciding whether any review work is actually available right now.
Skip any PR that has the `claimed` label — another worker is already handling it.
If there is no review work available, first report an `idle` key event, then call the `signal_no_more_review_work` tool immediately with a short reason instead of only stating that there is nothing to do.

## Claiming Work

When you select a PR to review, immediately add the `claimed` label to it BEFORE starting the review. This prevents other parallel workers from picking up the same PR.
When you finish reviewing (whether completing the review, calling `signal_no_more_review_work`, or moving on), remove the `claimed` label from any PR you claimed this iteration.

Your job is to:

1. Find pull requests that are marked ready for review.
2. Read the pull request and any linked issue context needed to judge the work correctly.
3. Review the change for correctness, completeness, and code quality.
4. If the work is acceptable, submit a formal GitHub approval review, and set the label `status:approved` on the PR.
5. Do not stop at writing a comment like "looks good", "no issues", or "still good to merge". Those comments are not enough by themselves. When the PR is acceptable, you must leave it in an explicitly approved state that the implementer can detect.
6. If the work needs changes, request changes with specific actionable feedback, and set the label `status:needs-work` on the PR.
7. When approving or requesting changes, also post a comment on the linked issue summarizing the review outcome. Use `gh issue comment <number> --body "..."` to explain what was found and what the next step is.

## Label Mutual Exclusivity

The `status:*` labels are mutually exclusive. When you set a new status label, ALWAYS remove ALL other `status:*` labels first. The set of status labels is: `status:in-progress`, `status:ready-for-review`, `status:needs-work`, `status:approved`.
For example, when setting `status:approved`, first remove `status:in-progress`, `status:ready-for-review`, and `status:needs-work` if present using `--remove-label`, then add `status:approved` using `--add-label`.
When setting `status:needs-work`, first remove `status:in-progress`, `status:ready-for-review`, and `status:approved` if present.
Note: The `claimed` label is NOT a status label. It is independent and must be managed separately.

Each worker starts in a detached checkout of `main` for isolation. Review in this worktree as-is; do not create or switch branches unless the task explicitly requires it.
Review one iteration only, then re-check whether any review work still remains right now.
If no review work remains after your iteration, report an `idle` key event and call the `signal_no_more_review_work` tool before you finish.
If more review work still remains, do not call the tool. Just stop and report what you did in this iteration.
If nothing is ready for review at the start, report an `idle` key event and call the `signal_no_more_review_work` tool immediately instead of only saying so.