using System.Threading;

namespace PhotoCatalog.Domain.EventBus;

internal static class EventTypeIdCounter
{
    private static int _nextId;

    public static int GetNextId() => Interlocked.Increment(ref _nextId) - 1;
}