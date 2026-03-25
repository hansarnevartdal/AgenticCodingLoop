---
name: ui-changes
description: >
  MANDATORY for any change to `.razor`, `.razor.css`, layout, navigation, or style files.
  Verifies UI changes with Playwright screenshots and documents them in docs/screenshots/.
  Must be invoked before pushing — not optional.
---

# UI Changes Skill

> ⚠️ **REQUIRED for every PR that touches UI files** (`.razor`, `.razor.css`, layout, navigation, styles). This skill **must be invoked** before pushing changes — not optional.
>
> **Trigger condition:** You modified or plan to modify any file matching `*.razor`, `*.razor.css`, or any layout/navigation/style file → **invoke this skill immediately**.

Use this skill whenever you modify **any user-facing UI** — components, pages, layouts, styles, or navigation.

## Requirements

All UI changes must:

1. **Be verified with Playwright** — take a screenshot after the change is visible in the running app.
2. **Update `docs/screenshots/`** — save the screenshot using the standardized component/page name.
3. **Include screenshots in the PR** — embed them in the PR description using commit-pinned GitHub blob URLs with `?raw=1`.

## Screenshot Convention

Store all screenshots in `docs/screenshots/` using the **component or page name**, not issue numbers or ad-hoc names. This makes screenshots living documentation that stays accurate across UI iterations.

### Naming examples

| Component / Page | File |
|---|---|
| Top navigation header | `docs/screenshots/header.png` |
| Assistant chat page | `docs/screenshots/assistant-page.png` |
| Repositories listing | `docs/screenshots/repositories-page.png` |
| Planning/projects view | `docs/screenshots/planning-page.png` |
| Actions / workers view | `docs/screenshots/actions-page.png` |

When a UI change is committed, update the relevant screenshot file in place. The same filename means the PR diff will show the visual change automatically — no cleanup step needed.

### Multiple states of the same component

When a component has distinct visual states that are all worth documenting, use a suffix to distinguish them:

| State | File |
|---|---|
| Header (default/collapsed) | `docs/screenshots/header.png` |
| Header (mobile / expanded menu) | `docs/screenshots/header-expanded.png` |
| Modal (open) | `docs/screenshots/repository-modal-open.png` |

Use a suffix only when the states are meaningfully different from a UX perspective. A plain screenshot of the default state is always the primary file (`<name>.png`).

## Workflow

```powershell
# 1. Start the app locally and open it in Playwright
playwright-cli open http://localhost:5056

# 2. Navigate to the changed page/component and take a screenshot
playwright-cli screenshot --filename=.playwright-artifacts/screenshots/header.png

# 3. Copy to docs/screenshots/ using the standard name
New-Item -ItemType Directory -Path docs/screenshots -Force | Out-Null
Copy-Item .playwright-artifacts/screenshots/header.png docs/screenshots/header.png

# 4. Commit alongside your UI change
git add docs/screenshots/header.png
git commit -m "docs: update header screenshot"
git push

# 5. Capture the commit SHA for a stable permalink
$commitSha = git rev-parse HEAD

# 6. Add screenshots as a PR comment
gh pr comment <PR> --body "## UI Screenshots

![Header](https://github.com/{owner}/{repo}/blob/$commitSha/docs/screenshots/header.png?raw=1)"
```

Use the `github.com/.../blob/<sha>/...?...raw=1` form instead of `raw.githubusercontent.com`. It works for public repositories and also renders correctly in private-repository PRs where raw URLs can break.

See the [playwright-cli skill](../playwright-cli/SKILL.md) for full screenshot and browser automation commands.

## Related Skills

- [playwright-cli](../playwright-cli/SKILL.md) — browser automation and screenshot capture
