namespace PhotoCatalog.Application.DTOs;

/// <summary>
///     DTO record для запроса создания папки.
/// </summary>
/// <param name="Name">Название папки.</param>
/// <param name="ParentFolderId">Идентификатор родительской папки.</param>
public record CreateFolderRequest(
    string Name,
    int? ParentFolderId);