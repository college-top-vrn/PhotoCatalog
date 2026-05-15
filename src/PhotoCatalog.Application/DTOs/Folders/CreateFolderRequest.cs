namespace PhotoCatalog.Application.DTOs.Folders;

/// <summary>
///     Запрос от клиента на создание новой папки.
///     Передается в формате JSON из React-фронтенда.
/// </summary>
/// <param name="Name">
///     Имя для новой папки, введенное пользователем.
///     Не может быть пустым или состоять из пробелов.
/// </param>
/// <param name="ParentFolderId">
///     Идентификатор папки, внутри которой создается новая папка.
///     Если значение равно null, папка создается в корневом каталоге.
/// </param>
public record CreateFolderRequest(
    string Name,
    int? ParentFolderId);