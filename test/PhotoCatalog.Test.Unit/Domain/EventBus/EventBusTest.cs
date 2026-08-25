// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Collections.Immutable;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
//
// using PhotoCatalog.Domain.EventBus;
// using PhotoCatalog.Domain.Primitives;
//
// using Xunit;
//
// namespace PhotoCatalog.Test.Unit.Domain.EventBus;
//
// public class TestHandler : IHandler
// {
//     public ResultVoid Handle(Event domainEvent)
//     {
//         return ResultVoid.Success();
//     }
// }
//
// public class TestHandler2 : IHandler
// {
//     public ResultVoid Handle(Event domainEvent)
//     {
//         return ResultVoid.Success();
//     }
// }
//
// public class TestHandler3 : IHandler
// {
//     public ResultVoid Handle(Event domainEvent)
//     {
//         return ResultVoid.Success();
//     }
// }
//
// public class TestEvent : Event;
//
// public class EventBusTest
// {
//     [Fact]
//     public async Task Subscribe_AddingEventAnd50Handlers_SuccessfullyAdds50HandlersToEvent()
//     {
//         const int threadsCount = 50;
//
//         TestEvent testEvent = new();
//         TestHandler testHandler = new();
//
//         using var barrier = new Barrier(threadsCount);
//
//         var tasks = Enumerable
//             .Range(0, threadsCount)
//             .Select(_ => Task.Run(() =>
//             {
//                 barrier.SignalAndWait();
//
//                 PhotoCatalog.Domain.EventBus.EventBus.Subscribe(testEvent, testHandler);
//             }));
//
//         await Task.WhenAll(tasks);
//
//         var pairs = PhotoCatalog.Domain.EventBus.EventBus.EventHandlers;
//
//         var handlers = pairs.Values.First();
//
//         Assert.True(pairs.ContainsKey(testEvent.GetType()));
//         Assert.Equal(threadsCount, handlers.Count);
//     }
//
//     [Fact]
//     public async Task Unsubscribe_DeletingAdded50Handlers_SuccessfullyDeletes50Handlers()
//     {
//         const int threadsCount = 50;
//
//         TestEvent testEvent = new();
//         TestHandler testHandler = new();
//
//         using Barrier barrier = new(threadsCount);
//
//         IEnumerable<Task> subscribeTasks = Enumerable
//             .Range(0, threadsCount)
//             .Select(_ => Task.Run(() =>
//             {
//                 barrier.SignalAndWait();
//
//                 PhotoCatalog.Domain.EventBus.EventBus.Subscribe(testEvent, testHandler);
//             }));
//
//         await Task.WhenAll(subscribeTasks);
//
//         IEnumerable<Task> unsubscribeTasks = Enumerable
//             .Range(0, threadsCount)
//             .Select(_ => Task.Run(() =>
//             {
//                 barrier.SignalAndWait();
//
//                 PhotoCatalog.Domain.EventBus.EventBus.Unsubscribe(testEvent, testHandler);
//             }));
//
//         await Task.WhenAll(unsubscribeTasks);
//
//         ConcurrentDictionary<Type, ImmutableHashSet<IHandler>> pairs2 =
//             PhotoCatalog.Domain.EventBus.EventBus.EventHandlers;
//
//         ImmutableHashSet<IHandler> handlers = pairs2.Values.First();
//
//         Assert.Empty(handlers);
//     }
//
//     [Fact]
//     public void Unsubscribe_DeletingHandlerFromIncorrectEvent_ReturnsError()
//     {
//         ResultVoid result = PhotoCatalog.Domain.EventBus.EventBus.Unsubscribe(
//             new TestEvent(),
//             new TestHandler()
//         );
//
//         Assert.True(result.IsFailure);
//     }
// }