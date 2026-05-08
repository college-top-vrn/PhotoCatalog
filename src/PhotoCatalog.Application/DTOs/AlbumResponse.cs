using System.Collections.Generic;

namespace PhotoCatalog.Application.DTOs;

/// <summary>
///     DTO record для ответа с альбомом.
/// </summary>
/// <param name="Id">Идентификатор альбома.</param>
/// <param name="Name">Название альбома.</param>
/// <param name="FolderId">
///     Идентификатор папки,
///     в которой он находится.
/// </param>
/// <param name="PhotoIds">
///     Список из идентификаторов фото,
///     доступный только для чтения.
/// </param>
public record AlbumResponse(
    int Id,
    string Name,
    int? FolderId,
    IReadOnlyCollection<int> PhotoIds);