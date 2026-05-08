using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Dapper;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Реализация репозитория для работы с альбомами в SQLite с использованием Dapper.
/// </summary>
public class SqliteAlbumRepository : IAlbumRepository
{
    private readonly SqliteUnitOfWork _unitOfWork;
    private readonly ILogger<SqliteAlbumRepository> _logger;

    /// <summary>
    ///     Инициализирует новый экземпляр репозитория альбомов.
    /// </summary>
    /// <param name="unitOfWork">Экземпляр Unit of Work для доступа к соединению и транзакции.</param>
    /// <param name="logger">Логгер для записи ошибок.</param>
    public SqliteAlbumRepository(
        SqliteUnitOfWork unitOfWork,
        ILogger<SqliteAlbumRepository> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                _logger.LogError("Connection is null in GetById");
                return Result<Album>.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            // Получаем базовые данные альбома
            var albumData = connection.QueryFirstOrDefault(
                "SELECT Id, Name, FolderId FROM Albums WHERE Id = @id",
                new { id },
                _unitOfWork.Transaction);

            if (albumData == null)
            {
                _logger.LogWarning("Album with id {Id} not found", id);
                return Result<Album>.Failure(new Error("Album.NotFound", $"Альбом с идентификатором {id} не найден."));
            }

            // Создаем альбом через фабричный метод
            var albumDataName = Convert.ToString(albumData.Name);
            var albumDataId = Convert.ToInt32(albumData.Id);
            var createResult = Album.Create(albumDataName, albumDataId);
            if (createResult.IsFailure)
            {
                return Result<Album>.Failure(createResult.Error);
            }

            var album = createResult.Value;

            // Устанавливаем FolderId
            if (albumData.FolderId != null)
            {
                var folderIdProperty = typeof(Album).GetProperty("FolderId");
                folderIdProperty?.SetValue(album, albumData.FolderId);
            }

            // Получаем список фотографий в альбоме
            var photoIds = connection.Query<int>(
                "SELECT PhotoId FROM AlbumPhotos WHERE AlbumId = @albumId",
                new { albumId = id },
                _unitOfWork.Transaction).ToList();

            // Восстанавливаем фотографии
            var restoreMethod = typeof(Album).GetMethod("RestorePhotos",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            restoreMethod?.Invoke(album, new object[] { photoIds });

            _logger.LogDebug("Album {Id} retrieved with {PhotoCount} photos", id, photoIds.Count);
            return Result<Album>.Success(album);
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "SQLite error in GetById for album {Id}", id);
            return Result<Album>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetById for album {Id}", id);
            return Result<Album>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Добавляет новый альбом в репозиторий.
    /// </summary>
    /// <param name="album">Объект альбома для добавления.</param>
    /// <returns>Результат операции.</returns>
    public ResultVoid Add(Album album)
    {
        if (album == null)
        {
            return ResultVoid.Failure(new Error("Album.Null", "Альбом не может быть null."));
        }

        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                _logger.LogError("Connection is null in Add");
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            // Вставляем альбом
            connection.Execute(
                "INSERT INTO Albums (Id, Name, FolderId) VALUES (@Id, @Name, @FolderId)",
                new { album.Id, album.Name, album.FolderId },
                _unitOfWork.Transaction);

            // Вставляем связи с фотографиями
            foreach (var photoId in album.PhotoIds)
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
                    _logger.LogWarning(ex, "Duplicate photo {PhotoId} in album {AlbumId}", photoId, album.Id);
                    return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
                }
            }

            _logger.LogDebug("Album {Id} added successfully", album.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "SQLite error in Add for album {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Add for album {Id}", album.Id);
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
        if (album == null)
        {
            return ResultVoid.Failure(new Error("Album.Null", "Альбом не может быть null."));
        }

        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                _logger.LogError("Connection is null in Update");
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            // Обновляем базовые данные альбома
            var rowsAffected = connection.Execute(
                "UPDATE Albums SET Name = @Name, FolderId = @FolderId WHERE Id = @Id",
                new { album.Id, album.Name, album.FolderId },
                _unitOfWork.Transaction);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Album {Id} not found for update", album.Id);
                return ResultVoid.Failure(new Error("Album.NotFound", $"Альбом с идентификатором {album.Id} не найден."));
            }

            // Удаляем старые связи
            connection.Execute(
                "DELETE FROM AlbumPhotos WHERE AlbumId = @AlbumId",
                new { AlbumId = album.Id },
                _unitOfWork.Transaction);

            // Вставляем новые связи
            foreach (var photoId in album.PhotoIds)
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
                    _logger.LogWarning(ex, "Duplicate photo {PhotoId} in album {AlbumId}", photoId, album.Id);
                    return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
                }
            }

            _logger.LogDebug("Album {Id} updated successfully", album.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "SQLite error in Update for album {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Update for album {Id}", album.Id);
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
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                _logger.LogError("Connection is null in Delete");
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            // Удаляем связи с фотографиями
            connection.Execute(
                "DELETE FROM AlbumPhotos WHERE AlbumId = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            // Удаляем альбом
            var rowsAffected = connection.Execute(
                "DELETE FROM Albums WHERE Id = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Album {Id} not found for delete", id);
                return ResultVoid.Failure(new Error("Album.NotFound", $"Альбом с идентификатором {id} не найден."));
            }

            _logger.LogDebug("Album {Id} deleted successfully", id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "SQLite error in Delete for album {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Delete for album {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }
}