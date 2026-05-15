namespace PhotoCatalog.Application.DTOs.Folders;

/// <summary>
///     Запрос от клиента на перемещение папки в другую директорию.
///     Используется при Drag-and-Drop операции на клиенте.
///     Идентификатор перемещаемой папки передается через URL эндпоинта.
/// </summary>
/// <param name="NewParentId">
///     Идентификатор целевой папки, куда перемещается текущая папка.
///     Если значение равно null, папка перемещается в корневой каталог.
///     Валидация значения выполняется в UseCase слое.
/// </param>
public record MoveFolderRequest(
    int? NewParentId);