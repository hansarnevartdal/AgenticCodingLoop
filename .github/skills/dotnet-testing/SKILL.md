---
name: dotnet-testing
description: Testing expert skill for writing xUnit tests, using FakeItEasy, and applying AAA patterns.
---

# Testing Guidelines

Use this skill when writing, debugging, or planning tests.

## Tech Stack
- **Framework:** [xUnit](https://xunit.net/)
- **Mocking:** [FakeItEasy](https://fakeiteasy.github.io/)
- **Assertions:** xUnit Assertions (no third-party library)
- **Web Integration:** `Microsoft.AspNetCore.Mvc.Testing`
- **Persistence:** Real SQL Server via EF Core

## Naming Conventions (BDD-style)
- **Class:** `When_<Scenario_Or_Action>` (e.g., `When_adding_device`)
- **Method:** `Should_<Expected_Outcome>` (e.g., `Should_call_repository_add`)

## Test Structure (AAA)
All tests must follow **Arrange, Act, Assert**:
```csharp
[Fact]
public async Task Should_succeed()
{
    // Arrange
    var input = "test";
    var mock = A.Fake<IService>();

    // Act
    var result = await SystemUnderTest.DoWork(input);

    // Assert
    Assert.True(result);
}
```

## Mocking (FakeItEasy)
- **Create:** `var fake = A.Fake<IMyService>();`
- **Setup:** `A.CallTo(() => fake.Method(A<int>.Ignored)).Returns(true);`
- **Verify:** `A.CallTo(() => fake.Method(expected)).MustHaveHappened();`

## Test Categories
1. **Unit Tests** (`*.Core.Tests`):
   - Isolate business logic.
   - Mock all external dependencies.
   - **Example:**
     ```csharp
     // Arrange
     var repo = A.Fake<IDeviceRepository>();
     var agent = new DeviceAgent(repo);
     // Act
     await agent.AddDevice(...);
     // Assert
     A.CallTo(() => repo.Add(...)).MustHaveHappened();
     ```

2. **API Integration** (`*.Web.Tests`):
   - Use `WebApplicationFactory`.
   - Use Refit client for API calls.
   - Use InMemory database (default) or mocked services.

3. **Controller Tests** (`*.Web.Tests`):
   - Unit tests for Controllers (not full integration).
   - Mock Queries/Commands.
   - Assert `ActionResult` types.

4. **Persistence Tests** (`*.Persistence.IntegrationTests`):
   - Real SQL Server.
   - Do NOT mock the database.

## Core Principles
- **Never modify production code solely to simplify testing.** Tests must adapt to production code, not the other way around.
- Handle test infrastructure issues (e.g., disposal race conditions) within test fixtures, not by adding test-only flags to production.
