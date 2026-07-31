using System;

using PhotoCatalog.Domain.Interfaces;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет программную доменную сущность тег.
/// </summary>
public sealed class Tag : Entity, IDeeplyCopyable<Tag>
{
    /// <summary>
    ///     Ммя тега.
    /// </summary>
    public Name Name { get; set; }

    /// <summary>
    ///     HEX-цвет тега.
    /// </summary>
    public ColorHex ColorHex { get; set; }

    private Tag(Guid id, Guid userId, Name name, ColorHex colorHex) : base(id, userId)
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
    /// <param name="colorHex">HEX-код цвета тега.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех с созданным тегом;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Name.IsEmpty"/>, если имя тега пустое;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Name.IsTooLong"/>, если длина имени тега превышает 50 символов.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Tag> Create(Guid id, Guid userId, Name name, ColorHex colorHex)
    {
        return Result.Success(new Tag(id, userId, name, colorHex));
    }

    /// <inheritdoc />
    public Tag DeepCopy()
    {
        return new Tag(Id, UserId, Name, ColorHex);
    }
}