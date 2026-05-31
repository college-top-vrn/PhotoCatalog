using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Реализация репозитория для операций чтения альбомов в SQLite.
/// </summary>
public class SqliteAlbumQueryRepository : IAlbumQueryRepository
{
    private readonly string _connectionString;
    private readonly ILogger _logger;

    /// <summary>
    ///     Инициализирует новый экземпляр репозитория для чтения альбомов.
    /// </summary>
    /// <param name="connectionString">Строка подключения к SQLite.</param>
    /// <param name="logger">Логгер Serilog.</param>
    public SqliteAlbumQueryRepository(string connectionString, ILogger logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <inheritdoc />
    public Result<Album> GetById(int id)
    {
        using SqliteConnection connection = new(_connectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        dynamic? albumData = connection.QueryFirstOrDefault(
            "SELECT Id, Name, FolderId FROM Albums WHERE Id = @Id",
            new { Id = id });

        if (albumData == null)
        {
            _logger.Warning("Альбом с идентификатором {Id} не найден", id);
            return Result.Failure<Album>(InfrastructureErrors.Database.NotFound);
        }

        Result<Album> createResult = Album.Create(albumData.Name, albumData.Id);
        if (createResult.IsFailure)
        {
            return Result.Failure<Album>(createResult.ResultError);
        }

        Album? album = createResult.Value;

        if (albumData.FolderId != null)
        {
            typeof(Album).GetProperty("FolderId")?.SetValue(album, albumData.FolderId);
        }

        List<int> photoIds = connection.Query<int>(
            "SELECT PhotoId FROM AlbumPhotos WHERE AlbumId = @AlbumId",
            new { AlbumId = id }).ToList();

        typeof(Album).GetMethod("RestorePhotos", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(album, [photoIds]);


        _logger.Debug("Альбом {Id} получен с {PhotoCount} фотографиями", id, photoIds.Count);
        return album != null
            ? Result.Success(album)
            : Result.Failure<Album>(InfrastructureErrors.Database.NotFound);
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<Album>> GetByFolderId(int folderId)
    {
        using SqliteConnection connection = new(_connectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        IEnumerable<dynamic> albumsData = connection.Query(
            "SELECT Id, Name, FolderId FROM Albums WHERE FolderId = @FolderId",
            new { FolderId = folderId });

        List<Album> albums = [];

        foreach (dynamic albumData in albumsData)
        {
            Result<Album> createResult = Album.Create(albumData.Name, albumData.Id);
            if (createResult.IsFailure)
            {
                _logger.Warning("Пропуск альбома {Id}: {Error}", albumData.Id, createResult.ResultError.Message);
                continue;
            }

            Album? album = createResult.Value;

            if (albumData.FolderId != null)
            {
                typeof(Album).GetProperty("FolderId")?.SetValue(album, albumData.FolderId);
            }

            List<int> photoIds = connection.Query<int>(
                "SELECT PhotoId FROM AlbumPhotos WHERE AlbumId = @AlbumId",
                new { AlbumId = albumData.Id }).ToList();

            typeof(Album).GetMethod("RestorePhotos", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(album, [photoIds]);

            if (album != null)
            {
                albums.Add(album);
            }
        }

        _logger.Debug("Получено {Count} альбомов для папки {FolderId}", albums.Count, folderId);
        return Result.Success<IReadOnlyCollection<Album>>(albums.AsReadOnly());
    }
}