using System;
using System.Collections.Generic;
using System.Linq;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Реализация репозитория для операций чтения данных об альбомах в SQLite с использованием Dapper.
/// </summary>
/// <remarks>
///     <para>
///         Данный репозиторий работает независимо от контекста транзакций,
///         открывая собственные легковесные подключения к базе данных.
///     </para>
///     <para>
///         Все методы возвращают иммутабельные коллекции и не изменяют состояние базы данных.
///     </para>
/// </remarks>
public class SqliteAlbumQueryRepository : IAlbumQueryRepository
{
    private readonly ILogger _logger;
    private readonly string _connectionString;

    /// <summary>
    ///     Инициализирует новый экземпляр репозитория для чтения альбомов.
    /// </summary>
    /// <param name="connectionString">Строка подключения к базе данных SQLite.</param>
    /// <param name="logger">Логгер Serilog для записи ошибок.</param>
    /// <exception cref="ArgumentNullException">
    ///     Выбрасывается, если <paramref name="connectionString" /> или <paramref name="logger" /> равен null.
    /// </exception>
    public SqliteAlbumQueryRepository(string connectionString, ILogger logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
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
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();

            var albumData = connection.QueryFirstOrDefault(
                "SELECT Id, Name, FolderId FROM Albums WHERE Id = @Id",
                new { Id = id });

            if (albumData == null)
            {
                _logger.Warning("Альбом с идентификатором {Id} не найден", id);
                return Result<Album>.Failure(InfrastructureErrors.Database.NotFound);
            }

            Result<Album> createResult = Album.Create(albumData.Name, albumData.Id);
            if (createResult.IsFailure)
            {
                return Result<Album>.Failure(createResult.Error);
            }

            Album album = createResult.Value;

            if (albumData.FolderId != null)
            {
                typeof(Album).GetProperty("FolderId")?.SetValue(album, albumData.FolderId);
            }

            List<int> photoIds = connection.Query<int>(
                "SELECT PhotoId FROM AlbumPhotos WHERE AlbumId = @AlbumId",
                new { AlbumId = id }).ToList();

            typeof(Album).GetMethod("RestorePhotos",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(album, new object[] { photoIds });

            _logger.Debug("Альбом {Id} получен с {PhotoCount} фотографиями", id, photoIds.Count);
            return Result<Album>.Success(album);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetById для альбома {Id}", id);
            return Result<Album>.Failure(InfrastructureErrors.Database.Sqlite);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetById для альбома {Id}", id);
            return Result<Album>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Получает коллекцию всех альбомов, находящихся внутри указанной папки.
    /// </summary>
    /// <param name="folderId">Уникальный идентификатор папки.</param>
    /// <returns>Результат операции с коллекцией альбомов или ошибкой.</returns>
    public Result<IReadOnlyCollection<Album>> GetByFolderId(int folderId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();

            var albumsData = connection.Query(
                "SELECT Id, Name, FolderId FROM Albums WHERE FolderId = @FolderId",
                new { FolderId = folderId });

            var albums = new List<Album>();

            foreach (var albumData in albumsData)
            {
                Result<Album> createResult = Album.Create(albumData.Name, albumData.Id);
                if (createResult.IsFailure)
                {
                    _logger.Warning("Пропуск альбома {Id}: {Error}", albumData.Id, createResult.Error.Message);
                    continue;
                }

                Album album = createResult.Value;

                if (albumData.FolderId != null)
                {
                    typeof(Album).GetProperty("FolderId")?.SetValue(album, albumData.FolderId);
                }

                List<int> photoIds = connection.Query<int>(
                    "SELECT PhotoId FROM AlbumPhotos WHERE AlbumId = @AlbumId",
                    new { AlbumId = albumData.Id }).ToList();

                typeof(Album).GetMethod("RestorePhotos",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(album, new object[] { photoIds });

                albums.Add(album);
            }

            _logger.Debug("Получено {Count} альбомов для папки {FolderId}", albums.Count, folderId);
            return Result<IReadOnlyCollection<Album>>.Success(albums.AsReadOnly());
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе GetByFolderId для папки {FolderId}", folderId);
            return Result<IReadOnlyCollection<Album>>.Failure(InfrastructureErrors.Database.Sqlite);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetByFolderId для папки {FolderId}", folderId);
            return Result<IReadOnlyCollection<Album>>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }
}