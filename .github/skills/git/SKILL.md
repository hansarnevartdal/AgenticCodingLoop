---
name: git
description: Git conventions for branch naming, commit messages, merge strategies, rebasing, and best practices. Use when working with Git operations, branches, commits, or pull request workflows.
---

# Git Conventions

This skill provides Git conventions and best practices for the repository.

## Branch Naming

Use short-lived topic branches with clear, English, kebab-case names.

### Feature Branches

```
feature/<description>
```

Examples: `feature/add-user-authentication`

Produces version label: `beta-<BranchName>`

### Hotfix Branches

```
hotfix/<description>
```

Examples: `hotfix/critical-security-vulnerability`

Produces version label: `beta-<BranchName>`

### Guidelines

- Use **kebab-case** for descriptions
- Keep descriptions **short and meaningful**
- **Write descriptions in English**
- Avoid special characters except hyphens

### Branch Management Policy

- **Never commit directly to `main` or `master`**
- **Always work on branches**
- **All changes via GitHub pull request**

## Commit Messages

### Format

```
<imperative verb> <concise description>
```

### Examples

✅ **Good:**

- `Add user authentication feature`
- `Fix null reference error in login`
- `Refactor customer service for better performance`

❌ **Bad:**

- `Added user authentication feature` (past tense)
- `Adding user authentication feature` (present continuous)
- `User authentication feature` (not a verb)

### Guidelines

- Start with imperative verb (Add, Fix, Update, Refactor, Remove, etc.)
- Keep first line under 72 characters
- Avoid generic messages like "fix bug" or "update code"

### Multi-line Commits

For complex changes:

```
Add user authentication with JWT tokens

- Implement JWT token generation and validation
- Add middleware for token verification
- Update login endpoint to return tokens
```

## Pull Request Descriptions

Keep pull request descriptions **minimal** (2-4 sentences):

```
<What changed>. <Why it changed>.
```

**Guidelines:**

- **2-4 sentences maximum**
- Focus on **what** and **why**
- Do NOT list files or changes
- Do NOT include test results

## Merge Strategies

### When to Rebase

Use `git rebase` when:

- Updating your feature branch with latest changes from main
- Cleaning up commit history before creating or updating a pull request
- Working on a solo feature branch

```powershell
git checkout feature/1234-my-feature
git fetch origin
git rebase origin/main
```

### When to Merge

Use `git merge` when:

- Merging a reviewed pull request into main
- Combining work from multiple developers
- Preserving exact commit history is important

### Handling Rebase Conflicts

1. Git will pause at each conflict
2. Resolve conflicts in the affected files
3. Stage the resolved files: `git add <file>`
4. Continue the rebase: `git rebase --continue`
5. If things go wrong: `git rebase --abort`

### Non-Interactive Rebase (Automation/Terminal)

When running git commands in terminals or scripts, avoid editor prompts that block execution:

```powershell
# Continue rebase without opening editor (accepts current message)
git -c core.editor=true rebase --continue

# Alternative: set GIT_EDITOR environment variable
$env:GIT_EDITOR = 'true'; git rebase --continue

# For commits, use --no-edit to keep existing message
git commit --amend --no-edit

# For merge commits without editor
git merge --no-edit <branch>
```

**Key flags:**
- `--no-edit` - Keep existing commit message (for amend, merge, rebase)
- `-c core.editor=true` - Temporarily set editor to no-op
- `GIT_EDITOR=true` - Environment variable alternative

### Squashing Commits

```powershell
# Interactive rebase to squash last 3 commits
git rebase -i HEAD~3
```

**When to squash:**

- Multiple "fix typo" or "WIP" commits
- Experimental commits that should be one logical change
- Before final pull request submission for cleaner history

**When NOT to squash:**

- Commits that represent distinct logical changes
- Commits already pushed and reviewed

### Amending Commits

```powershell
# Amend the last commit (not yet pushed)
git commit --amend

# Amending after pushing requires force-push
git commit --amend
git push --force-with-lease
```

## Best Practices

### Commits

- Make **atomic commits** (one logical change per commit)
- Commit **frequently** with meaningful messages
- Keep commits **focused** on a single concern
- **Test** before committing

### Branches

- Keep branches **short-lived**
- **Sync** with main branch regularly
- **Delete** branches after merge
- Avoid long-running feature branches

### Collaboration

- **Pull** before starting new work
- **Rebase** instead of merge when appropriate
- **Communicate** about large refactorings
- **Review** your own changes before pushing

### Generated Artifacts

- Include EF Core migrations, Bicep parameter snapshots, and other generated files **only** when required for the change
- Regenerate artifacts from a clean state; never edit generated code by hand
- Keep generated files in dedicated folders (`Database/Migrations`, etc.)
- If a generated artifact changes due to tool updates, call it out in the pull request description

## Troubleshooting

**Merge conflicts:** `git fetch origin; git rebase origin/main`, resolve conflicts, `git add`, `git rebase --continue`, `git push --force-with-lease`

**Rollback:** `git revert <hash>` or create a follow-up pull request that reverts the change