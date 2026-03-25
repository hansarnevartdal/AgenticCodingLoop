---
name: playwright-cli
description: Automates browser interactions for web testing, form filling, screenshots, and data extraction. Use when the user needs to navigate websites, interact with web pages, fill forms, take screenshots, test web applications, or extract information from web pages.
---

# Browser Automation with playwright-cli

## Environment Notes

The exact runtime environment depends on where the agent is running. In this repository, prefer PowerShell syntax for surrounding file-system commands when working from Windows terminals.

- `playwright-cli` commands themselves are shell-agnostic.
- Use headless mode when no desktop session is available.
- Pass `--browser chromium` if the environment only has Chromium installed.

## Quick start

```powershell
# open new browser
playwright-cli open
# navigate to a page
playwright-cli goto https://playwright.dev
# interact with the page using refs from the snapshot
playwright-cli click e15
playwright-cli type "page.click"
playwright-cli press Enter
# take a screenshot
playwright-cli screenshot
# close the browser
playwright-cli close
```

## Commands

### Core

```powershell
playwright-cli open
# open and navigate right away
playwright-cli open https://example.com/
playwright-cli goto https://playwright.dev
playwright-cli type "search query"
playwright-cli click e3
playwright-cli dblclick e7
playwright-cli fill e5 "user@example.com"
playwright-cli drag e2 e8
playwright-cli hover e4
playwright-cli select e9 "option-value"
playwright-cli upload ./document.pdf
playwright-cli check e12
playwright-cli uncheck e12
playwright-cli snapshot
playwright-cli snapshot --filename=after-click.yaml
playwright-cli eval "document.title"
playwright-cli eval "el => el.textContent" e5
playwright-cli dialog-accept
playwright-cli dialog-accept "confirmation text"
playwright-cli dialog-dismiss
playwright-cli resize 1920 1080
playwright-cli close
```

### Navigation

```powershell
playwright-cli go-back
playwright-cli go-forward
playwright-cli reload
```

### Keyboard

```powershell
playwright-cli press Enter
playwright-cli press ArrowDown
playwright-cli keydown Shift
playwright-cli keyup Shift
```

### Mouse

```powershell
playwright-cli mousemove 150 300
playwright-cli mousedown
playwright-cli mousedown right
playwright-cli mouseup
playwright-cli mouseup right
playwright-cli mousewheel 0 100
```

### Save as

```powershell
playwright-cli screenshot
playwright-cli screenshot e5
playwright-cli screenshot --filename=page.png
playwright-cli pdf --filename=page.pdf
```

### Tabs

```powershell
playwright-cli tab-list
playwright-cli tab-new
playwright-cli tab-new https://example.com/page
playwright-cli tab-close
playwright-cli tab-close 2
playwright-cli tab-select 0
```

### Storage

```powershell
playwright-cli state-save
playwright-cli state-save auth.json
playwright-cli state-load auth.json

# Cookies
playwright-cli cookie-list
playwright-cli cookie-list --domain=example.com
playwright-cli cookie-get session_id
playwright-cli cookie-set session_id abc123
playwright-cli cookie-set session_id abc123 --domain=example.com --httpOnly --secure
playwright-cli cookie-delete session_id
playwright-cli cookie-clear

# LocalStorage
playwright-cli localstorage-list
playwright-cli localstorage-get theme
playwright-cli localstorage-set theme dark
playwright-cli localstorage-delete theme
playwright-cli localstorage-clear

# SessionStorage
playwright-cli sessionstorage-list
playwright-cli sessionstorage-get step
playwright-cli sessionstorage-set step 3
playwright-cli sessionstorage-delete step
playwright-cli sessionstorage-clear
```

### Network

```powershell
playwright-cli route "**/*.jpg" --status=404
playwright-cli route "https://api.example.com/**" --body='{"mock": true}'
playwright-cli route-list
playwright-cli unroute "**/*.jpg"
playwright-cli unroute
```

### DevTools

```powershell
playwright-cli console
playwright-cli console warning
playwright-cli network
playwright-cli run-code "async page => await page.context().grantPermissions(['geolocation'])"
playwright-cli tracing-start
playwright-cli tracing-stop
playwright-cli video-start
playwright-cli video-stop video.webm
```

### Install

```powershell
playwright-cli install --skills
playwright-cli install-browser
```

## Artifact hygiene

- Save screenshots to `.playwright-artifacts/screenshots/`.
- Keep ad-hoc visual artifacts out of the repository root.
- Ensure `.playwright-artifacts/` remains gitignored.

## Sharing screenshots in PRs and issues

