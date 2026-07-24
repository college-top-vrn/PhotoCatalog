using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects.Photo;

/// <summary>
///     ValueObject, представляющий собой размер фотографии в битах.
/// </summary>
public record PhotoSize
{
    /// <summary>
    ///     Значение размера.
    /// </summary>
    public Int64 Value { get; }

    private PhotoSize(Int64 value)
    {
        Value = value;
    }

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
    public static Result<PhotoSize> Create(Int64 value)
    {
        var photoSize = new PhotoSize(value);

        return Result.Success(photoSize);
    }
}