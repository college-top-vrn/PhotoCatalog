using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects.Photo;

/// <summary>
///     ValueObject, представляющий собой ключ от физического файла из S3-хранилища.
/// </summary>
public sealed record StorageKey
{
    /// <summary>
    ///     Значение ключа.
    /// </summary>
    public string Value { get; }

    private StorageKey(string value) => Value = value;

    /// <summary>
    ///     Создаёт новый ключ.
    /// </summary>
    /// <param name="value">значение ключа.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с новым экземпляром ключа.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.StorageKey.IsEmpty"/>,
    ///                 если переданное значение пустое.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<StorageKey> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Result.Failure<StorageKey>(DomainErrors.StorageKey.IsEmpty);
        }

        StorageKey storageKey = new(value);

        return Result.Success(storageKey);
    }
}