using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PhotoCatalog.Domain.EventBus;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.EventBus;

public class TestEvent : Object;

public class TestHandler : IHandler
{
    public ValueTask<ResultVoid> Handle(TestEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return new ValueTask<ResultVoid>(ResultVoid.Success());
    }
}

public class EventBusTest
{
    private readonly PhotoCatalog.Domain.EventBus.EventBus _eventBus = new();

    [Fact]
    public async Task Subscribe_AddingEventAnd50Handlers_SuccessfullyAdds50HandlersToEvent()
    {
        const int threadsCount = 50;

        TestEvent testEvent = new();
        TestHandler testHandler = new();

        using var barrier = new Barrier(threadsCount);

        var tasks = Enumerable
            .Range(0, threadsCount)
            .Select(_ => Task.Run(() =>
            {
                barrier.SignalAndWait();

                _eventBus.Subscribe(testEvent, testHandler);
            }));

        await Task.WhenAll(tasks);

        var pairs = _eventBus.EventHandlers;

        var handlers = pairs.Values.First();

        Assert.True(pairs.ContainsKey(testEvent.GetType()));
        Assert.Equal(threadsCount, handlers.Count);
    }

    [Fact]
    public async Task Unsubscribe_DeletingAdded50Handlers_SuccessfullyDeletes50Handlers()
    {
        const int threadsCount = 50;

        TestEvent testEvent = new();
        TestHandler testHandler = new();

        using Barrier barrier = new(threadsCount);

        IEnumerable<Task> subscribeTasks = Enumerable
            .Range(0, threadsCount)
            .Select(_ => Task.Run(() =>
            {
                barrier.SignalAndWait();

                _eventBus.Subscribe(testEvent, testHandler);
            }));

        await Task.WhenAll(subscribeTasks);

        IEnumerable<Task> unsubscribeTasks = Enumerable
            .Range(0, threadsCount)
            .Select(_ => Task.Run(() =>
            {
                barrier.SignalAndWait();

                _eventBus.Unsubscribe(testEvent, testHandler);
            }));

        await Task.WhenAll(unsubscribeTasks);

        ConcurrentDictionary<Type, ImmutableHashSet<IHandler>> pairs2 = _eventBus.EventHandlers;

        ImmutableHashSet<IHandler> handlers = pairs2.Values.First();

        Assert.Empty(handlers);
    }

    [Fact]
    public void Unsubscribe_DeletingHandlerFromIncorrectEvent_ReturnsError()
    {
        ResultVoid result = _eventBus.Unsubscribe(
            new TestEvent(),
            new TestHandler()
        );

        Assert.True(result.IsFailure);
    }
}