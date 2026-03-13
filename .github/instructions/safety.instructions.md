# Safe Mutation Policy

## Async Agent Behavior

### Standard Operations (Auto-Execute)
After presenting a clear plan, **automatically proceed** with:
- Creating/editing files
- Creating branches/PRs/commits
- Running builds/tests
- Managing work items

**Pattern:** Plan → Execute → Report

### Destructive Operations (Require Confirmation)
**Always require explicit confirmation** for:
- **Deleting** files/branches/items
- **Force-pushing**, **Reverting**, or **Hard resetting**
- **Closing** PRs unmerged

**Pattern:** Warning → Wait for "Yes" → Execute

## Security & Secrets
- **Always mask** secrets, tokens, and sensitive data.
- **Redact** API keys, passwords, connection strings.
- **Never log** authentication tokens.
- **Validate** that sensitive data is not in diffs or outputs.
