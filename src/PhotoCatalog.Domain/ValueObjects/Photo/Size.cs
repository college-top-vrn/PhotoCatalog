using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects.Photo;

/// <summary>
///     ValueObject, представляющий собой размер фотографии в битах.
/// </summary>
public sealed record Size
{
    /// <summary>
    ///     Значение размера.
    /// </summary>
    public Int64 Value { get; }

    private Size(Int64 value) => Value = value;

    /// <summary>
    ///     Создаёт новый размер.
    /// </summary>
    /// <param name="value">значение размера.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с новым экземпляром размера.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Size> Create(Int64 value)
    {
        Size size = new(value);

        return Result.Success(size);
    }
}