using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
///     Реализация репозитория для работы с альбомами в SQLite с использованием Dapper.
/// </summary>
public class SqliteAlbumRepository : IAlbumRepository
{
    private readonly ILogger _logger;
    private readonly SqliteUnitOfWork _unitOfWork;

    /// <summary>
    ///     Инициализирует новый экземпляр репозитория альбомов.
    /// </summary>
    /// <param name="unitOfWork">Экземпляр Unit of Work для доступа к соединению и транзакции.</param>
    /// <param name="logger">Логгер для записи ошибок.</param>
    public SqliteAlbumRepository(SqliteUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    ///     Получает альбом по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор альбома.</param>
    /// <returns>Результат операции с найденным альбомом или ошибкой.</returns>
    public Result<Album> GetById(int id)
    {
        try
        {
            SqliteConnection? connection = _unitOfWork.Connection;
            if (connection == null)
            {
                _logger.Error("Соединение отсутствует в методе GetById");
                return Result.Failure<Album>(InfrastructureErrors.Database.ConnectionFailed);
            }

            dynamic? albumData = connection.QueryFirstOrDefault(
                "SELECT Id, Name, FolderId FROM Albums WHERE Id = @id",
                new { id },
                _unitOfWork.Transaction);

            if (albumData == null)
            {
                _logger.Warning("Альбом с идентификатором {Id} не найден", id);
                return Result.Failure<Album>(DomainErrors.Album.NotFound);
            }

            dynamic? albumDataName = Convert.ToString(albumData.Name);
            dynamic? albumDataId = Convert.ToInt32(albumData.Id);
            dynamic? createResult = Album.Create(albumDataName, albumDataId);
            if (createResult.IsFailure)
            {
                return Result.Failure(createResult.Error);
            }

            dynamic? album = createResult.Value;

            if (albumData.FolderId != null)
            {
                PropertyInfo? folderIdProperty = typeof(Album).GetProperty("FolderId");
                if (folderIdProperty != null)
                {
                    folderIdProperty.SetValue(album, albumData.FolderId);
                }
            }

            List<int> photoIds = connection.Query<int>(
                "SELECT PhotoId FROM AlbumPhotos WHERE AlbumId = @albumId",
                new { albumId = id },
                _unitOfWork.Transaction).ToList();

            MethodInfo? restoreMethod = typeof(Album).GetMethod("RestorePhotos",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (restoreMethod != null)
            {
                restoreMethod.Invoke(album, new object[] { photoIds });
            }

            _logger.Debug("Альбом {Id} получен с {PhotoCount} фотографиями", id, photoIds.Count);
            return Result.Success(album);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetById для альбома {Id}", id);
            return Result.Failure<Album>(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetById для альбома {Id}", id);
            return Result.Failure<Album>(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Добавляет новый альбом в репозиторий.
    /// </summary>
    /// <param name="album">Объект альбома для добавления.</param>
    /// <returns>Результат операции.</returns>
    public ResultVoid Add(Album album)
    {
        try
        {
            SqliteConnection? connection = _unitOfWork.Connection;
            if (connection == null)
            {
                _logger.Error("Соединение отсутствует в методе Add");
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            connection.Execute(
                "INSERT INTO Albums (Id, Name, FolderId) VALUES (@Id, @Name, @FolderId)",
                new { album.Id, album.Name, album.FolderId },
                _unitOfWork.Transaction);

            foreach (int photoId in album.PhotoIds)
            {
                try
                {
                    connection.Execute(
                        "INSERT INTO AlbumPhotos (AlbumId, PhotoId) VALUES (@AlbumId, @PhotoId)",
                        new { AlbumId = album.Id, PhotoId = photoId },
                        _unitOfWork.Transaction);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    _logger.Warning(ex, "Дубликат фотографии {PhotoId} в альбоме {AlbumId}", photoId, album.Id);
                    return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
                }
            }

            _logger.Debug("Альбом {Id} успешно добавлен", album.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Add для альбома {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Add для альбома {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }


    /// <summary>
    ///     Обновляет существующий альбом.
    /// </summary>
    /// <param name="album">Объект альбома с обновленными данными.</param>
    /// <returns>Результат операции.</returns>
    public ResultVoid Update(Album album)
    {
        try
        {
            SqliteConnection? connection = _unitOfWork.Connection;
            if (connection == null)
            {
                _logger.Error("Соединение отсутствует в методе Update");
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            int rowsAffected = connection.Execute(
                "UPDATE Albums SET Name = @Name, FolderId = @FolderId WHERE Id = @Id",
                new { album.Id, album.Name, album.FolderId },
                _unitOfWork.Transaction);

            if (rowsAffected == 0)
            {
                _logger.Warning("Альбом {Id} не найден для обновления", album.Id);
                return ResultVoid.Failure(DomainErrors.Album.NotFound);
            }

            connection.Execute(
                "DELETE FROM AlbumPhotos WHERE AlbumId = @AlbumId",
                new { AlbumId = album.Id },
                _unitOfWork.Transaction);

            foreach (int photoId in album.PhotoIds)
            {
                try
                {
                    connection.Execute(
                        "INSERT INTO AlbumPhotos (AlbumId, PhotoId) VALUES (@AlbumId, @PhotoId)",
                        new { AlbumId = album.Id, PhotoId = photoId },
                        _unitOfWork.Transaction);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    _logger.Warning(ex, "Дубликат фотографии {PhotoId} в альбоме {AlbumId}", photoId, album.Id);
                    return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
                }
            }

            _logger.Debug("Альбом {Id} успешно обновлен", album.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Update для альбома {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Update для альбома {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Удаляет альбом по идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор альбома.</param>
    /// <returns>Результат операции.</returns>
    public ResultVoid Delete(int id)
    {
        try
        {
            SqliteConnection? connection = _unitOfWork.Connection;
            if (connection == null)
            {
                _logger.Error("Соединение отсутствует в методе Delete");
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            connection.Execute(
                "DELETE FROM AlbumPhotos WHERE AlbumId = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            int rowsAffected = connection.Execute(
                "DELETE FROM Albums WHERE Id = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            if (rowsAffected == 0)
            {
                _logger.Warning("Альбом {Id} не найден для удаления", id);
                return ResultVoid.Failure(DomainErrors.Album.NotFound);
            }

            _logger.Debug("Альбом {Id} успешно удален", id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Delete для альбома {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Delete для альбома {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    // TODO: Добавить XML документацию
    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Result<IReadOnlyCollection<Album>> GetByFolderId(int id)
    {
        throw new NotImplementedException();
    }
}