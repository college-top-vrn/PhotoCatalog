using System;
using System.Collections.Generic;
using System.Linq;

using Dapper;

using Microsoft.Data.Sqlite;

using Serilog;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Реализация репозитория для работы с фотографиями в SQLite с использованием Dapper.
/// </summary>
public class SqlitePhotoRepository : IPhotoRepository
{
    private readonly SqliteUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    /// <summary>
    ///     Инициализирует новый экземпляр репозитория фотографий.
    /// </summary>
    /// <param name="unitOfWork">Экземпляр Unit of Work для доступа к соединению и транзакции.</param>
    /// <param name="logger">Логгер для записи ошибок.</param>
    public SqlitePhotoRepository(SqliteUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    ///     Получает фотографию по уникальному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор фотографии.</param>
    /// <returns>Результат операции с найденной фотографией или ошибкой.</returns>
    public Result<Photo> GetById(int id)
    {
        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return Result<Photo>.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var photoData = connection.QueryFirstOrDefault(
                "SELECT Id, RealPath, FileHash, Width, Height, AddedAt FROM Photos WHERE Id = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            if (photoData == null)
            {
                return Result<Photo>.Failure(DomainErrors.Photo.NotFound);
            }

            var photo = Photo.Create(photoData.RealPath).Value;

            var idProperty = typeof(Photo).GetProperty("Id");
            idProperty?.SetValue(photo, photoData.Id);

            photo.UpdateHash(photoData.FileHash);

            var dimensions = Domain.ValueObjects.Dimensions.Create(photoData.Width, photoData.Height).Value;
            photo.SetDimensions(dimensions);

            var tagIds = connection.Query<int>(
                "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            photo.RestoreTags(tagIds);

            return Result<Photo>.Success(photo);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetById для фотографии {Id}", id);
            return Result<Photo>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetById для фотографии {Id}", id);
            return Result<Photo>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Получает фотографию по реальному пути к файлу.
    /// </summary>
    /// <param name="realPath">Реальный путь к файлу.</param>
    /// <returns>Результат операции с найденной фотографией или ошибкой.</returns>
    public Result<Photo> GetByPath(string realPath)
    {
        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return Result<Photo>.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var photoData = connection.QueryFirstOrDefault(
                "SELECT Id, RealPath, FileHash, Width, Height, AddedAt FROM Photos WHERE RealPath = @realPath",
                new { realPath },
                _unitOfWork.Transaction);

            if (photoData == null)
            {
                return Result<Photo>.Failure(DomainErrors.Photo.NotFound);
            }

            var photo = Photo.Create(photoData.RealPath).Value;

            var idProperty = typeof(Photo).GetProperty("Id");
            idProperty?.SetValue(photo, photoData.Id);

            photo.UpdateHash(photoData.FileHash);

            var dimensions = Domain.ValueObjects.Dimensions.Create(photoData.Width, photoData.Height).Value;
            photo.SetDimensions(dimensions);

            var tagIds = connection.Query<int>(
                "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                new { Id = photoData.Id },
                _unitOfWork.Transaction);

            photo.RestoreTags(tagIds);

            return Result<Photo>.Success(photo);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetByPath для пути {RealPath}", realPath);
            return Result<Photo>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetByPath для пути {RealPath}", realPath);
            return Result<Photo>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Добавляет новую фотографию в репозиторий.
    /// </summary>
    /// <param name="photo">Объект фотографии для добавления.</param>
    /// <returns>Результат операции.</returns>
    public ResultVoid Add(Photo photo)
    {
        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            connection.Execute(
                "INSERT INTO Photos (Id, RealPath, FileHash, Width, Height, AddedAt) VALUES (@Id, @RealPath, @FileHash, @Width, @Height, @AddedAt)",
                new
                {
                    photo.Id,
                    photo.RealPath,
                    photo.FileHash,
                    Width = photo.Dimensions.Width,
                    Height = photo.Dimensions.Height,
                    photo.AddedAt
                },
                _unitOfWork.Transaction);

            foreach (var tagId in photo.TagIds)
            {
                connection.Execute(
                    "INSERT INTO PhotoTags (PhotoId, TagId) VALUES (@PhotoId, @TagId)",
                    new { PhotoId = photo.Id, TagId = tagId },
                    _unitOfWork.Transaction);
            }

            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Add для фотографии {Id}", photo.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Add для фотографии {Id}", photo.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Обновляет существующую фотографию.
    /// </summary>
    /// <param name="photo">Объект фотографии с обновленными данными.</param>
    /// <returns>Результат операции.</returns>
    public ResultVoid Update(Photo photo)
    {
        if (photo == null)
        {
            return ResultVoid.Failure(DomainErrors.Photo.NullPhoto);
        }

        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var rowsAffected = connection.Execute(
                "UPDATE Photos SET RealPath = @RealPath, FileHash = @FileHash, Width = @Width, Height = @Height, AddedAt = @AddedAt WHERE Id = @Id",
                new
                {
                    photo.Id,
                    photo.RealPath,
                    photo.FileHash,
                    Width = photo.Dimensions.Width,
                    Height = photo.Dimensions.Height,
                    photo.AddedAt
                },
                _unitOfWork.Transaction);

            if (rowsAffected == 0)
            {
                return ResultVoid.Failure(DomainErrors.Photo.NotFound);
            }

            connection.Execute(
                "DELETE FROM PhotoTags WHERE PhotoId = @PhotoId",
                new { PhotoId = photo.Id },
                _unitOfWork.Transaction);

            foreach (var tagId in photo.TagIds)
            {
                connection.Execute(
                    "INSERT INTO PhotoTags (PhotoId, TagId) VALUES (@PhotoId, @TagId)",
                    new { PhotoId = photo.Id, TagId = tagId },
                    _unitOfWork.Transaction);
            }

            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Update для фотографии {Id}", photo.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Update для фотографии {Id}", photo.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Удаляет фотографию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор фотографии для удаления.</param>
    /// <returns>Результат операции.</returns>
    public ResultVoid Delete(int id)
    {
        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            connection.Execute(
                "DELETE FROM PhotoTags WHERE PhotoId = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            var rowsAffected = connection.Execute(
                "DELETE FROM Photos WHERE Id = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            if (rowsAffected == 0)
            {
                return ResultVoid.Failure(DomainErrors.Photo.NotFound);
            }

            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Delete для фотографии {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Delete для фотографии {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Получает все фотографии альбома.
    /// </summary>
    /// <param name="albumId">Идентификатор альбома.</param>
    /// <returns>Результат операции с коллекцией фотографий альбома.</returns>
    public Result<IReadOnlyCollection<Photo>> GetByAlbumId(int albumId)
    {
        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return Result<IReadOnlyCollection<Photo>>.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var photosData = connection.Query(
                @"SELECT p.Id, p.RealPath, p.FileHash, p.Width, p.Height, p.AddedAt 
                  FROM Photos p
                  INNER JOIN AlbumPhotos ap ON p.Id = ap.PhotoId
                  WHERE ap.AlbumId = @albumId",
                new { albumId },
                _unitOfWork.Transaction);

            var photos = new List<Photo>();
            foreach (var photoData in photosData)
            {
                var photo = Photo.Create(photoData.RealPath).Value;

                var idProperty = typeof(Photo).GetProperty("Id");
                idProperty?.SetValue(photo, photoData.Id);

                photo.UpdateHash(photoData.FileHash);

                var dimensions = Domain.ValueObjects.Dimensions.Create(photoData.Width, photoData.Height).Value;
                photo.SetDimensions(dimensions);

                var tagIds = connection.Query<int>(
                    "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                    new { Id = photoData.Id },
                    _unitOfWork.Transaction);

                photo.RestoreTags(tagIds);

                photos.Add(photo);
            }

            return Result<IReadOnlyCollection<Photo>>.Success(photos.AsReadOnly());
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetByAlbumId для альбома {AlbumId}", albumId);
            return Result<IReadOnlyCollection<Photo>>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetByAlbumId для альбома {AlbumId}", albumId);
            return Result<IReadOnlyCollection<Photo>>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Получает фотографии по списку идентификаторов тегов.
    /// </summary>
    /// <param name="tagIds">Идентификаторы тегов.</param>
    /// <returns>Результат операции с коллекцией фотографий, содержащих указанные теги.</returns>
    public Result<IReadOnlyCollection<Photo>> GetByTags(IEnumerable<int> tagIds)
    {
        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return Result<IReadOnlyCollection<Photo>>.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var tagIdList = tagIds.ToList();
            if (!tagIdList.Any())
            {
                return Result<IReadOnlyCollection<Photo>>.Success(new List<Photo>().AsReadOnly());
            }

            var photosData = connection.Query(
                @"SELECT p.Id, p.RealPath, p.FileHash, p.Width, p.Height, p.AddedAt 
                  FROM Photos p
                  INNER JOIN PhotoTags pt ON p.Id = pt.PhotoId
                  WHERE pt.TagId IN @tagIds",
                new { tagIds = tagIdList },
                _unitOfWork.Transaction);

            var photos = new List<Photo>();
            foreach (var photoData in photosData)
            {
                var photo = Photo.Create(photoData.RealPath).Value;

                var idProperty = typeof(Photo).GetProperty("Id");
                idProperty?.SetValue(photo, photoData.Id);

                photo.UpdateHash(photoData.FileHash);

                var dimensions = Domain.ValueObjects.Dimensions.Create(photoData.Width, photoData.Height).Value;
                photo.SetDimensions(dimensions);

                var tagIdListForPhoto = connection.Query<int>(
                    "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                    new { Id = photoData.Id },
                    _unitOfWork.Transaction);

                photo.RestoreTags(tagIdListForPhoto);

                photos.Add(photo);
            }

            return Result<IReadOnlyCollection<Photo>>.Success(photos.AsReadOnly());
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetByTags");
            return Result<IReadOnlyCollection<Photo>>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetByTags");
            return Result<IReadOnlyCollection<Photo>>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }
}