---
name: github-cli
description: Expert skill for using GitHub CLI (gh) to interact with GitHub repositories, pull requests, issues, and workflows.
---

# GitHub CLI (gh) Skill

Use this skill when interacting with GitHub through the `gh` command-line tool.

## Prerequisites

- **CLI:** `gh` is installed and available in PATH.
- **Auth:** `gh auth status` succeeds for the current user.
- **Host:** Default host is `github.com` unless the repository uses GitHub Enterprise.

## Authentication

If authentication expires or fails, prefer one of these flows:

```powershell
gh auth login --web

$env:GITHUB_TOKEN | gh auth login --with-token
```

If `gh` is not found after install, reload PATH in the current terminal:

```powershell
$env:Path = [System.Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [System.Environment]::GetEnvironmentVariable('Path', 'User')
```

## Common Operations

### Repository Context

Most commands auto-detect the repository from the current git remote.

```powershell
gh repo view
gh repo view --json nameWithOwner,defaultBranchRef
```

### Pull Requests

```powershell
gh pr list
gh pr view 123
gh pr create --fill
gh pr checkout 123
gh pr comment 123 --body "Looks good overall."
gh pr review 123 --approve --body "Approved"
gh pr review 123 --request-changes --body "Please add input validation."
gh pr merge 123 --squash --delete-branch
```

Use `gh pr comment` for summary feedback and `gh pr review` for approval or request-changes workflows.

### Pull Request Review Comments on Specific Lines

For line-specific review comments, use the GitHub REST API through `gh api`.

```powershell
$repo = gh repo view --json nameWithOwner --jq .nameWithOwner
$commitId = gh pr view 123 --json headRefOid --jq .headRefOid

$body = @{
    body = 'Consider extracting this logic into a helper method.'
    commit_id = $commitId
    path = 'src/File.cs'
    line = 42
    side = 'RIGHT'
} | ConvertTo-Json -Compress

$body | Set-Content -Path pr-review-comment.json -Encoding utf8
gh api "repos/$repo/pulls/123/comments" --method POST --input pr-review-comment.json
Remove-Item pr-review-comment.json
```

Always verify the target line before posting:

```powershell
Select-String -Path 'src/File.cs' -Pattern 'expected code pattern' | Select-Object LineNumber, Line
```

### Issues

```powershell
gh issue list
gh issue view 123
gh issue create --title "Bug: Something broken" --body "Details"
gh issue comment 123 --body "Investigating this now."
gh issue close 123
```

### Actions Workflows

```powershell
gh run list --limit 10
gh run view 123456789
gh run view 123456789 --log
gh run watch 123456789
gh run rerun 123456789
gh run download 123456789 --dir artifacts
```

## Pull Request Review Workflow

For comprehensive pull request reviews:

1. Use `gh pr view <number> --comments` to read the current discussion.
2. Use `gh pr comment` for summary feedback.
3. Use `gh pr review --approve` or `gh pr review --request-changes` for formal review state.
4. Use `gh api repos/{owner}/{repo}/pulls/{number}/comments` for line-specific comments.

## Best Practices

- Prefer non-interactive flags in automation.
- Avoid opening terminal UIs when a JSON or plain-text alternative exists.
- Never print tokens or secrets.
- Review generated URLs and comments for sensitive data before posting.
