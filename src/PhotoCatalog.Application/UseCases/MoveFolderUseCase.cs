using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;

namespace PhotoCatalog.Application.UseCases;

/// <summary>
/// Сценарий использования для перемещения папки.
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
    /// Выполняет перемещение папки с идентификатором <paramref name="folderId"/>
    /// в родительскую папку с идентификатором <paramref name="newParentId"/>.
    /// </summary>
    /// <param name="folderId">Идентификатор исходной папки.</param>
    /// <param name="newParentId">Идентификатор целевой папки.</param>
    /// <returns>Результат выполнения операции (успех или ошибка).</returns>
    public ResultVoid Execute(int folderId, int newParentId)
    {
        Folder? sourceFolder = null;
        Folder? targetFolder = null;
        return folderRepository.GetById(folderId)
            .OnSuccess(data =>
            {
                logger.Information("Исходная папка {FolderId} найдена.",
                    folderId);
                sourceFolder = data;
            })
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Исходная папка {FolderId} не найдена.",
                    error.Code,
                    folderId))
            .Then(_ =>
                folderRepository.GetById(newParentId))
            .OnSuccess(data =>
            {
                logger.Information("Целевая папка {FolderId} найдена.", folderId);
                targetFolder = data;
            })
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Целевая папка {FolderId} не найдена.",
                    error.Code,
                    folderId))
            .Then(_ =>
                folderHierarchyValidator.CheckForCycles(folderId, newParentId))
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Циклические зависимости между папками {SourceId} и {TargetId} не найдены.",
                    folderId,
                    newParentId))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Найдены циклические зависимостим между папками {SourceId} и {TargetId}.",
                    error.Code,
                    folderId,
                    newParentId))
            .Ensure(_ =>
                    sourceFolder != null,
                ApplicationErrors.General.NotFound)
            .Ensure(_ =>
                    targetFolder != null,
                ApplicationErrors.General.NotFound)
            .Then(_ =>
                sourceFolder!.MoveTo(targetFolder!))
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Папка {SourceId} перемещена в {TargetId}.",
                    sourceFolder!.Id,
                    targetFolder!.Id))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Не удалось переместить папку {SourceId} в {TargetId}.",
                    error.Code,
                    sourceFolder!.Id,
                    targetFolder!.Id))
            .Then(_ =>
                unitOfWork.BeginTransaction())
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Успешно начата транзакция."))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Не удалось начать транзакцию.",
                    error.Code))
            .Then(_ =>
                folderRepository.Update(sourceFolder!))
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Успешно обновлена папка."))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Не удалось обновить папку.",
                    error.Code))
            .Then(_ => unitOfWork.Commit())
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Успешно выполнена транзакция."))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Не удалось завершить транзакцию",
                    error.Code))
            .Finally(_ => ResultVoid.Success(),
                ResultVoid.Failure);
    }
}