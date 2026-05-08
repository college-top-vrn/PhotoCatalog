using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Services;

/// <summary>
///     Интерфейс валидатора дерева,
///     который обеспечивает защиту от
///     циклических зависимостей графа.
/// </summary>
public interface IFolderHierarchyValidator
{
    /// <summary>
    ///     Метод проверки на циклические зависимости папки в дереве.
    /// </summary>
    /// <param name="sourceFolderId">Идентификатор перемещаемой папки.</param>
    /// <param name="targetFolderId">Идентификатор папки, в которую планируется перемещение.</param>
    /// <returns>
    ///     Успешный результат, если цикл не обнаружен;
    ///     результат с ошибкой <see cref="ApplicationErrors.Folders.CycleDetected" /> в противном случае.
    /// </returns>
    ResultVoid CheckForCycles(int sourceFolderId, int targetFolderId);
}