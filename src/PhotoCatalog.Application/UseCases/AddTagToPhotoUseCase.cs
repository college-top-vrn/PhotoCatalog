using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;

namespace PhotoCatalog.Application.UseCases;

/// <summary>
///     Сценарий использования для добавления тега к фото.
/// </summary>
/// <param name="tagQueryRepository">Репозиторий тегов для получения данных.</param>
/// <param name="photoRepository">Репозиторий фото.</param>
/// <param name="unitOfWork">Едиинца работы.</param>
/// <param name="logger">Логгер.</param>
public class AddTagToPhotoUseCase(
    ITagQueryRepository tagQueryRepository,
    IPhotoRepository photoRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
{
    /// <summary>
    ///     Выполняет добавление тега к фотографии
    ///     с сохранением изменений в рамках транзакции.
    /// </summary>
    /// <param name="photoId">Идентификатор фото.</param>
    /// <param name="tagId">Идентификатор тега.</param>
    /// <returns>Результат выполнения операции (успех или ошибка).</returns>
    public ResultVoid Execute(int photoId, int tagId)
    {
        Photo? photoToUpdate = null;
        return tagQueryRepository.GetById(tagId)
            .OnSuccess(_ =>
                logger.Information("Тег {TagId} найден.", tagId))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Тег {TagId} не найден.",
                    error.Code,
                    tagId))
            .ToResult()
            .Then(_ =>
                photoRepository.GetById(photoId))
            .OnSuccess(data =>
            {
                logger.Information("Фото {PhotoId} найдено.", photoId);
                photoToUpdate = data;
            })
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Фото {PhotoId} не найдено.",
                    error.Code,
                    photoId))
            .Then(photo =>
                photo.AddTag(tagId))
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Добавлен тег {TagId} к фото {PhotoId}.",
                    tagId,
                    photoId))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Не удалось добавить тег {TagId} к фото {PhotoId}.",
                    error.Code,
                    tagId,
                    photoId))
            .Then(_ => unitOfWork.BeginTransaction())
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Успешно начата транзакция."))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Не удалось начать транзакцию.",
                    error.Code))
            .Ensure(_ =>
                    photoToUpdate != null,
                ApplicationErrors.General.NotFound)
            .Then(_ =>
                photoRepository.Update(photoToUpdate!))
            .ToResult()
            .OnSuccess(_ =>
                logger.Information("Успешно обновлено фото."))
            .OnFailure(error =>
                logger.Warning("Ошибка {ErrorCode}: Не удалось обновить фото.",
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