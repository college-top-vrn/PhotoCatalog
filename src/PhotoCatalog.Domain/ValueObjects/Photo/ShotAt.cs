using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects.Photo;

/// <summary>
///     ValueObject, представляющий собой дату и время съёмки фотографии.
/// </summary>
public sealed record ShotAt
{
    /// <summary>
    ///     Значение даты и времени съёмки фотографии.
    /// </summary>
    public DateTimeOffset Value { get; }

    private ShotAt(DateTime value) => Value = value;

    /// <summary>
    ///     Создаёт новую дату и время съёмки фотографии.
    /// </summary>
    /// <param name="value">значение даты и времени.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с новым экземпляром даты и времени съёмки.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.ShotAt.IsInvalid"/>,
    ///                 если формат даты и времени съёмки не подходит.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<ShotAt> Create(string value)
    {
        if (!DateTime.TryParse(value, out DateTime result))
        {
            return Result.Failure<ShotAt>(DomainErrors.ShotAt.IsInvalid);
        }

        ShotAt shotAt = new(result);

        return Result.Success(shotAt);
    }
}