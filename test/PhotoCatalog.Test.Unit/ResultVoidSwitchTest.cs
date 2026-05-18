using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для проверки выражений switch и паттерн-матчинга для ResultVoid.
/// </summary>
public class ResultVoidSwitchTests
{
    private static readonly Error SwitchError = new("Switch.Error", "Pattern failed");

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
        ResultVoid result = ResultVoid.Failure(SwitchError);

        string errorDescription = result switch
        {
            { IsSuccess: true } => "Success",
            { IsFailure: true, Error.Message: var desc } => desc,
            _ => "Unknown"
        };

        Assert.Equal(SwitchError.Message, errorDescription);
    }
}