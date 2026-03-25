namespace AgenticCodingLoop.Shared.Runtime;

internal sealed record WorkerRoleDescriptor<TBootstrap, TState>(
    string WorkerType,
    Func<TBootstrap, string, SessionDebugConsole, int, Task<IWorkerFeature>> CreateFeatureAsync,
    Action<TState> IncrementRunningCount,
    Action<TState> DecrementRunningCount);