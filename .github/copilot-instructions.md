# GitHub Copilot Instructions for C# Projects

## Overview

This document provides high-level guidance for GitHub Copilot code generation in the C# solution. All detailed conventions are defined in the referenced instruction files below.

## Core Principles

- Use **C# 13+** with modern language features
- Follow **SOLID** and **Domain-Driven Design (DDD)** principles
- **Architecture:** Fix root causes in dependencies; avoid local workarounds
- Prioritize **readability**, **maintainability**, **performance**, and **security**
- Generate **concise code examples** - reference unchanged code rather than repeating it

## Environment & Tooling

- **Terminal Commands:** When running on Windows, use **PowerShell** syntax (e.g., `Select-String`, `Get-ChildItem`). On Unix-based systems, use standard shell commands.

## Related Skills

For detailed guidelines on specific aspects of development, refer to the Agent Skills:

- **[C# & Coding Standards](skills/dotnet-coding/SKILL.md)** - Code cleanliness, refactoring, naming conventions, and C# language features
- **[Testing Guidelines](skills/dotnet-testing/SKILL.md)** - xUnit, FakeItEasy, and Refit patterns
- **[Git Conventions](skills/git/SKILL.md)** - Branching, commit hygiene, rebasing, and pull request workflow
- **[Safe Mutation Policy](instructions/safety.instructions.md)** - Safe mutation and confirmation requirements
- **[GitHub CLI](./skills/github-cli/SKILL.md)** - Using `gh` CLI for PRs, issues, comments, and repository operations

## ⚠️ UI Changes — Mandatory Skill

> **CRITICAL:** Whenever you modify **any** user-facing UI file — including `.razor`, `.razor.css`, layout components, navigation, or styles — you **MUST** invoke the **[ui-changes skill](skills/ui-changes/SKILL.md)** before pushing.

This applies to every change that touches the visual output of the application. No exceptions.

The skill requires:
1. Taking a Playwright screenshot of the affected page/component
2. Saving it to `docs/screenshots/` using the standard component name
3. Embedding the screenshot in the PR description

Failure to invoke this skill means the PR is incomplete.

---

By following these guidelines, GitHub Copilot will help generate C# code that is modern, efficient, maintainable, and secure. Developers should use this document as a reference to ensure consistency across all code contributions.
