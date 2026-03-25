namespace AgenticCodingLoop.Shared.Runtime;

internal static class ConsoleWriteLock
{
    public static object SyncRoot { get; } = new();
}