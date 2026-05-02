using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects;

/// <summary>
/// Value Object для хранения габаритов изображения (ширина × высота в пикселях).
/// Гарантирует инварианты: ширина и высота строго от 1 до максимально допустимого размера.
/// </summary>
/// <remarks>
/// Dimensions представляет собой неизменяемый объект без идентификатора.
/// Два объекта считаются равными, если имеют одинаковые Width и Height (структурное равенство).
/// 
/// <para><b>Инварианты:</b></para>
/// • Width ∈ [1, 3840]<br/>
/// • Height ∈ [1, 2160]<br/>
/// 
/// Создание возможно только через <see cref="Create(int, int)"/> с валидацией.
/// </remarks>
/// <example>
/// <code>
/// var dimensions = Dimensions.Create(1920, 1080);
/// if (dimensions.IsSuccess)
/// {
///     Console.WriteLine($"Размер: {dimensions.Value.Width}x{dimensions.Value.Height}");
/// }
/// </code>
/// </example>
public record Dimensions
{
    private const int MaxWidth = 3840;
    private const int MaxHeight = 2160;
    private const int MinValues = 1;

    /// <summary>
    /// Ширина изображения в пикселях.
    /// </summary>
    /// <remarks>
    /// Допустимые значения: от <c>1</c> до <see cref="MaxWidth"/> пикселей.
    /// </remarks>
    public int Width { get; init; }

    /// <summary>
    /// Высота изображения в пикселях.
    /// </summary>
    /// <remarks>
    /// Допустимые значения: от <c>1</c> до <see cref="MaxHeight"/> пикселей.
    /// </remarks>
    public int Height { get; init; }

    /// <summary>
    /// Приватный конструктор. Используйте <see cref="Create(int, int)"/>.
    /// </summary>
    /// <param name="width">Ширина в пикселях</param>
    /// <param name="height">Высота в пикселях</param>
    /// <exception cref="InvalidOperationException"/>
    private Dimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Создает объект Dimensions с валидацией инвариантов.
    /// </summary>
    /// <param name="width">Ширина в пикселях (1-3840)</param>
    /// <param name="height">Высота в пикселях (1-2160)</param>
    /// <returns>
    /// <see cref="Result{TSuccess}.Success"/> с валидными габаритами,
    /// или <see cref="Result{TSuccess}.Failure"/> с <see cref="DomainErrors.Dimensions.Invalid"/>.
    /// </returns>
    /// <remarks>
    /// Оба измерения должны быть в допустимых пределах.
    /// Если хотя бы одно измерение невалидно, возвращается ошибка.
    /// </remarks>
    public static Result<Dimensions> Create(int width, int height)
    {
        if ((width >= MinValues && width <= MaxWidth) || (height >= MinValues && height <= MaxHeight))
        {
            return Result<Dimensions>.Success(new Dimensions(width, height));
        }

        return Result<Dimensions>.Failure(DomainErrors.Dimensions.Invalid);
    }
}