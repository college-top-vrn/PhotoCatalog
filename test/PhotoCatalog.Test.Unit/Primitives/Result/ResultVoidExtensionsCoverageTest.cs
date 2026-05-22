using System;

using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit.Primitives.Result;

/// <summary>
///     Тесты для полного покрытия ветвей методов расширения ResultVoidExtensions.
/// </summary>
public class ResultVoidExtensionsCoverageTest
{
    private static readonly ResultError DomainResultError = new("Domain.Error", "Error description");
    private static readonly ResultError ExceptionResultError = new("System.Exception", "Exception caught");

    /// <summary>
    ///     Проверяет обе ветви метода Then (переход от ResultVoid к ResultVoid).
    /// </summary>
    [Fact]
    public void ThenVoidAllBranchesShouldExecuteCorrectPath()
    {
        ResultVoid successResult = ResultVoid.Success().Then(ResultVoid.Success);
        ResultVoid failureResult = ResultVoid.Failure(DomainResultError).Then(ResultVoid.Success);

        Assert.True(successResult.IsSuccess);
        Assert.True(failureResult.IsFailure);
        Assert.Equal(DomainResultError, failureResult.ResultError);
    }

    /// <summary>
    ///     Проверяет ветви try и catch метода-фабрики TryCatch.
    /// </summary>
    [Fact]
    public void TryCatchAllBranchesShouldHandleSuccessAndException()
    {
        ResultVoid successResult = ResultVoidExtensions.TryCatch(
            () => { },
            _ => ExceptionResultError);

        ResultVoid exceptionResult = ResultVoidExtensions.TryCatch(
            () => throw new InvalidOperationException(),
            _ => ExceptionResultError);

        Assert.True(successResult.IsSuccess);
        Assert.True(exceptionResult.IsFailure);
        Assert.Equal(ExceptionResultError, exceptionResult.ResultError);
    }

    /// <summary>
    ///     Проверяет ветви метода ThenTry для операций без возвращаемого значения.
    /// </summary>
    [Fact]
    public void ThenTryVoidAllBranchesShouldCoverFailureTryAndCatch()
    {
        ResultVoid failedPrevious = ResultVoid.Failure(DomainResultError).ThenTry(() => { }, _ => ExceptionResultError);
        ResultVoid trySuccess = ResultVoid.Success().ThenTry(() => { }, _ => ExceptionResultError);
        //TODO: Выбросить более конкретный Exception
        ResultVoid catchTriggered =
            ResultVoid.Success().ThenTry(() => throw new Exception(), _ => ExceptionResultError);

        Assert.Equal(DomainResultError, failedPrevious.ResultError);
        Assert.True(trySuccess.IsSuccess);
        Assert.Equal(ExceptionResultError, catchTriggered.ResultError);
    }

    /// <summary>
    ///     Проверяет ветви выполнения методов побочных эффектов OnSuccess и OnFailure.
    /// </summary>
    [Fact]
    public void SideEffectsAllBranchesShouldTriggerBasedOnStatus()
    {
        int successCounter = 0;
        int failureCounter = 0;

        ResultVoid.Success()
            .OnSuccess(() => successCounter++)
            .OnFailure(_ => failureCounter++);

        ResultVoid.Failure(DomainResultError)
            .OnSuccess(() => successCounter++)
            .OnFailure(_ => failureCounter++);

        Assert.Equal(1, successCounter);
        Assert.Equal(1, failureCounter);
    }

    /// <summary>
    ///     Проверяет ветви метода Finally.
    /// </summary>
    [Fact]
    public void FinallyAllBranchesShouldMapCorrectly()
    {
        int result1 = ResultVoid.Success().Finally(() => 1, _ => 0);
        int result2 = ResultVoid.Failure(DomainResultError).Finally(() => 1, _ => 0);

        Assert.Equal(1, result1);
        Assert.Equal(0, result2);
    }
}