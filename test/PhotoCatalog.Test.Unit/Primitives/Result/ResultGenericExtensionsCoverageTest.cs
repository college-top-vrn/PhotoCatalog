using System;

using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit.Primitives.Result;

/// <summary>
///     Тесты для покрытия ветвей методов расширения ResultExtensions с учетом проверок на null.
/// </summary>
public class ResultGenericExtensionsCoverageTests
{
    private static readonly ResultError DomainResultError = new("Domain.Error", "Error description");
    private static readonly ResultError ExceptionResultError = new("System.Exception", "Exception caught");
    private static readonly ResultError NullConditionResultError = new("Value.Null", "Value cannot be null");
    private static readonly ResultError EnsureResultError = new("Ensure.Failed", "Condition not met");

    /// <summary>
    ///     Проверяет ветви метода ToResult для nullable-типов.
    /// </summary>
    [Fact]
    public void ToResultNullableAllBranchesShouldCoverNotNullAndNull()
    {
        const string? validString = "data";
        string? nullString = null;

        Result<string> successResult = validString.ToResult(NullConditionResultError);
        Result<string> failureResult = nullString.ToResult(NullConditionResultError);

        Assert.True(successResult.IsSuccess);
        Assert.Equal(NullConditionResultError, failureResult.ResultError);
    }

    /// <summary>
    ///     Проверяет ветви метода Then (переход от Result{T} к Result{TNext}).
    /// </summary>
    [Fact]
    public void ThenGenericAllBranchesShouldCoverNullFailureAndSuccess()
    {
        Result<int>? nullResult = null;
        Result<int> failedResult = PhotoCatalog.Domain.Primitives.Result.Failure<int>(DomainResultError);
        // TODO: Исправить магические числа
        Result<int> successResult = PhotoCatalog.Domain.Primitives.Result.Success(10);

        ResultVoid nullOutcome = nullResult.Then(x => PhotoCatalog.Domain.Primitives.Result.Success(x.ToString()));
        Result<string> failedOutcome =
            failedResult.Then(x => PhotoCatalog.Domain.Primitives.Result.Success(x.ToString()));
        Result<string> successOutcome =
            successResult.Then(x => PhotoCatalog.Domain.Primitives.Result.Success(x.ToString()));

        Assert.Equal(SystemErrors.NullResult, nullOutcome.ResultError);
        Assert.Equal(DomainResultError, failedOutcome.ResultError);
        Assert.Equal("10", successOutcome.Value);
    }

    /// <summary>
    ///     Проверяет ветви метода ThenTry (с трансформацией данных).
    /// </summary>
    [Fact]
    public void ThenTryAllBranchesShouldCoverNullFailureTryAndCatch()
    {
        Result<int>? nullResult = null;
        Result<int> failedResult = PhotoCatalog.Domain.Primitives.Result.Failure<int>(DomainResultError);
        Result<int> successResult = PhotoCatalog.Domain.Primitives.Result.Success(10);

        Result<int> nullOutcome = nullResult.ThenTry(x => x * 2, _ => ExceptionResultError);
        Result<int> failedOutcome = failedResult.ThenTry(x => x * 2, _ => ExceptionResultError);
        Result<int> trySuccessOutcome = successResult.ThenTry(x => x * 2, _ => ExceptionResultError);
        //TODO: Выбросить более определенный Exception
        Result<int> catchOutcome =
            successResult.ThenTry<int, int>(_ => throw new Exception(), _ => ExceptionResultError);

        Assert.Equal(SystemErrors.NullResult, nullOutcome.ResultError);
        Assert.Equal(DomainResultError, failedOutcome.ResultError);
        Assert.Equal(20, trySuccessOutcome.Value);
        Assert.Equal(ExceptionResultError, catchOutcome.ResultError);
    }

    /// <summary>
    ///     Проверяет все логические пути метода Ensure.
    /// </summary>
    [Fact]
    public void EnsureAllBranchesShouldCoverAllLogicalPaths()
    {
        Result<int>? nullResult = null;
        Result<int> failedResult = PhotoCatalog.Domain.Primitives.Result.Failure<int>(DomainResultError);
        Result<int> successResult = PhotoCatalog.Domain.Primitives.Result.Success(10);

        Result<int> nullOutcome = nullResult.Ensure(x => x > 5, EnsureResultError);
        Result<int> failedOutcome = failedResult.Ensure(x => x > 5, EnsureResultError);
        Result<int> trueOutcome = successResult.Ensure(x => x > 5, EnsureResultError);
        Result<int> falseOutcome = successResult.Ensure(x => x > 15, EnsureResultError);

        Assert.Equal(SystemErrors.NullResult, nullOutcome.ResultError);
        Assert.Equal(DomainResultError, failedOutcome.ResultError);
        Assert.Equal(10, trueOutcome.Value);
        Assert.Equal(EnsureResultError, falseOutcome.ResultError);
    }

