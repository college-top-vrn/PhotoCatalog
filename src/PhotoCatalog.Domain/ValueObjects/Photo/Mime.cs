using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects.Photo;

/// <summary>
///     ValueObject, представляющий собой MIME файла
/// </summary>
public sealed record Mime
{
    /// <summary>
    ///     Значение MIME.
    /// </summary>
    public string Value { get; }

    private Mime(string value) => Value = value;

    /// <summary>
    ///     Создаёт новый MIME.
    /// </summary>
    /// <param name="value">значение MIME.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с новым экземпляром MIME.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Mime.IsEmpty"/>,
    ///                 если переданный MIME пустой.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Mime> Create(string value)
    {
        if (string.IsNullOrEmpty(value)) return Result.Failure<Mime>(DomainErrors.Mime.IsEmpty);

        Mime mime = new(value);

        return Result.Success(mime);
    }
}