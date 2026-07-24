using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects;

/// <summary>
///     ValueObject, представляющий собой имя.
/// </summary>
public record Name
{
    /// <summary>
    ///     Значение имени.
    /// </summary>
    public string Value { get; }

    private Name(string value) => Value = value;

    /// <summary>
    ///     Создаёт новое имя
    /// </summary>
    /// <param name="value">значение имени.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с новым экземпляром именем;</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Name.IsEmpty"/>, если имя пустое или null;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Name.IsTooLong"/>, если длина имени больше 50;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Name> Create(string value)
    {
        const int maxNameLength = 50;

        var trimmedValue = value.Trim();

        if (string.IsNullOrEmpty(value)) return Result.Failure<Name>(DomainErrors.Name.IsEmpty);

        if (value.Length > maxNameLength) return Result.Failure<Name>(DomainErrors.Name.IsTooLong);

        var name = new Name(trimmedValue);

        return Result.Success(name);
    }
}