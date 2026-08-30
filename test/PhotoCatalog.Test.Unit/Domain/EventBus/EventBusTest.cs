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

public abstract class TestEvent(string value)
{
    public string Value { get; } = value;
}

public sealed class FirstTestEvent(string value) : TestEvent(value);

public sealed class SecondTestEvent(string value) : TestEvent(value);

public sealed class ThirdTestEvent(string value) : TestEvent(value);

public sealed class FourthTestEvent(string value) : TestEvent(value);

public sealed class FifthTestEvent(string value) : TestEvent(value);

public sealed class SixthTestEvent(string value) : TestEvent(value);

public sealed class SeventhTestEvent(string value) : TestEvent(value);

public sealed class EighthTestEvent(string value) : TestEvent(value);

public sealed class NinthTestEvent(string value) : TestEvent(value);

public sealed class TenthTestEvent(string value) : TestEvent(value);

public abstract class TestHandler<TEvent> : IHandler<TEvent>
{
    public ValueTask<ResultVoid> HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return new ValueTask<ResultVoid>(ResultVoid.Success());
    }
}

public sealed class FirstTestHandler : TestHandler<FirstTestEvent>;

public sealed class FirstSecondTestHandler : TestHandler<FirstTestEvent>;

public sealed class SecondTestHandler : TestHandler<SecondTestEvent>;

public sealed class ThirdTestHandler : TestHandler<ThirdTestEvent>;

public sealed class FourthTestHandler : TestHandler<FourthTestEvent>;

public sealed class FifthTestHandler : TestHandler<FifthTestEvent>;

public sealed class SixthTestHandler : TestHandler<SixthTestEvent>;

public sealed class SeventhTestHandler : TestHandler<SeventhTestEvent>;

public sealed class EighthTestHandler : TestHandler<EighthTestEvent>;

public sealed class NinthTestHandler : TestHandler<NinthTestEvent>;

public sealed class TenthTestHandler : TestHandler<TenthTestEvent>;

public sealed class EventBusTest
{
    private static readonly PhotoCatalog.Domain.EventBus.EventBus EventBus = new();
    private static readonly FirstTestHandler FirstTestHandler = new();
    private static readonly FirstSecondTestHandler FirstSecondTestHandler = new();
    private static readonly SecondTestHandler SecondTestHandler = new();
    private static readonly ThirdTestHandler ThirdTestHandler = new();
    private static readonly FourthTestHandler FourthTestHandler = new();
    private static readonly FifthTestHandler FifthTestHandler = new();
    private static readonly SixthTestHandler SixthTestHandler = new();
    private static readonly SeventhTestHandler SeventhTestHandler = new();
    private static readonly EighthTestHandler EighthTestHandler = new();
    private static readonly NinthTestHandler NinthTestHandler = new();
    private static readonly TenthTestHandler TenthTestHandler = new();


