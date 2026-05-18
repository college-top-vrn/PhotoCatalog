namespace PhotoCatalog.Application.DTOs.Folders;

/// <summary>
///     Ответ сервера при запросе информации о папке.
///     Содержит все необходимые данные для отображения папки на клиенте.
/// </summary>
/// <param name="Id">
///     Уникальный идентификатор папки в базе данных.
///     Соответствует <see cref="Domain.Entities.Folder.Id" />.
/// </param>
/// <param name="Name">
///     Название папки, отображаемое пользователю.
///     Соответствует <see cref="Domain.Entities.Folder.Name" />.
/// </param>
/// <param name="ParentFolderId">
///     Идентификатор родительской папки.
///     Если значение равно null, папка находится в корневом каталоге.
///     Соответствует <see cref="Domain.Entities.Folder.ParentFolderId" />.
/// </param>
public record FolderResponse(
    int Id,
    string Name,
    int? ParentFolderId);