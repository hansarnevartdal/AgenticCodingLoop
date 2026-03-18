using AgenticCodingLoop;
using AgenticCodingLoop.Configuration;
using AgenticCodingLoop.Bootstrap;
using AgenticCodingLoop.Loops;

// ── Validate inputs ──────────────────────────────────────────────────────────
var config = WorkspaceConfig.Parse(args);
if (config is null) { return 1; }

var debugConsole = new SessionDebugConsole(config.Debug);

using var shutdown = new CancellationTokenSource();

ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    if (shutdown.IsCancellationRequested) { return; }

    Console.WriteLine();
    Console.WriteLine("Cancellation requested. Stopping after the current operation...");
    shutdown.Cancel();
};

Console.CancelKeyPress += cancelHandler;

var failed = false;

try
{
    await using var bootstrap = await BootstrapContext.CreateAsync(config, debugConsole, shutdown.Token);

    // ── Monitor and worker loops ─────────────────────────────────────────────
    Console.WriteLine("═══ Monitoring ═══");
    Console.WriteLine();

    await using var monitorLoop = await MonitorLoop.CreateAsync(bootstrap.Client, bootstrap.SourceSkills, debugConsole);
    await using var implementationLoop = await ImplementationLoop.CreateAsync(bootstrap.Client, bootstrap.SourceGitHub, bootstrap.SourceSkills, debugConsole);
    await using var reviewLoop = await ReviewLoop.CreateAsync(bootstrap.Client, bootstrap.SourceGitHub, bootstrap.SourceSkills, debugConsole);
    Task? implementerTask = null;
    Task? reviewerTask = null;

    while (!shutdown.Token.IsCancellationRequested)
    {
        if (implementerTask is { IsCompleted: true })
        {
            await implementerTask;
            implementerTask = null;
        }

        if (reviewerTask is { IsCompleted: true })
        {
            await reviewerTask;
            reviewerTask = null;
        }

        var decision = await monitorLoop.CheckAsync(shutdown.Token);

        if (implementerTask is null && decision.StartImplementer)
        {
            Console.WriteLine("  Monitor: starting implementer loop.");
            Console.WriteLine();

            implementerTask = implementationLoop.RunAsync(shutdown.Token);
        }

        if (reviewerTask is null && decision.StartReviewer)
        {
            Console.WriteLine("  Monitor: starting reviewer loop.");
            Console.WriteLine();

            reviewerTask = reviewLoop.RunAsync(shutdown.Token);
        }

        await Task.Delay(TimeSpan.FromSeconds(2), shutdown.Token);
    }
}
catch (TimeoutException ex)
{
    Console.Error.WriteLine($"Agent timed out: {ex.Message}");
    failed = true;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Agent failed: {ex.Message}");
    failed = true;
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
}
finally
{
    Console.CancelKeyPress -= cancelHandler;
}

return failed ? 1 : 0;
