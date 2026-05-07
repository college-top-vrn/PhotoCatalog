namespace PhotoCatalog.Application.DTOs;

/// <summary>
///     DTO record для ответа с папкой.
/// </summary>
/// <param name="Id">Идентификатор папки.</param>
/// <param name="Name">Название папки.</param>
/// <param name="ParentFolderId">Идентификатор родительской папки.</param>
public record FolderResponse(
    int Id,
    string Name,
    int? ParentFolderId);