    /// <summary>
    ///     Проверяет поведение методов Check с сохранением исходного значения в цепочке.
    /// </summary>
    [Fact]
    public void CheckGenericAllBranchesShouldCoverAllLogicalPaths()
    {
        Result<int>? nullResult = null;
        Result<int> successResult = PhotoCatalog.Domain.Primitives.Result.Success(10);

        Result<int> nullOutcome = nullResult.Check(_ => PhotoCatalog.Domain.Primitives.Result.Success("ok"));
        Result<int> checkSuccessOutcome = successResult.Check(_ => PhotoCatalog.Domain.Primitives.Result.Success("ok"));
        Result<int> checkFailureOutcome =
            successResult.Check(_ => PhotoCatalog.Domain.Primitives.Result.Failure<string>(EnsureResultError));

        Assert.Equal(SystemErrors.NullResult, nullOutcome.ResultError);
        Assert.Equal(10, checkSuccessOutcome.Value);
        Assert.Equal(EnsureResultError, checkFailureOutcome.ResultError);
    }

    /// <summary>
    ///     Проверяет ветви метода Transform.
    /// </summary>
    [Fact]
    public void TransformAllBranchesShouldCoverNullFailureAndSuccess()
    {
        Result<int>? nullResult = null;
        Result<int> successResult = PhotoCatalog.Domain.Primitives.Result.Success(10);

        Result<string> nullOutcome = nullResult.Transform(x => x.ToString());
        Result<string> successOutcome = successResult.Transform(x => x.ToString());

        Assert.Equal(SystemErrors.NullResult, nullOutcome.ResultError);
        Assert.Equal("10", successOutcome.Value);
    }

    /// <summary>
    ///     Проверяет ветви метода Finally.
    /// </summary>
    [Fact]
    public void FinallyAllBranchesShouldMapBasedOnStateAndNull()
    {
        Result<int>? nullResult = null;
        Result<int> successResult = PhotoCatalog.Domain.Primitives.Result.Success(10);
        Result<int> failureResult = PhotoCatalog.Domain.Primitives.Result.Failure<int>(DomainResultError);

        string nullMapped = nullResult.Finally(_ => "Ok", e => e.Code);
        string successMapped = successResult.Finally(_ => "Ok", e => e.Code);
        string failureMapped = failureResult.Finally(_ => "Ok", e => e.Code);

        Assert.Equal(SystemErrors.NullResult.Code, nullMapped);
        Assert.Equal("Ok", successMapped);
        Assert.Equal(DomainResultError.Code, failureMapped);
    }
}

/// <summary>
///     Интеграционные тесты для комбинаций методов обобщенного типа в единой цепочке вызовов.
/// </summary>
public class ResultGenericChainsTests
{
    private static readonly ResultError StepResultError = new("Chain.StepError", "Failed at step");
    private static readonly ResultError ExceptionResultError = new("Chain.Exception", "Exception in chain");

    /// <summary>
    ///     Проверяет прерывание цепочки (Short-circuiting) на моменте проверки Ensure.
    /// </summary>
    [Fact]
    public void ChainInterruptionInMiddleShouldShortCircuit()
    {
        bool transformCalled = false;

        string finalValue = 5.ToResult()
            .Ensure(v => v > 10, StepResultError)
            .Transform(v =>
            {
                transformCalled = true;
                return v.ToString();
            })
            .Finally(_ => "Success", e => e.Code);

        Assert.Equal(StepResultError.Code, finalValue);
        Assert.False(transformCalled);
    }

    /// <summary>
    ///     Проверяет перехват исключения внутри цепочки и корректное прохождение через OnFailure.
    /// </summary>
    [Fact]
    public void ChainExceptionCaughtShouldShortCircuitAndTriggerOnFailure()
    {
        ResultError? caughtError = null;

        ResultVoid result = "Data".ToResult()
            .ThenTry<string, int>(
                _ => throw new FormatException(),
                _ => ExceptionResultError)
            .OnFailure(err => caughtError = err)
            .Then(_ => ResultVoid.Success());

        Assert.True(result.IsFailure);
        Assert.Equal(ExceptionResultError, caughtError);
    }
}