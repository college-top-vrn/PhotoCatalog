using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects;

/// <summary>
///     Value Object для хранения габаритов изображения (ширина × высота в пикселях).
///     Гарантирует инварианты: ширина и высота строго положительные числа (> 0).
/// </summary>
/// <remarks>
///     Dimensions представляет собой неизменяемый объект без идентификатора.
///     Два объекта считаются равными, если имеют одинаковые Width и Height (структурное равенство).
///     <para>
///         <b>Инварианты:</b>
///     </para>
///     • Width > 0<br />
///     • Height > 0<br />
///     Создание возможно только через <see cref="Create(int, int)" /> с валидацией.
/// </remarks>
/// <example>
///     <code>
/// var dimensions = Dimensions.Create(1920, 1080);
/// if (dimensions.IsSuccess)
/// {
///     Console.WriteLine($"Размер: {dimensions.Value.Width}x{dimensions.Value.Height}");
/// }
/// </code>
/// </example>
public record Dimensions
{
    private const int MinValues = 1;

    /// <summary>
    ///     Приватный конструктор. Используйте <see cref="Create(int, int)" />.
    /// </summary>
    /// <param name="width">Ширина в пикселях</param>
    /// <param name="height">Высота в пикселях</param>
    private Dimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    ///     Ширина изображения в пикселях.
    /// </summary>
    /// <remarks>
    ///     Допустимые значения: больше <c>0</c>.
    /// </remarks>
    public int Width { get; init; }

    /// <summary>
    ///     Высота изображения в пикселях.
    /// </summary>
    /// <remarks>
    ///     Допустимые значения: больше <c>0</c>.
    /// </remarks>
    public int Height { get; init; }

    /// <summary>
    ///     Создает объект Dimensions с валидацией инвариантов.
    /// </summary>
    /// <param name="width">Ширина в пикселях (больше 0)</param>
    /// <param name="height">Высота в пикселях (больше 0)</param>
    /// <returns>
    ///     <see cref="Result{TSuccess}.Success" /> с валидными габаритами,
    ///     или <see cref="Result{TSuccess}.Failure" /> с <see cref="DomainErrors.Dimensions.Invalid" />.
    /// </returns>
    /// <remarks>
    ///     Оба измерения должны быть больше 0.
    ///     Если хотя бы одно измерение невалидно, возвращается ошибка.
    /// </remarks>
    public static Result<Dimensions> Create(int width, int height)
    {
        // Только проверка на положительные значения, без ограничений сверху
        if (width >= MinValues && height >= MinValues)
        {
            return Result<Dimensions>.Success(new Dimensions(width, height));
        }

        return Result<Dimensions>.Failure(DomainErrors.Dimensions.Invalid);
    }
}