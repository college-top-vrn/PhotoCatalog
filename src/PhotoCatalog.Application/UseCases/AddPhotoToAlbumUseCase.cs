using System;
using System.Collections.Generic;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;
using Serilog.Events;

namespace PhotoCatalog.Application.UseCases;

/// <summary>
/// Сценарий использования для добавления фотографии в существующий альбом.
/// </summary>
public class AddPhotoToAlbumUseCase
{
    private IAlbumRepository _albumRepository;
    private IPhotoRepository _photoRepository;
    private IUnitOfWork _unitOfWork;
    private ILogger _logger;

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
        var photo = _photoRepository.GetById(photoId);

        if (photo.IsFailure)
        {
            _logger.Warning("Фото {PhotoId} не найдено", photoId);
            return ResultVoid.Failure(ApplicationErrors.General.NotFound);
        }

        var album = _albumRepository.GetById(albumId);

        if (album.IsFailure)
        {
            _logger.Warning("Альбом {AlbumId} не найден", albumId);
            return ResultVoid.Failure(ApplicationErrors.General.NotFound);
        }

        var addResult = album.Value!.AddPhoto(photoId);

        if (addResult.IsFailure)
        {
            _logger.Warning("Не удалось добавить фото {PhotoId} в альбом {AlbumId}: {Error}", photoId, albumId,
                addResult.Error);
            return
                ResultVoid.Failure(addResult
                    .Error); // TODO сделать ошибку для album в ApplicationErrors если есть дубликат.
        }

        var beginResult = _unitOfWork.BeginTransaction();
        if (beginResult.IsFailure) return ResultVoid.Failure(beginResult.Error);// TODO сделать ошибку транзакции в ApplicationErrors.

        try
        {
            var updateResult = _albumRepository.Update(album.Value);
            if (updateResult.IsFailure)
            {
                _unitOfWork.Rollback();
                return ResultVoid.Failure(updateResult.Error);// TODO сделать ошибку транзакции в ApplicationErrors.
            }

            var commitResult = _unitOfWork.Commit();
            if (commitResult.IsSuccess)
            {
                _logger.Information("Фото {PhotoId} успешно добавлено в альбом {AlbumId}", photoId, albumId);
                return ResultVoid.Success();
            }

            _unitOfWork.Rollback();
            return ResultVoid.Failure(commitResult.Error); // TODO сделать ошибку транзакции в ApplicationErrors.
        }
        catch (Exception ex)
        {
            _unitOfWork.Rollback();
            _logger.Error(ex, "Критическая ошибка при сохранении изменений для альбома {AlbumId}", albumId);
            return ResultVoid.Failure(default); // TODO сделать системную ошибку в ApplicationErrors.
        }
    }
}