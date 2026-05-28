using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;

namespace PhotoCatalog.Application.UseCases;

/// <summary>
///     Сценарий использования для удаления папки.
/// </summary>
/// <param name="folderQueryRepository">Репозиторий папок для получения данных.</param>
/// <param name="folderCommandRepository">Репозиторий папок для изменения данных.</param>
/// <param name="unitOfWork">Единица работы.</param>
/// <param name="logger">Логгер.</param>
public class DeleteFolderUseCase(
    IFolderQueryRepository folderQueryRepository,
    IFolderCommandRepository folderCommandRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
{

    /// <summary>
    ///     Выполняет сценарий удаления папки.
    /// </summary>
    /// <param name="folderId">Идентификатор папки для удаления.</param>
    /// <returns>Результат выполнения сценария.</returns>
    public ResultVoid Execute(int folderId)
    {
        Result<Folder> folderResult = folderQueryRepository.GetById(folderId);

        if (folderResult.IsFailure)
        {
            logger.Warning("Не удалось найти папку с Id {FolderId}", folderId);
            return ResultVoid.Failure(ApplicationErrors.General.NotFound);
        }

        ResultVoid beginTransactionResult = unitOfWork.BeginTransaction();
        if (beginTransactionResult.IsFailure)
        {
            logger.Error("Не удалось начать транзакцию: {ErrorCode}: {Error}",
                beginTransactionResult.Error.Code,
                beginTransactionResult.Error.Message);
            return ResultVoid.Failure(beginTransactionResult.Error);
        }

        ResultVoid deleteResult = folderCommandRepository.Delete(folderId);
        if (deleteResult.IsFailure)
        {
            logger.Error("Не удалось удалить папку с Id {FolderId}: {ErrorCode}: {Error}",
                folderId,
                deleteResult.Error.Code,
                deleteResult.Error.Message);
            return ResultVoid.Failure(deleteResult.Error);
        }

        ResultVoid commitResult = unitOfWork.Commit();
        if (commitResult.IsSuccess)
        {
            return ResultVoid.Success();
        }

        logger.Error("Не удалось зафиксировать изменения транзакции: {ErrorCode}: {Error}",
            commitResult.Error.Code,
            commitResult.Error.Message);
        unitOfWork.Rollback();
        return ResultVoid.Failure(commitResult.Error);
    }
}