    [Fact]
    private void Subscribe_SubscribingGivenHandlers_ReturnsSuccess()
    {
        for (int i = 0; i < 10; i++) Assert.True(EventBus.Subscribe(FirstTestHandler).IsSuccess);

        Assert.True(EventBus.Subscribe(FirstSecondTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(SecondTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(ThirdTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(FourthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(FifthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(SixthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(SeventhTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(EighthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(NinthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(TenthTestHandler).IsSuccess);
    }

    [Fact]
    private void Subscribe_SubscribingNull_ReturnsFailure()
    {
        FirstTestHandler? firstTestHandler = null;

        ResultVoid result = EventBus.Subscribe(firstTestHandler);

        Assert.True(result.IsFailure);
        Assert.Equal(result.ResultError, SystemErrors.NullArgument);
    }

    [Fact]
    private void Unsubscribe_UnsubscribingGivenHandlers_ReturnsSuccess()
    {
        for (int i = 0; i < 10; i++) Assert.True(EventBus.Unsubscribe(FirstTestHandler).IsSuccess);

        Assert.True(EventBus.Unsubscribe(FirstSecondTestHandler).IsSuccess);
        Assert.True(EventBus.Unsubscribe(SecondTestHandler).IsSuccess);
        Assert.True(EventBus.Unsubscribe(ThirdTestHandler).IsSuccess);
        Assert.True(EventBus.Unsubscribe(FourthTestHandler).IsSuccess);
        Assert.True(EventBus.Unsubscribe(FifthTestHandler).IsSuccess);
        Assert.True(EventBus.Unsubscribe(SixthTestHandler).IsSuccess);
        Assert.True(EventBus.Unsubscribe(SeventhTestHandler).IsSuccess);
        Assert.True(EventBus.Unsubscribe(EighthTestHandler).IsSuccess);
        Assert.True(EventBus.Unsubscribe(NinthTestHandler).IsSuccess);
        Assert.True(EventBus.Unsubscribe(TenthTestHandler).IsSuccess);
    }

    [Fact]
    private void Unsubscribe_UnsubscribingHandlerThatNotExists_ReturnsFailure()
    {
        FirstTestHandler firstTestHandler = new();

        ResultVoid result = EventBus.Unsubscribe(firstTestHandler);

        Assert.True(result.IsFailure);
        Assert.Equal(result.ResultError, DomainErrors.EventBus.HandlerNotFound);
    }

    [Fact]
    private void Unsubscribe_UnsubscribingGivenNull_ReturnsFailure()
    {
        FirstTestHandler? firstTestHandler = null;

        ResultVoid result = EventBus.Unsubscribe(firstTestHandler);

        Assert.True(result.IsFailure);
        Assert.Equal(result.ResultError, SystemErrors.NullArgument);
    }

    [Fact]
    private async Task PublishAsync_PublishingEventForExistingHandlers_ReturnsSuccess()
    {
        const int threadCount = 20;

        ValueTask<ResultVoid> firstResult = new();
        ValueTask<ResultVoid> secondResult = new();
        ValueTask<ResultVoid> thirdResult = new();
        ValueTask<ResultVoid> fourthResult = new();
        ValueTask<ResultVoid> fifthResult = new();
        ValueTask<ResultVoid> sixthResult = new();
        ValueTask<ResultVoid> seventhResult = new();
        ValueTask<ResultVoid> eighthResult = new();
        ValueTask<ResultVoid> ninthResult = new();
        ValueTask<ResultVoid> tenthResult = new();

        for (int i = 0; i < 10; i++) Assert.True(EventBus.Subscribe(FirstTestHandler).IsSuccess);

        Assert.True(EventBus.Subscribe(FirstSecondTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(SecondTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(ThirdTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(FourthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(FifthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(SixthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(SeventhTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(EighthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(NinthTestHandler).IsSuccess);
        Assert.True(EventBus.Subscribe(TenthTestHandler).IsSuccess);

        using var startGate = new ManualResetEventSlim(false);
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        var tasks = Enumerable.Range(0, threadCount)
            .Select(_ => Task.Factory.StartNew(() =>
            {
                startGate.Wait(cancellationToken);

                firstResult = EventBus.PublishAsync(
                    new FirstTestEvent("Test1"),
                    cancellationToken
                );
                secondResult = EventBus.PublishAsync(
                    new SecondTestEvent("Test2"),
                    cancellationToken
                );
                thirdResult = EventBus.PublishAsync(
                    new ThirdTestEvent("Test3"),
                    cancellationToken
                );
                fourthResult = EventBus.PublishAsync(
                    new FourthTestEvent("Test4"),
                    cancellationToken
                );
                fifthResult = EventBus.PublishAsync(
                    new FifthTestEvent("Test5"),
                    cancellationToken
                );
                sixthResult = EventBus.PublishAsync(
                    new SixthTestEvent("Test6"),
                    cancellationToken
                );
                seventhResult = EventBus.PublishAsync(
                    new SeventhTestEvent("Test7"),
                    cancellationToken
                );
                eighthResult = EventBus.PublishAsync(
                    new EighthTestEvent("Test8"),
                    cancellationToken
                );
                ninthResult = EventBus.PublishAsync(
                    new NinthTestEvent("Test9"),
                    cancellationToken
                );
                tenthResult = EventBus.PublishAsync(
                    new TenthTestEvent("Test10"),
                    cancellationToken
                );
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default))
            .ToArray();

        startGate.Set();

        await Task.WhenAll(tasks);

        Assert.True(firstResult.Result.IsSuccess);
        Assert.True(secondResult.Result.IsSuccess);
        Assert.True(thirdResult.Result.IsSuccess);
        Assert.True(fourthResult.Result.IsSuccess);
        Assert.True(fifthResult.Result.IsSuccess);
        Assert.True(sixthResult.Result.IsSuccess);
        Assert.True(seventhResult.Result.IsSuccess);
        Assert.True(eighthResult.Result.IsSuccess);
        Assert.True(ninthResult.Result.IsSuccess);
        Assert.True(tenthResult.Result.IsSuccess);

        EventBus.Unsubscribe(FirstTestHandler);
        EventBus.Unsubscribe(FirstSecondTestHandler);
        EventBus.Unsubscribe(SecondTestHandler);
        EventBus.Unsubscribe(ThirdTestHandler);
        EventBus.Unsubscribe(FourthTestHandler);
        EventBus.Unsubscribe(FifthTestHandler);
        EventBus.Unsubscribe(SixthTestHandler);
        EventBus.Unsubscribe(SeventhTestHandler);
        EventBus.Unsubscribe(EighthTestHandler);
        EventBus.Unsubscribe(NinthTestHandler);
        EventBus.Unsubscribe(TenthTestHandler);
    }

    [Fact]
    private async Task PublishAsync_PublishingEventWithoutHandler_ReturnsFailure()
    {
        ResultVoid result = await EventBus.PublishAsync(
            new FirstTestEvent("Test"),
            TestContext.Current.CancellationToken
        );

        Assert.True(result.IsFailure);
        Assert.Equal(result.ResultError, SystemErrors.NullValue);
    }

    [Fact]
    private async Task PublishAsync_PublishingNull_ReturnsFailure()
    {
        FirstTestEvent? firstTestEvent = null;

        ResultVoid result = await EventBus.PublishAsync(
            firstTestEvent,
            TestContext.Current.CancellationToken
        );

        Assert.True(result.IsFailure);
        Assert.Equal(result.ResultError, SystemErrors.NullArgument);
    }
}