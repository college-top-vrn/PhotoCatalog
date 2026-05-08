using System.Data;
using System.Reflection;

using Dapper;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;
using PhotoCatalog.Infrastructure.Errors;

namespace PhotoCatalog.Infrastructure.Persistence;

/// <summary>
///     Реализация <see cref="IPhotoRepository"/> для SQLite на базе Dapper.
/// </summary>
public class SqlitePhotoRepository : IPhotoRepository
{
    private static readonly Error PhotoNotFound = new("Photo.NotFound", "Фотография не найдена.");
    private static readonly PropertyInfo? DimensionsProperty =
        typeof(Photo).GetProperty(nameof(Photo.Dimensions), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private readonly ILogger<SqlitePhotoRepository> _logger;
    private readonly SqliteUnitOfWork _unitOfWork;

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="SqlitePhotoRepository"/>.
    /// </summary>
    /// <param name="unitOfWork">Источник текущего SQLite-соединения и транзакции.</param>
    /// <param name="logger">Логгер для инфраструктурных ошибок.</param>
    public SqlitePhotoRepository(SqliteUnitOfWork unitOfWork, ILogger<SqlitePhotoRepository> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public Result<Photo> GetById(int id)
    {
        const string sql = """
                           SELECT Id, RealPath, FileHash, AddedAt, Width, Height
                           FROM Photos
                           WHERE Id = @Id
                           """;

        try
        {
            Photo? photo = QuerySinglePhoto(sql, new { Id = id });
            if (photo is null)
            {
                return Result<Photo>.Failure(PhotoNotFound);
            }

            IReadOnlyCollection<int> tags = QueryTagsByPhotoId(id);
            photo.RestoreTags(tags);
            return Result<Photo>.Success(photo);
        }
        catch (SqliteException ex)
        {
            return HandleDatabaseError<Photo>(ex, "Не удалось получить фотографию по Id {PhotoId}.", id);
        }
    }

    /// <inheritdoc />
    public Result<Photo> GetByPath(string realPath)
    {
        const string sql = """
                           SELECT Id, RealPath, FileHash, AddedAt, Width, Height
                           FROM Photos
                           WHERE RealPath = @RealPath
                           """;

        try
        {
            Photo? photo = QuerySinglePhoto(sql, new { RealPath = realPath });
            if (photo is null)
            {
                return Result<Photo>.Failure(PhotoNotFound);
            }

            IReadOnlyCollection<int> tags = QueryTagsByPhotoId(photo.Id);
            photo.RestoreTags(tags);
            return Result<Photo>.Success(photo);
        }
        catch (SqliteException ex)
        {
            return HandleDatabaseError<Photo>(ex, "Не удалось получить фотографию по пути {RealPath}.", realPath);
        }
    }

    /// <inheritdoc />
    public ResultVoid Add(Photo photo)
    {
        const string insertPhotoSql = """
                                      INSERT INTO Photos (RealPath, FileHash, Width, Height, AddedAt)
                                      VALUES (@RealPath, @FileHash, @Width, @Height, @AddedAt);
                                      SELECT last_insert_rowid();
                                      """;

        try
        {
            int photoId = (int)_unitOfWork.CurrentConnection.ExecuteScalar<long>(
                insertPhotoSql,
                new
                {
                    photo.RealPath,
                    photo.FileHash,
                    Width = photo.Dimensions.Width,
                    Height = photo.Dimensions.Height,
                    photo.AddedAt
                },
                _unitOfWork.CurrentTransaction);

            SyncPhotoTags(photoId, photo.TagIds);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            return HandleDatabaseError(ex, "Не удалось добавить фотографию с путем {RealPath}.", photo.RealPath);
        }
    }

    /// <inheritdoc />
    public ResultVoid Update(Photo photo)
    {
        const string updatePhotoSql = """
                                      UPDATE Photos
                                      SET RealPath = @RealPath,
                                          FileHash = @FileHash,
                                          Width = @Width,
                                          Height = @Height,
                                          AddedAt = @AddedAt
                                      WHERE Id = @Id
                                      """;

        try
        {
            int affectedRows = _unitOfWork.CurrentConnection.Execute(
                updatePhotoSql,
                new
                {
                    photo.Id,
                    photo.RealPath,
                    photo.FileHash,
                    Width = photo.Dimensions.Width,
                    Height = photo.Dimensions.Height,
                    photo.AddedAt
                },
                _unitOfWork.CurrentTransaction);

            if (affectedRows == 0)
            {
                return ResultVoid.Failure(PhotoNotFound);
            }

            SyncPhotoTags(photo.Id, photo.TagIds);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            return HandleDatabaseError(ex, "Не удалось обновить фотографию с Id {PhotoId}.", photo.Id);
        }
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        const string sql = """
                           DELETE FROM Photos
                           WHERE Id = @Id
                           """;

        try
        {
            int affectedRows = _unitOfWork.CurrentConnection.Execute(
                sql,
                new { Id = id },
                _unitOfWork.CurrentTransaction);

            return affectedRows == 0
                ? ResultVoid.Failure(PhotoNotFound)
                : ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            return HandleDatabaseError(ex, "Не удалось удалить фотографию с Id {PhotoId}.", id);
        }
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<Photo>> GetByAlbumId(int albumId)
    {
        const string sql = """
                           SELECT p.Id, p.RealPath, p.FileHash, p.AddedAt, p.Width, p.Height
                           FROM Photos p
                           INNER JOIN AlbumPhotos ap ON ap.PhotoId = p.Id
                           WHERE ap.AlbumId = @AlbumId
                           """;

        try
        {
            IReadOnlyCollection<Photo> photos = QueryPhotoCollection(sql, new { AlbumId = albumId });
            HydrateTagsForPhotos(photos);
            return Result<IReadOnlyCollection<Photo>>.Success(photos);
        }
        catch (SqliteException ex)
        {
            return HandleDatabaseError<IReadOnlyCollection<Photo>>(ex,
                "Не удалось получить фотографии альбома с Id {AlbumId}.", albumId);
        }
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<Photo>> GetByTags(IEnumerable<int> tagIds)
    {
        int[] tagArray = tagIds as int[] ?? tagIds.Distinct().ToArray();
        if (tagArray.Length == 0)
        {
            return Result<IReadOnlyCollection<Photo>>.Success(Array.Empty<Photo>());
        }

        const string sql = """
                           SELECT p.Id, p.RealPath, p.FileHash, p.AddedAt, p.Width, p.Height
                           FROM Photos p
                           WHERE p.Id IN (
                               SELECT pt.PhotoId
                               FROM PhotoTags pt
                               WHERE pt.TagId IN @TagIds
                               GROUP BY pt.PhotoId
                               HAVING COUNT(DISTINCT pt.TagId) = @TagCount
                           )
                           """;

        try
        {
            IReadOnlyCollection<Photo> photos = QueryPhotoCollection(sql, new { TagIds = tagArray, TagCount = tagArray.Length });
            HydrateTagsForPhotos(photos);
            return Result<IReadOnlyCollection<Photo>>.Success(photos);
        }
        catch (SqliteException ex)
        {
            return HandleDatabaseError<IReadOnlyCollection<Photo>>(ex,
                "Не удалось получить фотографии по тегам.");
        }
    }

    private Photo? QuerySinglePhoto(string sql, object parameters)
    {
        return _unitOfWork.CurrentConnection.Query<Photo, int, int, Photo>(
                sql,
                (photo, width, height) =>
                {
                    Dimensions dimensions = Dimensions.Create(width, height).Value!;
                    SetPhotoDimensions(photo, dimensions);
                    return photo;
                },
                parameters,
                _unitOfWork.CurrentTransaction,
                splitOn: "Width,Height")
            .SingleOrDefault();
    }

    private IReadOnlyCollection<Photo> QueryPhotoCollection(string sql, object parameters)
    {
        return _unitOfWork.CurrentConnection.Query<Photo, int, int, Photo>(
                sql,
                (photo, width, height) =>
                {
                    Dimensions dimensions = Dimensions.Create(width, height).Value!;
                    SetPhotoDimensions(photo, dimensions);
                    return photo;
                },
                parameters,
                _unitOfWork.CurrentTransaction,
                splitOn: "Width,Height")
            .ToArray();
    }

    private IReadOnlyCollection<int> QueryTagsByPhotoId(int id)
    {
        const string tagsSql = """
                               SELECT TagId
                               FROM PhotoTags
                               WHERE PhotoId = @Id
                               """;

        return _unitOfWork.CurrentConnection.Query<int>(tagsSql, new { Id = id }, _unitOfWork.CurrentTransaction).ToArray();
    }

    private void HydrateTagsForPhotos(IReadOnlyCollection<Photo> photos)
    {
        if (photos.Count == 0)
        {
            return;
        }

        int[] photoIds = photos.Select(x => x.Id).ToArray();
        const string tagsSql = """
                               SELECT PhotoId, TagId
                               FROM PhotoTags
                               WHERE PhotoId IN @PhotoIds
                               """;

        Dictionary<int, int[]> groupedTags = _unitOfWork.CurrentConnection
            .Query<PhotoTagLink>(tagsSql, new { PhotoIds = photoIds }, _unitOfWork.CurrentTransaction)
            .GroupBy(link => link.PhotoId)
            .ToDictionary(group => group.Key, group => group.Select(link => link.TagId).ToArray());

        foreach (Photo photo in photos)
        {
            groupedTags.TryGetValue(photo.Id, out int[]? tags);
            photo.RestoreTags(tags ?? Array.Empty<int>());
        }
    }

    private void SyncPhotoTags(int photoId, IEnumerable<int> tagIds)
    {
        const string deleteTagsSql = """
                                     DELETE FROM PhotoTags
                                     WHERE PhotoId = @PhotoId
                                     """;
        const string insertTagSql = """
                                    INSERT INTO PhotoTags (PhotoId, TagId)
                                    VALUES (@PhotoId, @TagId)
                                    """;

        _unitOfWork.CurrentConnection.Execute(deleteTagsSql, new { PhotoId = photoId }, _unitOfWork.CurrentTransaction);

        int[] tagsToInsert = tagIds.Distinct().ToArray();
        if (tagsToInsert.Length == 0)
        {
            return;
        }

        _unitOfWork.CurrentConnection.Execute(
            insertTagSql,
            tagsToInsert.Select(tagId => new { PhotoId = photoId, TagId = tagId }),
            _unitOfWork.CurrentTransaction);
    }

    private ResultVoid HandleDatabaseError(SqliteException ex, string messageTemplate, params object[] args)
    {
        _logger.LogError(ex, messageTemplate, args);
        return ResultVoid.Failure(MapSqliteError(ex));
    }

    private Result<T> HandleDatabaseError<T>(SqliteException ex, string messageTemplate, params object[] args)
    {
        _logger.LogError(ex, messageTemplate, args);
        return Result<T>.Failure(MapSqliteError(ex));
    }

    private static Error MapSqliteError(SqliteException ex)
    {
        return ex.SqliteErrorCode == 19 && ex.Message.Contains("Photos.RealPath", StringComparison.OrdinalIgnoreCase)
            ? InfrastructureErrors.Database.ConstraintViolation
            : InfrastructureErrors.Database.ConnectionFailed;
    }

    private static void SetPhotoDimensions(Photo photo, Dimensions dimensions)
    {
        DimensionsProperty?.SetValue(photo, dimensions);
    }

    private record PhotoTagLink(int PhotoId, int TagId);
}