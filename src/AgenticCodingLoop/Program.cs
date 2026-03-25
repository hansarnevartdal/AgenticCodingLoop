using AgenticCodingLoop.Features.Bootstrap;
using AgenticCodingLoop.Features.Implementer;
using AgenticCodingLoop.Features.Monitor;
using AgenticCodingLoop.Features.Monitor.Tools;
using AgenticCodingLoop.Features.Reviewer;
using AgenticCodingLoop.Host;
using AgenticCodingLoop.Shared.Runtime;

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
var nextImplementerId = 0;
var nextReviewerId = 0;
var implementerTasks = new List<Task>();
var reviewerTasks = new List<Task>();

try
{
    await using var bootstrap = await BootstrapFeature.ExecuteAsync(config, debugConsole, shutdown.Token);

    // ── Monitor and worker loops ─────────────────────────────────────────────
    Console.WriteLine("═══ Monitoring ═══");
    Console.WriteLine();

    var workerLoopState = new MonitorWorkerStateTool();
    var implementerRole = new WorkerRoleDescriptor<BootstrapFeature, MonitorWorkerStateTool>(
        WorkerType: "implementer",
        CreateFeatureAsync: static async (bootstrapContext, worktreePath, console, workerId) =>
            await ImplementerFeature.CreateAsync(
                bootstrapContext.CliPath,
                bootstrapContext.ClientEnvironment,
                worktreePath,
                bootstrapContext.SourceGitHub,
                bootstrapContext.SourceSkills,
                console,
                workerId),
        IncrementRunningCount: static state => state.IncrementImplementer(),
        DecrementRunningCount: static state => state.DecrementImplementer());
    var reviewerRole = new WorkerRoleDescriptor<BootstrapFeature, MonitorWorkerStateTool>(
        WorkerType: "reviewer",
        CreateFeatureAsync: static async (bootstrapContext, worktreePath, console, workerId) =>
            await ReviewerFeature.CreateAsync(
                bootstrapContext.CliPath,
                bootstrapContext.ClientEnvironment,
                worktreePath,
                bootstrapContext.SourceGitHub,
                bootstrapContext.SourceSkills,
                console,
                workerId),
        IncrementRunningCount: static state => state.IncrementReviewer(),
        DecrementRunningCount: static state => state.DecrementReviewer());
    await using var monitorLoop = await MonitorFeature.CreateAsync(bootstrap.Client, bootstrap.SourceSkills, workerLoopState, debugConsole, config.MaxParallel);

    while (!shutdown.Token.IsCancellationRequested)
    {
        await ReapCompletedTasks(implementerTasks, "implementer");
        await ReapCompletedTasks(reviewerTasks, "reviewer");

        MonitorDecision decision;
        try
        {
            decision = await monitorLoop.ExecuteAsync(shutdown.Token);
        }
        catch (Exception ex) when (ex is TimeoutException or InvalidOperationException)
        {
            Console.Error.WriteLine($"Monitor error (will retry next cycle): {ex.Message}");
            await Task.Delay(TimeSpan.FromSeconds(30), shutdown.Token);
            continue;
        }

        var implementersToStart = Math.Max(0, Math.Min(decision.ImplementersToStart, config.MaxParallel - implementerTasks.Count));
        for (var i = 0; i < implementersToStart; i++)
        {
            var workerId = nextImplementerId++;
            Console.WriteLine($"  Monitor: starting implementer worker {workerId}.");
            Console.WriteLine();

            implementerTasks.Add(RunWorker(
                implementerRole,
                workerId,
                bootstrap,
                config,
                debugConsole,
                workerLoopState,
                shutdown.Token));
        }

        var reviewersToStart = Math.Max(0, Math.Min(decision.ReviewersToStart, config.MaxParallel - reviewerTasks.Count));
        for (var i = 0; i < reviewersToStart; i++)
        {
            var workerId = nextReviewerId++;
            Console.WriteLine($"  Monitor: starting reviewer worker {workerId}.");
            Console.WriteLine();

            reviewerTasks.Add(RunWorker(
                reviewerRole,
                workerId,
                bootstrap,
                config,
                debugConsole,
                workerLoopState,
                shutdown.Token));
        }

        // Give workers time to make progress before the monitor checks again.
        await Task.Delay(TimeSpan.FromSeconds(30), shutdown.Token);
    }
}
catch (TimeoutException ex)
{
    Console.Error.WriteLine($"Agent timed out: {ex.Message}");
    shutdown.Cancel();
    failed = true;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Agent failed: {ex.Message}");
    shutdown.Cancel();
    failed = true;
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
}
finally
{
    Console.CancelKeyPress -= cancelHandler;
    shutdown.Cancel();
    await WaitForWorkers(implementerTasks, reviewerTasks);
}

return failed ? 1 : 0;

// Keep the worker lifecycle close to the outer orchestration loop so the demo's control flow
// stays readable in one file.
static async Task RunWorker(
    WorkerRoleDescriptor<BootstrapFeature, MonitorWorkerStateTool> role,
    int workerId,
    BootstrapFeature bootstrap,
    WorkspaceConfig config,
    SessionDebugConsole debugConsole,
    MonitorWorkerStateTool workerLoopState,
    CancellationToken ct)
{
    var worktreePath = await WorktreeManager.CreateAsync(config.RepoDirectory, role.WorkerType, workerId, ct);
    var workerLabel = $"{role.WorkerType} worker {workerId}";
    var workerCounted = false;

    try
    {
        await using var feature = await role.CreateFeatureAsync(bootstrap, worktreePath, debugConsole, workerId);
        role.IncrementRunningCount(workerLoopState);
        workerCounted = true;
        await feature.ExecuteAsync(ct);
    }
    catch (Exception ex) when (!workerCounted && ex is not OperationCanceledException)
    {
        throw new InvalidOperationException($"Failed to start {workerLabel}: {ex.Message}", ex);
    }
    finally
    {
        if (workerCounted)
        {
            role.DecrementRunningCount(workerLoopState);
        }

        try
        {
            await WorktreeManager.RemoveAsync(config.RepoDirectory, worktreePath, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to remove worktree for {workerLabel}: {ex.Message}");
        }
    }
}

static async Task ReapCompletedTasks(List<Task> tasks, string workerType)
{
    // Walk backward so removing a completed task does not shift items we have not visited yet.
    for (var i = tasks.Count - 1; i >= 0; i--)
    {
        if (tasks[i].IsCompleted)
        {
            try
            {
                await tasks[i];
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{workerType} worker failed: {ex.Message}");
            }

            tasks.RemoveAt(i);
        }
    }
}

static async Task WaitForWorkers(List<Task> implementerTasks, List<Task> reviewerTasks)
{
    var workerTasks = implementerTasks.Concat(reviewerTasks).ToArray();
    if (workerTasks.Length is 0)
    {
        return;
    }

    try
    {
        await Task.WhenAll(workerTasks);
    }
    catch (Exception)
    {
        // Individual worker failures are reported when the tasks are reaped below.
    }

    await ReapCompletedTasks(implementerTasks, "implementer");
    await ReapCompletedTasks(reviewerTasks, "reviewer");
}
