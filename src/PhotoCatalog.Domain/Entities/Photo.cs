using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет программную сущность физической фотографии, хранящейся в S3-хранилище.
/// </summary>
public sealed class Photo : Entity, IDeeplyCopyable<Photo>
{
    /// <summary>
    ///     Дата и время съёмки фотографии.
    /// </summary>
    public DateTime CapturedAt { get; }

    /// <summary>
    ///     Размер фотографии в битах.
    /// </summary>
    public Int64 PhotoSize { get; }

    /// <summary>
    ///     Формат фотографии.
    /// </summary>
    public string Mime { get; }

    /// <summary>
    ///     Ключ доступа к физической фотографии в S3-хранилище.
    /// </summary>
    public string StorageKey { get; }

    /// <summary>
    ///     Метаданные фотографии.
    /// </summary>
    public JsonDocument Metadata { get; }

    private readonly List<Guid> _tagIds;

    /// <summary>
    ///     Иммутабельный список идентификаторов тегов фотографии.
    /// </summary>
    public IImmutableList<Guid> TagIds => _tagIds.ToImmutableList();

    private Photo(
        Guid id,
        Guid userId,
        DateTime capturedAt,
        Int64 photoSize,
        string mime,
        string storageKey,
        JsonDocument metadata,
        List<Guid> tagIds) : base(id, userId)
    {
        CapturedAt = capturedAt;
        PhotoSize = photoSize;
        Mime = mime;
        StorageKey = storageKey;
        Metadata = metadata;
        _tagIds = tagIds;
    }

    /// <summary>
    ///     Создаёт новую фотографию.
    /// </summary>
    /// <param name="id">идентификатор фотографии.</param>
    /// <param name="userId">идентификатор владельца фотографии.</param>
    /// <param name="capturedAt">дата и время съёмки фотографии.</param>
    /// <param name="photoSize">размер фотографии в битах.</param>
    /// <param name="mime">формат фотографии.</param>
    /// <param name="storageKey">ключ доступа к физической фотографии в S3-хранилище.</param>
    /// <param name="metadata">метаданные фотографии.</param>
    /// <param name="tagIds">список идентификаторов тегов фотографии.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех с созданной фотографией;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Photo> Create(
        Guid id,
        Guid userId,
        DateTime capturedAt,
        Int64 photoSize,
        string mime,
        string storageKey,
        JsonDocument metadata,
        List<Guid> tagIds)
    {
        // TODO: реализовать валидатор MIME

        var photo = new Photo(
            id,
            userId,
            capturedAt,
            photoSize,
            mime,
            storageKey,
            metadata,
            tagIds
        );

        return Result.Success(photo);
    }

    /// <inheritdoc />
    public Photo DeepCopy()
    {
        List<Guid> tagIdsCopy = _tagIds.ConvertAll(tagId => Guid.Parse(tagId.ToString()));

        Photo clone = new(
            Id,
            UserId,
            CapturedAt,
            PhotoSize,
            Mime,
            StorageKey,
            Metadata,
            tagIdsCopy
        );

        return clone;
    }

    /// <summary>
    ///     Добавляет тег к фотографии.
    /// </summary>
    /// <param name="tagId">идентификатор тега.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Photo.DuplicateTag"/>, если данный тег уже привязан.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid AddTag(Guid tagId)
    {
        if (_tagIds.Contains(tagId))
        {
            return ResultVoid.Failure(DomainErrors.Photo.DuplicateTag);
        }

        _tagIds.Add(tagId);
        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаляет тег фотографии.
    /// </summary>
    /// <param name="tagId">идентификатор тега.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Photo.TagNotExists"/>, если тег не найден.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid RemoveTag(Guid tagId)
    {
        return _tagIds.Remove(tagId)
            ? ResultVoid.Success()
            : ResultVoid.Failure(DomainErrors.Photo.TagNotExists);
    }
}