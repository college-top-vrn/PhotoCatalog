using PhotoCatalog.Application.DTOs;
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
/// <param name="folderRepository">Репозиторий папок.</param>
/// <param name="unitOfWork">Единица работы.</param>
/// <param name="logger">Логгер.</param>
public class CreateFolderUseCase(IFolderRepository folderRepository, IUnitOfWork unitOfWork, ILogger logger)
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
            Result<Folder> parentResult = folderRepository.GetById(request.ParentFolderId.Value);
            if (parentResult.IsFailure)
            {
                logger.Warning("Родительская папка с Id {ParentId} не найдена.", request.ParentFolderId);
                return Result<FolderResponse>.Failure(ApplicationErrors.General.NotFound);
            }

            parentFolder = parentResult.Value!;
        }

        Result<Folder> createFolderResult = Folder.Create(parentFolder!.Id, request.Name);
        if (createFolderResult.IsFailure)
        {
            logger.Warning("Не удалось создать папку с Id {ParentId}: {ErrorCode}: {Error}",
                parentFolder.Id,
                createFolderResult.Error.Code,
                createFolderResult.Error.Message);
            return Result<FolderResponse>.Failure(createFolderResult.Error);
        }

        Folder folder = createFolderResult.Value!;

        ResultVoid beginTransactionResult = unitOfWork.BeginTransaction();
        if (beginTransactionResult.IsFailure)
        {
            logger.Error("Не удалось начать транзакцию: {ErrorCode}: {Error}",
                beginTransactionResult.Error.Code,
                beginTransactionResult.Error.Message);
        }

        ResultVoid addFolderResult = folderRepository.Add(folder);
        if (addFolderResult.IsFailure)
        {
            logger.Error("Не удалось добавить папку в репозиторий: {ErrorCode}: {Error}",
                addFolderResult.Error.Code,
                addFolderResult.Error.Message);
            unitOfWork.Rollback();
            return Result<FolderResponse>.Failure(addFolderResult.Error);
        }

        ResultVoid commitResult = unitOfWork.Commit();
        if (commitResult.IsFailure)
        {
            logger.Error("Не удалось зафиксировать изменения транзакции: {ErrorCode}: {Error}",
                commitResult.Error.Code,
                commitResult.Error.Message);
        }

        FolderResponse response = new(folder.Id, folder.Name, folder.ParentFolderId);
        return Result<FolderResponse>.Success(response);
    }
}