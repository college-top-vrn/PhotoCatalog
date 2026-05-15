using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;

namespace PhotoCatalog.Application.UseCases;

/// <summary>
///     Сценарий использования для перемещения папки.
/// </summary>
/// <param name="folderRepository">Репозиторий папок.</param>
/// <param name="folderHierarchyValidator">Валидатор иерархии папок.</param>
/// <param name="unitOfWork">Единица работы.</param>
/// <param name="logger">Логгер.</param>
public class MoveFolderUseCase(
    IFolderRepository folderRepository,
    IFolderHierarchyValidator folderHierarchyValidator,
    IUnitOfWork unitOfWork,
    ILogger logger)
{
    /// <summary>
    ///     Выполняет перемещение папки с идентификатором <paramref name="folderId" />
    ///     в родительскую папку с идентификатором <paramref name="newParentId" />.
    ///     Если <paramref name="newParentId" /> равен null, папка перемещается в корень.
    /// </summary>
    /// <param name="folderId">Идентификатор исходной папки.</param>
    /// <param name="newParentId">
    ///     Идентификатор целевой папки.
    ///     Если значение равно null, папка перемещается в корневой каталог.
    /// </param>
    /// <returns>Результат выполнения операции (успех или ошибка).</returns>
    public ResultVoid Execute(int folderId, int? newParentId)
    {
        // Если newParentId == null, перемещаем в корень
        if (newParentId == null)
        {
            return MoveToRoot(folderId);
        }

        return MoveToFolder(folderId, newParentId.Value);
    }

    /// <summary>
    ///     Перемещает папку в корень.
    /// </summary>
    private ResultVoid MoveToRoot(int folderId)
    {
        Folder? sourceFolder = null;
        
        return folderRepository.GetById(folderId)
            .OnSuccess(data =>
            {
                logger.Information("Исходная папка {FolderId} найдена.", folderId);
                sourceFolder = data;
            })
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Исходная папка {FolderId} не найдена.",
                    error.Code, folderId))
            .Ensure(_ => sourceFolder != null, ApplicationErrors.General.NotFound)
            .Then(_ => sourceFolder!.MoveToRoot())
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Папка {SourceId} перемещена в корень.", sourceFolder!.Id))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Не удалось переместить папку {SourceId} в корень.",
                    error.Code, sourceFolder!.Id))
            .Then(_ => unitOfWork.BeginTransaction())
            .ToResult()
            .OnSuccess(_ => logger.Information("Успешно начата транзакция."))
            .OnFailure(error => logger.Warning("Ошибка {ErrorCode}: Не удалось начать транзакцию.", error.Code))
            .Then(_ => folderRepository.Update(sourceFolder!))
            .ToResult()
            .OnSuccess(_ => logger.Information("Успешно обновлена папка."))
            .OnFailure(error => logger.Warning("Ошибка {ErrorCode}: Не удалось обновить папку.", error.Code))
            .Then(_ => unitOfWork.Commit())
            .ToResult()
            .OnSuccess(_ => logger.Information("Успешно выполнена транзакция."))
            .OnFailure(error => logger.Warning("Ошибка {ErrorCode}: Не удалось завершить транзакцию", error.Code))
            .Finally(_ => ResultVoid.Success(), ResultVoid.Failure);
    }

    /// <summary>
    ///     Перемещает папку в целевую папку.
    /// </summary>
    private ResultVoid MoveToFolder(int folderId, int targetFolderId)
    {
        Folder? sourceFolder = null;
        Folder? targetFolder = null;
        
        return folderRepository.GetById(folderId)
            .OnSuccess(data =>
            {
                logger.Information("Исходная папка {FolderId} найдена.", folderId);
                sourceFolder = data;
            })
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Исходная папка {FolderId} не найдена.",
                    error.Code, folderId))
            .Then(_ => folderRepository.GetById(targetFolderId))
            .OnSuccess(data =>
            {
                logger.Information("Целевая папка {TargetId} найдена.", targetFolderId);
                targetFolder = data;
            })
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Целевая папка {TargetId} не найдена.",
                    error.Code, targetFolderId))
            .Ensure(_ => sourceFolder != null, ApplicationErrors.General.NotFound)
            .Ensure(_ => targetFolder != null, ApplicationErrors.General.NotFound)
            .Then(_ => folderHierarchyValidator.CheckForCycles(folderId, targetFolderId))
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Циклические зависимости между папками {SourceId} и {TargetId} не найдены.",
                    folderId, targetFolderId))
            .OnFailure(error =>
                logger.Warning(
                    "Ошибка {ErrorCode}: Найдены циклические зависимости между папками {SourceId} и {TargetId}.",
                    error.Code, folderId, targetFolderId))
            .Then(_ => sourceFolder!.MoveTo(targetFolder!))
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Папка {SourceId} перемещена в {TargetId}.",
                    sourceFolder!.Id, targetFolder!.Id))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Не удалось переместить папку {SourceId} в {TargetId}.",
                    error.Code, sourceFolder!.Id, targetFolder!.Id))
            .Then(_ => unitOfWork.BeginTransaction())
            .ToResult()
            .OnSuccess(_ => logger.Information("Успешно начата транзакция."))
            .OnFailure(error => logger.Warning("Ошибка {ErrorCode}: Не удалось начать транзакцию.", error.Code))
            .Then(_ => folderRepository.Update(sourceFolder!))
            .ToResult()
            .OnSuccess(_ => logger.Information("Успешно обновлена папка."))
            .OnFailure(error => logger.Warning("Ошибка {ErrorCode}: Не удалось обновить папку.", error.Code))
            .Then(_ => unitOfWork.Commit())
            .ToResult()
            .OnSuccess(_ => logger.Information("Успешно выполнена транзакция."))
            .OnFailure(error => logger.Warning("Ошибка {ErrorCode}: Не удалось завершить транзакцию", error.Code))
            .Finally(_ => ResultVoid.Success(), ResultVoid.Failure);
    }
}