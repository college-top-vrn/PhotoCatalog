using System;

using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для покрытия ветвей методов расширения ResultExtensions с учетом проверок на null.
/// </summary>
public class ResultGenericExtensionsCoverageTests
{
    private static readonly Error DomainError = new("Domain.Error", "Error description");
    private static readonly Error ExceptionError = new("System.Exception", "Exception caught");
    private static readonly Error NullConditionError = new("Value.Null", "Value cannot be null");
    private static readonly Error EnsureError = new("Ensure.Failed", "Condition not met");

    /// <summary>
    ///     Проверяет ветви метода ToResult для nullable-типов.
    /// </summary>
    [Fact]
    public void ToResultNullableAllBranchesShouldCoverNotNullAndNull()
    {
        const string? validString = "data";
        string? nullString = null;

        Result<string> successResult = validString.ToResult(NullConditionError);
        Result<string> failureResult = nullString.ToResult(NullConditionError);

        Assert.True(successResult.IsSuccess);
        Assert.Equal(NullConditionError, failureResult.Error);
    }

    /// <summary>
    ///     Проверяет ветви метода Then (переход от Result{T} к Result{TNext}).
    /// </summary>
    [Fact]
    public void ThenGenericAllBranchesShouldCoverNullFailureAndSuccess()
    {
        Result<int>? nullResult = null;
        Result<int> failedResult = Result.Failure<int>(DomainError);
        // TODO: Исправить магические числа
        Result<int> successResult = Result.Success(10);

        ResultVoid nullOutcome = nullResult.Then(x => Result.Success(x.ToString()));
        Result<string> failedOutcome = failedResult.Then(x => Result.Success(x.ToString()));
        Result<string> successOutcome = successResult.Then(x => Result.Success(x.ToString()));

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal(DomainError, failedOutcome.Error);
        Assert.Equal("10", successOutcome.Value);
    }

    /// <summary>
    ///     Проверяет ветви метода ThenTry (с трансформацией данных).
    /// </summary>
    [Fact]
    public void ThenTryAllBranchesShouldCoverNullFailureTryAndCatch()
    {
        Result<int>? nullResult = null;
        Result<int> failedResult = Result.Failure<int>(DomainError);
        Result<int> successResult = Result.Success(10);

        Result<int> nullOutcome = nullResult.ThenTry(x => x * 2, _ => ExceptionError);
        Result<int> failedOutcome = failedResult.ThenTry(x => x * 2, _ => ExceptionError);
        Result<int> trySuccessOutcome = successResult.ThenTry(x => x * 2, _ => ExceptionError);
        //TODO: Выбросить более определенный Exception
        Result<int> catchOutcome = successResult.ThenTry<int, int>(_ => throw new Exception(), _ => ExceptionError);

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal(DomainError, failedOutcome.Error);
        Assert.Equal(20, trySuccessOutcome.Value);
        Assert.Equal(ExceptionError, catchOutcome.Error);
    }

    /// <summary>
    ///     Проверяет все логические пути метода Ensure.
    /// </summary>
    [Fact]
    public void EnsureAllBranchesShouldCoverAllLogicalPaths()
    {
        Result<int>? nullResult = null;
        Result<int> failedResult = Result.Failure<int>(DomainError);
        Result<int> successResult = Result.Success(10);

        Result<int> nullOutcome = nullResult.Ensure(x => x > 5, EnsureError);
        Result<int> failedOutcome = failedResult.Ensure(x => x > 5, EnsureError);
        Result<int> trueOutcome = successResult.Ensure(x => x > 5, EnsureError);
        Result<int> falseOutcome = successResult.Ensure(x => x > 15, EnsureError);

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal(DomainError, failedOutcome.Error);
        Assert.Equal(10, trueOutcome.Value);
        Assert.Equal(EnsureError, falseOutcome.Error);
    }

    /// <summary>
    ///     Проверяет поведение методов Check с сохранением исходного значения в цепочке.
    /// </summary>
    [Fact]
    public void CheckGenericAllBranchesShouldCoverAllLogicalPaths()
    {
        Result<int>? nullResult = null;
        Result<int> successResult = Result.Success(10);

        Result<int> nullOutcome = nullResult.Check(_ => Result.Success("ok"));
        Result<int> checkSuccessOutcome = successResult.Check(_ => Result.Success("ok"));
        Result<int> checkFailureOutcome = successResult.Check(_ => Result.Failure<string>(EnsureError));

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal(10, checkSuccessOutcome.Value);
        Assert.Equal(EnsureError, checkFailureOutcome.Error);
    }

    /// <summary>
    ///     Проверяет ветви метода Transform.
    /// </summary>
    [Fact]
    public void TransformAllBranchesShouldCoverNullFailureAndSuccess()
    {
        Result<int>? nullResult = null;
        Result<int> successResult = Result.Success(10);

        Result<string> nullOutcome = nullResult.Transform(x => x.ToString());
        Result<string> successOutcome = successResult.Transform(x => x.ToString());

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal("10", successOutcome.Value);
    }

    /// <summary>
    ///     Проверяет ветви метода Finally.
    /// </summary>
    [Fact]
    public void FinallyAllBranchesShouldMapBasedOnStateAndNull()
    {
        Result<int>? nullResult = null;
        Result<int> successResult = Result.Success(10);
        Result<int> failureResult = Result.Failure<int>(DomainError);

        string nullMapped = nullResult.Finally(_ => "Ok", e => e.Code);
        string successMapped = successResult.Finally(_ => "Ok", e => e.Code);
        string failureMapped = failureResult.Finally(_ => "Ok", e => e.Code);

        Assert.Equal(SystemErrors.NullResult.Code, nullMapped);
        Assert.Equal("Ok", successMapped);
        Assert.Equal(DomainError.Code, failureMapped);
    }
}

/// <summary>
///     Интеграционные тесты для комбинаций методов обобщенного типа в единой цепочке вызовов.
/// </summary>
public class ResultGenericChainsTests
{
    private static readonly Error StepError = new("Chain.StepError", "Failed at step");
    private static readonly Error ExceptionError = new("Chain.Exception", "Exception in chain");

    /// <summary>
    ///     Проверяет прерывание цепочки (Short-circuiting) на моменте проверки Ensure.
    /// </summary>
    [Fact]
    public void ChainInterruptionInMiddleShouldShortCircuit()
    {
        bool transformCalled = false;

        string finalValue = 5.ToResult()
            .Ensure(v => v > 10, StepError)
            .Transform(v =>
            {
                transformCalled = true;
                return v.ToString();
            })
            .Finally(_ => "Success", e => e.Code);

        Assert.Equal(StepError.Code, finalValue);
        Assert.False(transformCalled);
    }

    /// <summary>
    ///     Проверяет перехват исключения внутри цепочки и корректное прохождение через OnFailure.
    /// </summary>
    [Fact]
    public void ChainExceptionCaughtShouldShortCircuitAndTriggerOnFailure()
    {
        Error? caughtError = null;

        ResultVoid result = "Data".ToResult()
            .ThenTry<string, int>(
                _ => throw new FormatException(),
                _ => ExceptionError)
            .OnFailure(err => caughtError = err)
            .Then(_ => ResultVoid.Success());

        Assert.True(result.IsFailure);
        Assert.Equal(ExceptionError, caughtError);
    }
}