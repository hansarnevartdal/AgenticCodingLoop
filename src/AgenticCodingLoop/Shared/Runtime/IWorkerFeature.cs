namespace AgenticCodingLoop.Shared.Runtime;

internal interface IWorkerFeature : IAsyncDisposable
{
    Task ExecuteAsync(CancellationToken ct);
}