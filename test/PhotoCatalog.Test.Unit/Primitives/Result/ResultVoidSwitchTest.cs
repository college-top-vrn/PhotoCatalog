using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit.Primitives.Result;

/// <summary>
///     Тесты для проверки выражений switch и паттерн-матчинга для ResultVoid.
/// </summary>
public class ResultVoidSwitchTests
{
    private static readonly ResultError SwitchResultError = new("Switch.Error", "Pattern failed");

    /// <summary>
    ///     Проверяет сопоставление успешного результата через паттерн свойств.
    /// </summary>
    [Fact]
    public void SwitchPropertyPatternOnSuccessShouldMatchCorrectBranch()
    {
        ResultVoid result = ResultVoid.Success();

        string status = result switch
        {
            { IsSuccess: true } => "Success",
            { IsFailure: true } => "Failure",
            _ => "Unknown"
        };

        Assert.Equal("Success", status);
    }

    /// <summary>
    ///     Проверяет глубокое сопоставление свойств ошибки внутри провального результата.
    /// </summary>
    [Fact]
    public void SwitchNestedPropertyPatternOnFailureShouldExtractNestedData()
    {
        ResultVoid result = ResultVoid.Failure(SwitchResultError);

        string errorDescription = result switch
        {
            { IsSuccess: true } => "Success",
            { IsFailure: true, ResultError.Message: var desc } => desc,
            _ => "Unknown"
        };

        Assert.Equal(SwitchResultError.Message, errorDescription);
    }
}