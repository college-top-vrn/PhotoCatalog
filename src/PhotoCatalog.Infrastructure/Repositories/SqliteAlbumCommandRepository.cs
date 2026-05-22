using Dapper;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Реализация репозитория для операций записи альбомов в SQLite.
/// </summary>
/// <remarks>
///     Работает строго в контексте транзакций IUnitOfWork.
///     Не вызывает Commit самостоятельно.
/// </remarks>
public class SqliteAlbumCommandRepository : IAlbumCommandRepository
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Инициализирует новый экземпляр репозитория для записи альбомов.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work для доступа к транзакции.</param>
    /// <param name="logger">Логгер Serilog.</param>
    public SqliteAlbumCommandRepository(IUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public ResultVoid Add(Album album)
    {
        if (album == null)
        {
            return ResultVoid.Failure(DomainErrors.Album.NullAlbum);
        }

        var sqliteUnitOfWork = _unitOfWork as SqliteUnitOfWork;
        if (sqliteUnitOfWork?.Connection == null)
        {
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
            connection.Execute(
                "INSERT INTO AlbumPhotos (AlbumId, PhotoId) VALUES (@AlbumId, @PhotoId)",
                new { AlbumId = album.Id, PhotoId = photoId },
                transaction);
        }

        _logger.Debug("Альбом {Id} добавлен", album.Id);
        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Update(Album album)
    {
        if (album == null)
        {
            return ResultVoid.Failure(DomainErrors.Album.NullAlbum);
        }

        var sqliteUnitOfWork = _unitOfWork as SqliteUnitOfWork;
        if (sqliteUnitOfWork?.Connection == null)
        {
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
            return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
        }

        connection.Execute(
            "DELETE FROM AlbumPhotos WHERE AlbumId = @AlbumId",
            new { AlbumId = album.Id },
            transaction);

        foreach (int photoId in album.PhotoIds)
        {
            connection.Execute(
                "INSERT INTO AlbumPhotos (AlbumId, PhotoId) VALUES (@AlbumId, @PhotoId)",
                new { AlbumId = album.Id, PhotoId = photoId },
                transaction);
        }

        _logger.Debug("Альбом {Id} обновлен", album.Id);
        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        var sqliteUnitOfWork = _unitOfWork as SqliteUnitOfWork;
        if (sqliteUnitOfWork?.Connection == null)
        {
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
            return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
        }

        _logger.Debug("Альбом {Id} удален", id);
        return ResultVoid.Success();
    }
}