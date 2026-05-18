using System;
using System.Collections.Generic;
using System.Linq;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Реализация репозитория для работы с получением фотографий в SQLite с использованием Dapper.
/// </summary>
public class SqlitePhotoQueryRepository
{
    private readonly SqliteUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    /// <summary>
    ///     Инициализирует новый экземпляр репозитория фотографий.
    /// </summary>
    /// <param name="unitOfWork">Экземпляр Unit of Work для доступа к соединению и транзакции.</param>
    /// <param name="logger">Логгер для записи ошибок.</param>
    public SqlitePhotoQueryRepository(SqliteUnitOfWork unitOfWork, ILogger logger)
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

            var photo = connection.QueryFirstOrDefault<Photo>(
                "SELECT Id, RealPath, FileHash, Dimensions, AddedAt FROM Photos WHERE Id = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            if (photo == null)
            {
                return Result<Photo>.Failure(DomainErrors.Photo.NotFound);
            }

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

            var photo = connection.QueryFirstOrDefault<Photo>(
                "SELECT Id, RealPath, FileHash, Dimensions, AddedAt FROM Photos WHERE RealPath = @realPath",
                new { realPath },
                _unitOfWork.Transaction);

            if (photo == null)
            {
                return Result<Photo>.Failure(DomainErrors.Photo.NotFound);
            }

            var tagIds = connection.Query<int>(
                "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                new { Id = photo.Id },
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

            var photos = connection.Query<Photo>(
                @"SELECT p.Id, p.RealPath, p.FileHash, p.Dimensions, p.AddedAt 
                  FROM Photos p
                  INNER JOIN AlbumPhotos ap ON p.Id = ap.PhotoId
                  WHERE ap.AlbumId = @albumId",
                new { albumId },
                _unitOfWork.Transaction).ToList();

            foreach (var photo in photos)
            {
                var tagIds = connection.Query<int>(
                    "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                    new { Id = photo.Id },
                    _unitOfWork.Transaction);

                photo.RestoreTags(tagIds);
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

            var photos = connection.Query<Photo>(
                @"SELECT p.Id, p.RealPath, p.FileHash, p.Dimensions, p.AddedAt 
                  FROM Photos p
                  INNER JOIN PhotoTags pt ON p.Id = pt.PhotoId
                  WHERE pt.TagId IN @tagIds",
                new { tagIds = tagIdList },
                _unitOfWork.Transaction).ToList();

            foreach (var photo in photos)
            {
                var tagIdListForPhoto = connection.Query<int>(
                    "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                    new { Id = photo.Id },
                    _unitOfWork.Transaction);

                photo.RestoreTags(tagIdListForPhoto);
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