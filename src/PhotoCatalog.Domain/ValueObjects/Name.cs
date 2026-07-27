using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects;

/// <summary>
///     ValueObject, представляющий собой имя.
/// </summary>
public sealed record Name
{
    const int MaxLength = 50;

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
    ///                 Ошибка <see cref="DomainErrors.Name.IsEmpty"/>, если имя тега пустое;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Name.IsTooLong"/>,
    ///                 если длина имени тега превышает 50 символов.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Name> Create(string value)
    {
        string trimmedValue = value.Trim();

        if (string.IsNullOrEmpty(value)) return Result.Failure<Name>(DomainErrors.Name.IsEmpty);

        if (value.Length > MaxLength) return Result.Failure<Name>(DomainErrors.Name.IsTooLong);

        Name name = new(trimmedValue);

        return Result.Success(name);
    }
}