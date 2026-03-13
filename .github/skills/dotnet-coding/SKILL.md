---
name: dotnet-coding
description: Expert C# developer skill for implementing features, refactoring code, and ensuring .NET best practices (C# 13, DDD, Minimal APIs).
---

# C# Development Guidelines

Use this skill when writing, refactoring, or reviewing C# code.

## Core Language Standards
- Use **.NET 10+** and **C# 13+** features where appropriate.
- **Modern Constructs:**
  - File-scoped namespaces
  - Records and record types
  - Pattern matching
  - Primary constructors
  - Collection expressions
  - Top-level statements (where appropriate)
  - `Span<T>` for performance-critical code
- **Async:** Do NOT postfix methods with "Async".
- **Nullability:** Treat warnings as errors; use `!` sparingly.

## Architecture & DDD
- **Minimal APIs:** Prefer Minimal APIs over Controllers for new endpoints. Group endpoints in `Web.Api` namespace.
- **API Naming:** Use hierarchical paths for related operations (e.g., `/admin/sync/{id}`, `/admin/sync/full`) for natural sorting and discoverability.
- **Typed Results:** Use `TypedResults` for return values.
- **Domain-Driven Design (DDD):** Keep core logic isolated from external concerns (HTTP, Database).
- **Abstractions:** Prefer meaningful abstractions. If an abstraction hinders the task, question/refactor it rather than hacking around it.
- **Dependencies:** **Fix the root cause.** If a dependency is deficient, suggest fixing it there instead of local workarounds.
- **SOLID:** Follow Single Responsibility and Dependency Inversion.

## Code Hygiene
- **Norwegian Quality:** Validate grammar, spelling, singular/plural agreement in all user-facing text.
- **Cleanliness:** Remove unused code immediately (unused usings, variables, methods).
- **Cleanup Dependencies:** When removing code, check if methods/classes it used are now unused. If so, remove those too (cascade cleanup).
- **XML Documentation:** Only add XML comments for external-facing interfaces (NuGet packages, public APIs). Internal code does not require XML documentation.
- **Member Order:** Constants → static fields → fields → constructors → properties → public → internal → private → nested types.
- **Naming:** 
  - Classes/Interfaces: PascalCase (`CustomerService`)
  - Methods/Properties: PascalCase (`Execute`)
  - Parameters/Locals: camelCase (`customerId`)
  - Private Fields: camelCase without prefix (`customer`)
- **Signatures:** 
  - Parameter order: Required first, then optional.
  - Return types: Prefer specific types or tuples `(byte[] Data, string Name)`.

## Refactoring Pattern
Use guard clauses to flatten nesting.

**Example:**
```csharp
// ✅ Good: Guard clauses, early returns
public void ProcessOrder(Order order)
{
    if (order is null) { return; }
    if (!order.IsValid) { return; }
    // Process with confidence
}
```

## Code Quality
- **Control Flow:** Always use braces `{}`. For single-line guards, braces on same line is acceptable: `if (x is null) { return; }`
- **Equality:** Use `is` for reference type equality.
- **Logging:** Use structured logging with proper levels:
  - `LogTrace`/`LogDebug`: Diagnostics/Internal.
  - `LogInformation`: General flow.
  - `LogWarning`: Unexpected but handled.
  - `LogError`: Exceptions.
  - `LogCritical`: App crashes.
  - **NEVER** log secrets, tokens, or PII.

## Validation & Exceptions
- **Input Validation:** Fail fast with `ArgumentNullException` etc. at public boundaries.
- **Exceptions:** Do NOT catch `Exception` unless at top level. Do NOT swallow errors.

**Example:**
```csharp
if (id is null) throw new ArgumentNullException(nameof(id));
if (customer is null) throw new CustomerNotFoundException(id);
```

## Database & Persistence
- **Date Properties:** New `DateTime` columns MUST be suffixed with `Utc` (e.g., `LastSynchronizedUtc`). Existing columns are not renamed.
- **Migrations:** ALWAYS use `dotnet ef migrations add <Name>` command. Do NOT manually generate migration files.
- **Bulk Operations:** Prefer EF Core's `ExecuteUpdateAsync`/`ExecuteDeleteAsync` over raw SQL for atomic updates. These are type-safe and refactoring-friendly.
- **Raw SQL:** Avoid `ExecuteSqlRawAsync` unless EF Core lacks support. When required, use parameterized queries.
- **Time:** Use `DateTime.UtcNow` (never `DateTime.Now`). Prefer injecting `TimeProvider` for testability.
- **Timestamp Policy:** Persist all timestamps in UTC (database/contracts) and convert to local time only in UI display code.

## Definition of Done (APIs)
- Add request to `Web.http` for manual testing.
- Add Refit interface to `Web.Client` project.
- Add unit tests calling static implementation.
- Add integration tests calling API via Refit client.
