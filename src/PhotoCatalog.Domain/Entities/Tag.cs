using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет программный тег фотографии.
/// </summary>
public sealed class Tag : Entity, IDeeplyCopyable<Tag>
{
    /// <summary>
    ///     Ммя тега.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     HEX-цвет тега.
    /// </summary>
    public string ColorHex { get; }

    private Tag(
        Guid id,
        Guid userId,
        string name,
        string colorHex
    ) : base(id, userId)
    {
        Name = name;
        ColorHex = colorHex;
    }

    /// <summary>
    ///     Создаёт новый тег.
    /// </summary>
    /// <param name="id">идентификатор тега.</param>
    /// <param name="userId">идентификатор владельца тега.</param>
    /// <param name="name">имя тега.</param>
    /// <param name="colorHex">HEX-цвет тега</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех с созданным тегом;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Tag.EmptyName"/>, если имя тега пустое;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Tag.TooLong"/>, если длина имени тега превышает 50 символов.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Tag> Create(
        Guid id,
        Guid userId,
        string name,
        string colorHex
    )
    {
        const int maxNameLength = 50;

        if (string.IsNullOrEmpty(name))
        {
            return Result.Failure<Tag>(DomainErrors.Tag.EmptyName);
        }

        string trimmedName = name.Trim();

        if (trimmedName.Length > maxNameLength)
        {
            return Result.Failure<Tag>(DomainErrors.Tag.TooLong);
        }

        string normalizedName = trimmedName.ToLowerInvariant();

        return Result.Success(new Tag(
            id,
            userId,
            normalizedName,
            colorHex
        ));
    }

    /// <inheritdoc />
    public Tag DeepCopy()
    {
        return new Tag(
            Id,
            UserId,
            Name,
            ColorHex
        );
    }
}