The `gh` CLI has no built-in command for attaching image files to issue comments, and `gh gist create` rejects binary files. The GitHub asset upload API (`uploads.github.com`) is not reliably available from CLI environments.

**Reliable method: commit screenshots to the feature branch and embed via a commit-pinned GitHub blob URL with `?raw=1`.**

Screenshots live in `docs/screenshots/` using **standardized, page/component-based names** (e.g., `header.png`, `assistant-page.png`). This makes them living documentation — when the UI changes, the file is updated in place and the PR diff shows the visual change automatically.

> **Legacy files:** Existing `docs/screenshots/issue-*-*.png` assets are considered legacy; do not add new files with issue-prefixed names. When you update a UI area that still uses an `issue-…` screenshot, rename the file to an appropriate component/page-based name and update any references in docs/tests as part of the same PR.

> ⚠️ Review screenshots for sensitive data (credentials, tokens, PII) before committing.

```powershell
# 1. Take a screenshot
playwright-cli screenshot --filename=.playwright-artifacts/screenshots/header.png

# 2. Copy into the tracked docs/screenshots directory using the component/page name
New-Item -ItemType Directory -Path docs/screenshots -Force | Out-Null
Copy-Item .playwright-artifacts/screenshots/header.png docs/screenshots/header.png

# 3. Commit and push
git add docs/screenshots/
git commit -m "docs: update UI screenshot for header"
git push

# 4. Capture the commit SHA for a stable permalink
$commitSha = git rev-parse HEAD

# 5. Embed in the PR description or issue comment using a commit-pinned URL
gh pr comment <PR> --body "## UI Screenshots

![Header](https://github.com/{owner}/{repo}/blob/$commitSha/docs/screenshots/header.png?raw=1)"
```

Prefer the `github.com/.../blob/<sha>/...?...raw=1` form over `raw.githubusercontent.com`. The blob URL works in PRs for both public and private repositories, while raw URLs often fail to render for private repos.

> **Note:** Do not clean up `docs/screenshots/` — screenshots are long-lived documentation and should be updated in place whenever the corresponding UI changes.

### Configuration
```powershell
# Use specific browser when creating session
playwright-cli open --browser=chrome
playwright-cli open --browser=firefox
playwright-cli open --browser=webkit
playwright-cli open --browser=msedge
# Connect to browser via extension
playwright-cli open --extension

# Use persistent profile (by default profile is in-memory)
playwright-cli open --persistent
# Use persistent profile with custom directory
playwright-cli open --profile=/path/to/profile

# Start with config file
playwright-cli open --config=my-config.json

# Close the browser
playwright-cli close
# Delete user data for the default session
playwright-cli delete-data
```

### Browser Sessions

```powershell
# create new browser session named "mysession" with persistent profile
playwright-cli -s=mysession open example.com --persistent
# same with manually specified profile directory (use when requested explicitly)
playwright-cli -s=mysession open example.com --profile=/path/to/profile
playwright-cli -s=mysession click e6
playwright-cli -s=mysession close  # stop a named browser
playwright-cli -s=mysession delete-data  # delete user data for persistent session

playwright-cli list
# Close all browsers
playwright-cli close-all
# Forcefully kill all browser processes
playwright-cli kill-all
```

## Example: Form submission

```powershell
playwright-cli open https://example.com/form
playwright-cli snapshot

playwright-cli fill e1 "user@example.com"
playwright-cli fill e2 "password123"
playwright-cli click e3
playwright-cli snapshot
playwright-cli close
```

## Example: Multi-tab workflow

```powershell
playwright-cli open https://example.com
playwright-cli tab-new https://example.com/other
playwright-cli tab-list
playwright-cli tab-select 0
playwright-cli snapshot
playwright-cli close
```

## Example: Debugging with DevTools

```powershell
playwright-cli open https://example.com
playwright-cli click e4
playwright-cli fill e7 "test"
playwright-cli console
playwright-cli network
playwright-cli close
```

```powershell
playwright-cli open https://example.com
playwright-cli tracing-start
playwright-cli click e4
playwright-cli fill e7 "test"
playwright-cli tracing-stop
playwright-cli close
```

## Specific tasks

* **Request mocking** [references/request-mocking.md](references/request-mocking.md)
* **Running Playwright code** [references/running-code.md](references/running-code.md)
* **Browser session management** [references/session-management.md](references/session-management.md)
* **Storage state (cookies, localStorage)** [references/storage-state.md](references/storage-state.md)
* **Test generation** [references/test-generation.md](references/test-generation.md)
* **Tracing** [references/tracing.md](references/tracing.md)
* **Video recording** [references/video-recording.md](references/video-recording.md)
