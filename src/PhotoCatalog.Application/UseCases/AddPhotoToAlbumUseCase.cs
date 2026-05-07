using System;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;

namespace PhotoCatalog.Application.UseCases;

/// <summary>
/// Сценарий использования для добавления фотографии в существующий альбом.
/// </summary>
public class AddPhotoToAlbumUseCase
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddPhotoToAlbumUseCase(IAlbumRepository albumRepository, IPhotoRepository photoRepository,
        IUnitOfWork unitOfWork, ILogger logger)
    {
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddPhotoToAlbumUseCase>();
    }

    /// <summary>
    /// Выполняет добавление фотографии в альбом с сохранением изменений в рамках транзакции.
    /// </summary>
    /// <param name="albumId">Идентификатор альбома.</param>
    /// <param name="photoId">Идентификатор фотографии.</param>
    /// <returns>Результат выполнения операции (успех или ошибка).</returns>
    public ResultVoid Execute(int albumId, int photoId)
    {
        _logger.Information("Запуск процесса добавления фото {PhotoId} в альбом {AlbumId}", photoId, albumId);
        return _photoRepository.GetById(photoId)
            .OnSuccess(_ =>
                _logger.Information("Фото {PhotoId} найдено", photoId))
            .OnFailure(_ =>
                _logger.Warning("Фото {PhotoId} не найдено", photoId))
            .Then(photo =>
                _albumRepository.GetById(albumId)
                    .OnSuccess(_ =>
                        _logger.Information("Альбом {AlbumId} найден", albumId)))
            .OnFailure(_ =>
                _logger.Warning("Альбом {AlbumId} не найден", albumId))
            .Then(album => album.AddPhoto(photoId))
            .ToResult()
            .OnFailure(error =>
                _logger.Warning("Не удалось добавить фото {PhotoId} в альбом {AlbumId}: {Error}", photoId, albumId,
                    error))
            .Transform(_ => _unitOfWork.BeginTransaction())
            .Ensure(beginResult => beginResult.IsSuccess,
                ApplicationErrors.Transactions.StartTransactions)
            .Then(_ => _albumRepository.GetById(albumId)) // TODO Исправить костыль.
            .Transform(album => _albumRepository.Update(album))
            .Ensure(updateResult => updateResult.IsSuccess,
                ApplicationErrors.Albums.UpdateFailed)
            .Transform(_ => _unitOfWork.Commit())
            .Ensure(commitResult => commitResult.IsSuccess,
                ApplicationErrors.Transactions.CommitFailed)
            .Finally(success: _ => ResultVoid.Success(),
                failure: error => ResultVoid.Failure(ApplicationErrors.UseCases.SystemFailure));
    }
}