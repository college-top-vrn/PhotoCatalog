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
    public void ToResultNullable_AllBranches_ShouldCoverNotNullAndNull()
    {
        string? validString = "data";
        string? nullString = null;

        var successResult = validString.ToResult(NullConditionError);
        var failureResult = nullString.ToResult(NullConditionError);

        Assert.True(successResult.IsSuccess);
        Assert.Equal(NullConditionError, failureResult.Error);
    }

    /// <summary>
    ///     Проверяет ветви метода Then (переход от Result{T} к Result{TNext}).
    /// </summary>
    [Fact]
    public void ThenGeneric_AllBranches_ShouldCoverNullFailureAndSuccess()
    {
        Result<int>? nullResult = null;
        var failedResult = Result<int>.Failure(DomainError);
        var successResult = Result<int>.Success(10);

        var nullOutcome = nullResult.Then(x => Result<string>.Success(x.ToString()));
        var failedOutcome = failedResult.Then(x => Result<string>.Success(x.ToString()));
        var successOutcome = successResult.Then(x => Result<string>.Success(x.ToString()));

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal(DomainError, failedOutcome.Error);
        Assert.Equal("10", successOutcome.Value);
    }

    /// <summary>
    ///     Проверяет ветви метода ThenTry (с трансформацией данных).
    /// </summary>
    [Fact]
    public void ThenTry_AllBranches_ShouldCoverNullFailureTryAndCatch()
    {
        Result<int>? nullResult = null;
        var failedResult = Result<int>.Failure(DomainError);
        var successResult = Result<int>.Success(10);

        var nullOutcome = nullResult.ThenTry(x => x * 2, _ => ExceptionError);
        var failedOutcome = failedResult.ThenTry(x => x * 2, _ => ExceptionError);
        var trySuccessOutcome = successResult.ThenTry(x => x * 2, _ => ExceptionError);
        var catchOutcome = successResult.ThenTry<int, int>(x => throw new Exception(), _ => ExceptionError);

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal(DomainError, failedOutcome.Error);
        Assert.Equal(20, trySuccessOutcome.Value);
        Assert.Equal(ExceptionError, catchOutcome.Error);
    }

    /// <summary>
    ///     Проверяет все логические пути метода Ensure.
    /// </summary>
    [Fact]
    public void Ensure_AllBranches_ShouldCoverAllLogicalPaths()
    {
        Result<int>? nullResult = null;
        var failedResult = Result<int>.Failure(DomainError);
        var successResult = Result<int>.Success(10);

        var nullOutcome = nullResult.Ensure(x => x > 5, EnsureError);
        var failedOutcome = failedResult.Ensure(x => x > 5, EnsureError);
        var trueOutcome = successResult.Ensure(x => x > 5, EnsureError);
        var falseOutcome = successResult.Ensure(x => x > 15, EnsureError);

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal(DomainError, failedOutcome.Error);
        Assert.Equal(10, trueOutcome.Value);
        Assert.Equal(EnsureError, falseOutcome.Error);
    }

    /// <summary>
    ///     Проверяет поведение методов Check с сохранением исходного значения в цепочке.
    /// </summary>
    [Fact]
    public void CheckGeneric_AllBranches_ShouldCoverAllLogicalPaths()
    {
        Result<int>? nullResult = null;
        var successResult = Result<int>.Success(10);

        var nullOutcome = nullResult.Check(x => Result<string>.Success("ok"));
        var checkSuccessOutcome = successResult.Check(x => Result<string>.Success("ok"));
        var checkFailureOutcome = successResult.Check(x => Result<string>.Failure(EnsureError));

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal(10, checkSuccessOutcome.Value);
        Assert.Equal(EnsureError, checkFailureOutcome.Error);
    }

    /// <summary>
    ///     Проверяет ветви метода Transform.
    /// </summary>
    [Fact]
    public void Transform_AllBranches_ShouldCoverNullFailureAndSuccess()
    {
        Result<int>? nullResult = null;
        var successResult = Result<int>.Success(10);

        var nullOutcome = nullResult.Transform(x => x.ToString());
        var successOutcome = successResult.Transform(x => x.ToString());

        Assert.Equal(SystemErrors.NullResult, nullOutcome.Error);
        Assert.Equal("10", successOutcome.Value);
    }

    /// <summary>
    ///     Проверяет ветви метода Finally.
    /// </summary>
    [Fact]
    public void Finally_AllBranches_ShouldMapBasedOnStateAndNull()
    {
        Result<int>? nullResult = null;
        var successResult = Result<int>.Success(10);
        var failureResult = Result<int>.Failure(DomainError);

        var nullMapped = nullResult.Finally(v => "Ok", e => e.Code);
        var successMapped = successResult.Finally(v => "Ok", e => e.Code);
        var failureMapped = failureResult.Finally(v => "Ok", e => e.Code);

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
    public void Chain_InterruptionInMiddle_ShouldShortCircuit()
    {
        var transformCalled = false;

        var finalValue = 5.ToResult()
            .Ensure(v => v > 10, StepError)
            .Transform(v =>
            {
                transformCalled = true;
                return v.ToString();
            })
            .Finally(v => "Success", e => e.Code);

        Assert.Equal(StepError.Code, finalValue);
        Assert.False(transformCalled);
    }

    /// <summary>
    ///     Проверяет перехват исключения внутри цепочки и корректное прохождение через OnFailure.
    /// </summary>
    [Fact]
    public void Chain_ExceptionCaught_ShouldShortCircuitAndTriggerOnFailure()
    {
        Error? caughtError = null;

        var result = "Data".ToResult()
            .ThenTry<string, int>(
                _ => throw new FormatException(),
                _ => ExceptionError)
            .OnFailure(err => caughtError = err)
            .Then(v => ResultVoid.Success());

        Assert.True(result.IsFailure);
        Assert.Equal(ExceptionError, caughtError);
    }
}