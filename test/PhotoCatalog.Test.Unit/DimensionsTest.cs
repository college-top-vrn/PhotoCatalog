using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;
using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Unit-тесты для DimensionsTest
/// </summary>
public class DimensionsTest
{
    /// <summary>
    ///     Проверяет, что валидные размеры (положительные числа) создаются успешно.
    /// </summary>
    [Fact]
    public void Create_ValidWidthAndHeight_ReturnsSuccess()
    {
        var result = Dimensions.Create(100, 100);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.Width);
        Assert.Equal(100, result.Value.Height);
    }

    /// <summary>
    ///     Проверяет минимально допустимые размеры (1×1 пикселей).
    /// </summary>
    [Fact]
    public void Create_MinimumValidSizeOne_ReturnsSuccess()
    {
        Result<Dimensions> result = Dimensions.Create(1, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Width);
        Assert.Equal(1, result.Value.Height);
    }

    /// <summary>
    ///     Проверяет, что нулевые значения (0×0) отклоняются с ошибкой Invalid.
    /// </summary>
    [Fact]
    public void Create_ZeroValues_ReturnsFailure()
    {
        Result<Dimensions> actual = Dimensions.Create(0, 0);

        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
        Assert.Equal(DomainErrors.Dimensions.Invalid, actual.Error);
    }

    /// <summary>
    ///     Проверяет, что отрицательные значения отклоняются с ошибкой Invalid.
    /// </summary>
    [Fact]
    public void Create_NegativeValues_ReturnsFailure()
    {
        Result<Dimensions> actual = Dimensions.Create(-1, -1);

        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
        Assert.Equal(DomainErrors.Dimensions.Invalid, actual.Error);
    }

    /// <summary>
    ///     Проверяет, что смешанные невалидные значения (отрицательная ширина, положительная высота) отклоняются.
    /// </summary>
    [Fact]
    public void Create_MixedInvalidValues_ReturnsFailure()
    {
        Result<Dimensions> actualNegativeWidth = Dimensions.Create(-100, 500);
        Assert.False(actualNegativeWidth.IsSuccess);
        Assert.Equal(DomainErrors.Dimensions.Invalid, actualNegativeWidth.Error);

        Result<Dimensions> actualNegativeHeight = Dimensions.Create(500, -100);
        Assert.False(actualNegativeHeight.IsSuccess);
        Assert.Equal(DomainErrors.Dimensions.Invalid, actualNegativeHeight.Error);
    }
}