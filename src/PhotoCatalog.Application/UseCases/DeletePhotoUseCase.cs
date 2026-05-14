using Microsoft.Extensions.Logging;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.UseCases;

/// <summary>
///     Представляет прикладную сущность для удаления файла из репозитория и диска.
/// </summary>
/// <param name="photoRepository">репозиторий фотографий.</param>
/// <param name="fileStorage">хранение файлов.</param>
/// <param name="unitOfWork">единица работы.</param>
/// <param name="logger">логгер.</param>
public class DeletePhotoUseCase(
    IPhotoRepository photoRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ILogger<DeletePhotoUseCase> logger)
{
    /// <summary>
    ///     Метод для удаления файла в репозитории и на диске по идентификатору.
    /// </summary>
    /// <param name="photoId">идентификатор фотографии.</param>
    /// <returns>
    ///     Возвращает ResultVoid.Success(), если файл успешно удалился.
    ///     Возвращает ResultVoid.Failure(), если файл неуспешно удалился.
    /// </returns>
    public ResultVoid Execute(int photoId)
    {
        Result<Photo>? photo = photoRepository.GetById(photoId)
            .ToResult(ApplicationErrors.General.NotFound)
            .Value;

        unitOfWork.BeginTransaction();

        photoRepository.Delete(photoId);

        if (unitOfWork.Commit().IsSuccess)
        {
            fileStorage.DeleteFile(photo!.Value!.RealPath);

            return ResultVoid.Success();
        }

        logger.LogError("Orphaned file left on disk: {Path}", photo!.Value!.RealPath);

        return ResultVoid.Failure(ApplicationErrors.Files.OrphanedFile);
    }
}