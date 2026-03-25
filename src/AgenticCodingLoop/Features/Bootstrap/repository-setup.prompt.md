<!-- Sent to the setup session during bootstrap. -->

Set up the repository for development.

1. {{cloneStep}}
2. Ensure these GitHub labels exist on the repo (create any that are missing):
   - `status:in-progress` (color: 1d76db, description: "Issue is being implemented")
   - `status:ready-for-review` (color: 0e8a16, description: "PR is ready for review")
   - `status:needs-work` (color: e11d48, description: "PR needs changes after review")
   - `status:approved` (color: 28a745, description: "PR approved and ready to merge")
   - `claimed` (color: d93f0b, description: "Work item claimed by a worker")

   Use `gh label list --json name` (inside the repo directory) to check, then `gh label create` for missing ones.
3. Remove stale `claimed` labels from any open issues or PRs that have them (leftover from a previous run):
   - Run `gh issue list --state open --label claimed --json number --jq '.[].number'` and remove the label from each.
   - Run `gh pr list --state open --label claimed --json number --jq '.[].number'` and remove the label from each.
4. List open issues: `gh issue list --state open` and report whether there is work to do.