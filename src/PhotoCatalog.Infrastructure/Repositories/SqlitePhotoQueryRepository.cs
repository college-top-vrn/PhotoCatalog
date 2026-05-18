using System;
using System.Collections.Generic;
using System.Linq;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Реализация репозитория для работы с получением фотографий в SQLite с использованием Dapper.
/// </summary>
public class SqlitePhotoQueryRepository : IPhotoQueryRepository
{
    private readonly ILogger _logger;
    private readonly SqliteUnitOfWork _unitOfWork;

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


    /// <inheritdoc />
    public Result<Photo> GetById(int id)
    {
        try
        {
            SqliteConnection? connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return Result.Failure<Photo>(InfrastructureErrors.Database.ConnectionFailed);
            }

            Photo? photo = connection.QueryFirstOrDefault<Photo>(
                "SELECT Id, RealPath, FileHash, Dimensions, AddedAt FROM Photos WHERE Id = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            if (photo == null)
            {
                return Result.Failure<Photo>(DomainErrors.Photo.NotFound);
            }

            IEnumerable<int> tagIds = connection.Query<int>(
                "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            photo.RestoreTags(tagIds);

            return Result.Success(photo);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetById для фотографии {Id}", id);
            return Result.Failure<Photo>(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetById для фотографии {Id}", id);
            return Result.Failure<Photo>(InfrastructureErrors.Database.ConnectionFailed);
        }
    }


    /// <inheritdoc />
    public Result<Photo> GetByPath(string realPath)
    {
        try
        {
            SqliteConnection? connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return Result.Failure<Photo>(InfrastructureErrors.Database.ConnectionFailed);
            }

            Photo? photo = connection.QueryFirstOrDefault<Photo>(
                "SELECT Id, RealPath, FileHash, Dimensions, AddedAt FROM Photos WHERE RealPath = @realPath",
                new { realPath },
                _unitOfWork.Transaction);

            if (photo == null)
            {
                return Result.Failure<Photo>(DomainErrors.Photo.NotFound);
            }

            IEnumerable<int> tagIds = connection.Query<int>(
                "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                new { photo.Id },
                _unitOfWork.Transaction);

            photo.RestoreTags(tagIds);

            return Result.Success(photo);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetByPath для пути {RealPath}", realPath);
            return Result.Failure<Photo>(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetByPath для пути {RealPath}", realPath);
            return Result.Failure<Photo>(InfrastructureErrors.Database.ConnectionFailed);
        }
    }


    /// <inheritdoc />
    public Result<IReadOnlyCollection<Photo>> GetByAlbumId(int albumId)
    {
        try
        {
            SqliteConnection? connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return Result.Failure<IReadOnlyCollection<Photo>>(InfrastructureErrors.Database.ConnectionFailed);
            }

            List<Photo> photos = connection.Query<Photo>(
                """
                SELECT p.Id, p.RealPath, p.FileHash, p.Dimensions, p.AddedAt 
                                  FROM Photos p
                                  INNER JOIN AlbumPhotos ap ON p.Id = ap.PhotoId
                                  WHERE ap.AlbumId = @albumId
                """,
                new { albumId },
                _unitOfWork.Transaction).ToList();

            foreach (Photo photo in photos)
            {
                IEnumerable<int> tagIds = connection.Query<int>(
                    "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                    new { photo.Id },
                    _unitOfWork.Transaction);

                photo.RestoreTags(tagIds);
            }

            return Result.Success<IReadOnlyCollection<Photo>>(photos.AsReadOnly());
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetByAlbumId для альбома {AlbumId}", albumId);
            return Result.Failure<IReadOnlyCollection<Photo>>(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetByAlbumId для альбома {AlbumId}", albumId);
            return Result.Failure<IReadOnlyCollection<Photo>>(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<Photo>> GetByTags(IEnumerable<int> tagIds)
    {
        try
        {
            SqliteConnection? connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return Result.Failure<IReadOnlyCollection<Photo>>(InfrastructureErrors.Database.ConnectionFailed);
            }

            List<int> tagIdList = tagIds.ToList();
            if (!tagIdList.Any())
            {
                return Result.Success<IReadOnlyCollection<Photo>>(new List<Photo>().AsReadOnly());
            }

            List<Photo> photos = connection.Query<Photo>(
                @"SELECT p.Id, p.RealPath, p.FileHash, p.Dimensions, p.AddedAt 
                  FROM Photos p
                  INNER JOIN PhotoTags pt ON p.Id = pt.PhotoId
                  WHERE pt.TagId IN @tagIds",
                new { tagIds = tagIdList },
                _unitOfWork.Transaction).ToList();

            foreach (Photo photo in photos)
            {
                IEnumerable<int> tagIdListForPhoto = connection.Query<int>(
                    "SELECT TagId FROM PhotoTags WHERE PhotoId = @Id",
                    new { photo.Id },
                    _unitOfWork.Transaction);

                photo.RestoreTags(tagIdListForPhoto);
            }

            return Result.Success<IReadOnlyCollection<Photo>>(photos.AsReadOnly());
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetByTags");
            return Result.Failure<IReadOnlyCollection<Photo>>(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetByTags");
            return Result.Failure<IReadOnlyCollection<Photo>>(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <inheritdoc />
    public Result<IEnumerable<Photo>> GetAll()
    {
        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return Result.Failure<IEnumerable<Photo>>(InfrastructureErrors.Database.ConnectionFailed);
            }

            var photos = connection.Query<Photo>(
                @"SELECT p.Id, p.RealPath, p.FileHash, p.Dimensions, p.AddedAt 
                  FROM Photos p",
                _unitOfWork.Transaction).ToList();


            return Result.Success<IEnumerable<Photo>>(photos.AsReadOnly());
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetAll");
            return Result.Failure<IEnumerable<Photo>>(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetAll");
            return Result.Failure<IEnumerable<Photo>>(InfrastructureErrors.Database.ConnectionFailed);
        }
    }
}