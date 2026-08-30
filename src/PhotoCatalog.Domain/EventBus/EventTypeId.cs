namespace PhotoCatalog.Domain.EventBus;

internal static class EventTypeId<TEvent>
{
    public static readonly int Id = EventTypeIdCounter.GetNextId();
}