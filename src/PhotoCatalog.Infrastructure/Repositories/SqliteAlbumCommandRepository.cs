using System;

using Dapper;

using Microsoft.Data.Sqlite;

using Serilog;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Реализация репозитория для операций записи данных об альбомах в SQLite с использованием Dapper.
/// </summary>
/// <remarks>
///     <para>
///         Данный репозиторий работает строго в контексте транзакций, предоставляемых
///         через <see cref="IUnitOfWork" />. Все операции выполняются с использованием
///         общего подключения и транзакции.
///     </para>
///     <para>
///         <b>Важно:</b> Репозиторий НЕ вызывает Commit самостоятельно. Фиксация транзакции
///         является ответственностью вызывающего кода.
///     </para>
/// </remarks>
public class AlbumCommandRepository : IAlbumCommandRepository
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Инициализирует новый экземпляр репозитория для записи альбомов.
    /// </summary>
    /// <param name="unitOfWork">Экземпляр Unit of Work для доступа к соединению и транзакции.</param>
    /// <param name="logger">Логгер Serilog для записи ошибок.</param>
    /// <exception cref="ArgumentNullException">
    ///     Выбрасывается, если <paramref name="unitOfWork" /> или <paramref name="logger" /> равен null.
    /// </exception>
    public AlbumCommandRepository(IUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Добавляет новый альбом в систему.
    /// </summary>
    /// <param name="album">Объект альбома для добавления.</param>
    /// <returns>Результат операции.</returns>
    public ResultVoid Add(Album album)
    {
        if (album == null)
        {
            _logger.Warning("Попытка добавить null-альбом");
            return ResultVoid.Failure(DomainErrors.Album.NullAlbum);
        }

        try
        {
            var sqliteUnitOfWork = _unitOfWork as SqliteUnitOfWork;
            if (sqliteUnitOfWork?.Connection == null)
            {
                _logger.Error("Соединение отсутствует в методе Add. Убедитесь, что BeginTransaction был вызван.");
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var connection = sqliteUnitOfWork.Connection;
            var transaction = sqliteUnitOfWork.Transaction;

            connection.Execute(
                "INSERT INTO Albums (Id, Name, FolderId) VALUES (@Id, @Name, @FolderId)",
                new { album.Id, album.Name, album.FolderId },
                transaction);

            foreach (int photoId in album.PhotoIds)
            {
                try
                {
                    connection.Execute(
                        "INSERT INTO AlbumPhotos (AlbumId, PhotoId) VALUES (@AlbumId, @PhotoId)",
                        new { AlbumId = album.Id, PhotoId = photoId },
                        transaction);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    _logger.Warning(ex, "Нарушение уникальности: фотография {PhotoId} уже в альбоме {AlbumId}", photoId,
                        album.Id);
                    return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
                }
            }

            _logger.Debug("Альбом {Id} успешно добавлен", album.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            _logger.Error(ex, "Нарушение ограничений при добавлении альбома {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Add для альбома {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Add для альбома {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Обновляет данные существующего альбома.
    /// </summary>
    /// <param name="album">Объект альбома с обновленными данными.</param>
    /// <returns>Результат операции.</returns>
    public ResultVoid Update(Album album)
    {
        if (album == null)
        {
            _logger.Warning("Попытка обновить null-альбом");
            return ResultVoid.Failure(DomainErrors.Album.NullAlbum);
        }

        try
        {
            var sqliteUnitOfWork = _unitOfWork as SqliteUnitOfWork;
            if (sqliteUnitOfWork?.Connection == null)
            {
                _logger.Error("Соединение отсутствует в методе Update. Убедитесь, что BeginTransaction был вызван.");
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var connection = sqliteUnitOfWork.Connection;
            var transaction = sqliteUnitOfWork.Transaction;

            int rowsAffected = connection.Execute(
                "UPDATE Albums SET Name = @Name, FolderId = @FolderId WHERE Id = @Id",
                new { album.Id, album.Name, album.FolderId },
                transaction);

            if (rowsAffected == 0)
            {
                _logger.Warning("Альбом {Id} не найден для обновления", album.Id);
                return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
            }

            connection.Execute(
                "DELETE FROM AlbumPhotos WHERE AlbumId = @AlbumId",
                new { AlbumId = album.Id },
                transaction);

            foreach (int photoId in album.PhotoIds)
            {
                try
                {
                    connection.Execute(
                        "INSERT INTO AlbumPhotos (AlbumId, PhotoId) VALUES (@AlbumId, @PhotoId)",
                        new { AlbumId = album.Id, PhotoId = photoId },
                        transaction);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    _logger.Warning(ex, "Нарушение уникальности: фотография {PhotoId} уже в альбоме {AlbumId}", photoId,
                        album.Id);
                    return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
                }
            }

            _logger.Debug("Альбом {Id} успешно обновлен", album.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            _logger.Error(ex, "Нарушение ограничений при обновлении альбома {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Update для альбома {Id}", album.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
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
            var sqliteUnitOfWork = _unitOfWork as SqliteUnitOfWork;
            if (sqliteUnitOfWork?.Connection == null)
            {
                _logger.Error("Соединение отсутствует в методе Delete. Убедитесь, что BeginTransaction был вызван.");
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var connection = sqliteUnitOfWork.Connection;
            var transaction = sqliteUnitOfWork.Transaction;

            connection.Execute(
                "DELETE FROM AlbumPhotos WHERE AlbumId = @Id",
                new { Id = id },
                transaction);

            int rowsAffected = connection.Execute(
                "DELETE FROM Albums WHERE Id = @Id",
                new { Id = id },
                transaction);

            if (rowsAffected == 0)
            {
                _logger.Warning("Альбом {Id} не найден для удаления", id);
                return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
            }

            _logger.Debug("Альбом {Id} успешно удален", id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            _logger.Error(ex, "Нарушение ограничений при удалении альбома {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Delete для альбома {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Delete для альбома {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }
}