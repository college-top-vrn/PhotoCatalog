using System;

using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для полного покрытия ветвей методов расширения ResultVoidExtensions.
/// </summary>
public class ResultVoidExtensionsCoverageTest
{
    private static readonly Error DomainError = new("Domain.Error", "Error description");
    private static readonly Error ExceptionError = new("System.Exception", "Exception caught");

    /// <summary>
    ///     Проверяет обе ветви метода Then (переход от ResultVoid к ResultVoid).
    /// </summary>
    [Fact]
    public void ThenVoidAllBranchesShouldExecuteCorrectPath()
    {
        ResultVoid successResult = ResultVoid.Success().Then(ResultVoid.Success);
        ResultVoid failureResult = ResultVoid.Failure(DomainError).Then(ResultVoid.Success);

        Assert.True(successResult.IsSuccess);
        Assert.True(failureResult.IsFailure);
        Assert.Equal(DomainError, failureResult.Error);
    }

    /// <summary>
    ///     Проверяет ветви try и catch метода-фабрики TryCatch.
    /// </summary>
    [Fact]
    public void TryCatchAllBranchesShouldHandleSuccessAndException()
    {
        ResultVoid successResult = ResultVoidExtensions.TryCatch(
            () => { },
            _ => ExceptionError);

        ResultVoid exceptionResult = ResultVoidExtensions.TryCatch(
            () => throw new InvalidOperationException(),
            _ => ExceptionError);

        Assert.True(successResult.IsSuccess);
        Assert.True(exceptionResult.IsFailure);
        Assert.Equal(ExceptionError, exceptionResult.Error);
    }

    /// <summary>
    ///     Проверяет ветви метода ThenTry для операций без возвращаемого значения.
    /// </summary>
    [Fact]
    public void ThenTryVoidAllBranchesShouldCoverFailureTryAndCatch()
    {
        ResultVoid failedPrevious = ResultVoid.Failure(DomainError).ThenTry(() => { }, _ => ExceptionError);
        ResultVoid trySuccess = ResultVoid.Success().ThenTry(() => { }, _ => ExceptionError);
        //TODO: Выбросить более конкретный Exception
        ResultVoid catchTriggered = ResultVoid.Success().ThenTry(() => throw new Exception(), _ => ExceptionError);

        Assert.Equal(DomainError, failedPrevious.Error);
        Assert.True(trySuccess.IsSuccess);
        Assert.Equal(ExceptionError, catchTriggered.Error);
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

        ResultVoid.Failure(DomainError)
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
        int result2 = ResultVoid.Failure(DomainError).Finally(() => 1, _ => 0);

        Assert.Equal(1, result1);
        Assert.Equal(0, result2);
    }
}