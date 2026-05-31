using PhotoCatalog.Application.DTOs.Folders;
using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;

namespace PhotoCatalog.Application.UseCases;

/// <summary>
///     Сценарий использования для создания папки.
/// </summary>
/// <param name="folderQueryRepository">Репозиторий папок.</param>
/// <param name="unitOfWork">Единица работы.</param>
/// <param name="logger">Логгер.</param>
public class CreateFolderUseCase(
    IFolderQueryRepository folderQueryRepository,
    IFolderCommandRepository folderCommandRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
{
    /// <summary>
    ///     Выполняет сценарий создания папки.
    /// </summary>
    /// <param name="request">Данные запроса.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>Успех – содержит <see cref="FolderResponse" /> с данными созданной папки.</item>
    ///         <item>Ошибка – если родитель не найден, доменная валидация не пройдена или произошёл сбой транзакции.</item>
    ///     </list>
    /// </returns>
    public Result<FolderResponse> Execute(CreateFolderRequest request)
    {
        Folder? parentFolder = null;
        if (request.ParentFolderId.HasValue)
        {
            Result<Folder> parentResult = folderQueryRepository.GetById(request.ParentFolderId.Value);
            if (parentResult.IsFailure)
            {
                logger.Warning("Родительская папка с Id {ParentId} не найдена.", request.ParentFolderId);
                return Result.Failure<FolderResponse>(ApplicationErrors.General.NotFound);
            }

            parentFolder = parentResult.Value;
        }

        if (parentFolder == null)
        {
            return Result.Failure<FolderResponse>(ApplicationErrors.General.NotFound);
        }

        Result<Folder> createFolderResult = Folder.Create(parentFolder.Id, request.Name);
        if (createFolderResult.IsFailure)
        {
            logger.Warning("Не удалось создать папку с Id {ParentId}: {ErrorCode}: {Error}",
                parentFolder.Id,
                createFolderResult.ResultError.Code,
                createFolderResult.ResultError.Message);
            return Result.Failure<FolderResponse>(createFolderResult.ResultError);
        }

        Folder? folder = createFolderResult.Value;

        ResultVoid beginTransactionResult = unitOfWork.BeginTransaction();
        if (beginTransactionResult.IsFailure)
        {
            logger.Error("Не удалось начать транзакцию: {ErrorCode}: {Error}",
                beginTransactionResult.ResultError.Code,
                beginTransactionResult.ResultError.Message);
        }

        if (folder == null)
        {
            return Result.Failure<FolderResponse>(ApplicationErrors.General.NotFound);
        }

        ResultVoid addFolderResult = folderCommandRepository.Add(folder);
        if (addFolderResult.IsFailure)
        {
            logger.Error("Не удалось добавить папку в репозиторий: {ErrorCode}: {Error}",
                addFolderResult.ResultError.Code,
                addFolderResult.ResultError.Message);
            unitOfWork.Rollback();
            return Result.Failure<FolderResponse>(addFolderResult.ResultError);
        }

        ResultVoid commitResult = unitOfWork.Commit();
        if (commitResult.IsFailure)
        {
            logger.Error("Не удалось зафиксировать изменения транзакции: {ErrorCode}: {Error}",
                commitResult.ResultError.Code,
                commitResult.ResultError.Message);
        }

        FolderResponse response = new(folder.Id, folder.Name, folder.ParentFolderId);
        return Result.Success(response);
    }
}