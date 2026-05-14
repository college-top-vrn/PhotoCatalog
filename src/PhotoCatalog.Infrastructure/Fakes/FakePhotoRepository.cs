using System.Collections.Concurrent;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Репозиторий фотографий, имитирующий БД, и хранящий данные в оперативной памяти.
/// </summary>
public class FakePhotoRepository(FakeAlbumRepository fakeAlbumRepository) : IPhotoRepository
{
    /// <summary>
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;

    private readonly FakeAlbumRepository _fakeAlbumRepository = fakeAlbumRepository;

    /// <summary>
    ///     Словарь альбомов.
    /// </summary>
    private readonly ConcurrentDictionary<int, Photo> _data = new();

    /// <summary>
    ///     Получение фотографии по идентификатору.
    /// </summary>
    /// <param name="id">идентификатор фотографии.</param>
    /// <returns>Фотография.</returns>
    public Result<Photo> GetById(int id)
    {
        foreach (var pair in _data)
        {
            if (pair.Key == id) return pair.Value.ToResult();
        }

        return Result<Photo>.Failure(new Error("PhotoRepository.PhotoNotFound",
            "Не удалось найти фотографию по идентификатору"));
    }

    /// <summary>
    ///     Получение фотографии по заданному пути.
    /// </summary>
    /// <param name="realPath">путь фотографии.</param>
    /// <returns>Фотография.</returns>
    public Result<Photo> GetByPath(string realPath)
    {
        foreach (var pair in _data)
        {
            if (pair.Value.RealPath == realPath) return pair.Value.ToResult();
        }

        return Result<Photo>.Failure(new Error("PhotoRepository.PhotoNotFound",
            "Не удалось найти фотографию по заданному пути"));
    }

    /// <summary>
    ///     Добавление фотографии.
    /// </summary>
    /// <param name="photo">фотография.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Add(Photo photo)
    {
        var result = _data
            .TryAdd(_lastId, photo)
            .ToResult();

        if (result.IsFailure)
            return ResultVoid.Failure(new Error("PhotoRepository.CantAddPhoto",
                "Не удалось добавить фото"));

        _lastId += 1;

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Обновление фотографии.
    /// </summary>
    /// <param name="photo">фотография.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Update(Photo photo)
    {
        var deleteResult = Delete(photo.Id);

        if (deleteResult.IsFailure) return ResultVoid.Failure(deleteResult.Error);

        var addResult = Add(photo);

        if (addResult.IsFailure) return ResultVoid.Failure(addResult.Error);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаление фотографии.
    /// </summary>
    /// <param name="id">идентификатор фотографии.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Delete(int id)
    {
        var result = _data
            .TryRemove(id, out var photo)
            .ToResult();

        if (result.IsFailure)
            return ResultVoid.Failure(new Error("PhotoRepository.CantDeletePhoto",
                "Не удалось удалить фото"));

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Получение фотографий по идентификатору альбома.
    /// </summary>
    /// <param name="albumId">идентификатор альбома.</param>
    /// <returns>
    ///     Возвращает фотографии.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public Result<IReadOnlyCollection<Photo>> GetByAlbumId(int albumId)
    {
        var album = _fakeAlbumRepository.GetById(albumId);
        
        if (album.IsFailure) return Result<IReadOnlyCollection<Photo>>.Failure(album.Error);

        var photos = (
            from photoId in album.Value.PhotoIds
            from photo in _data
            where photo.Value.Id == photoId
            select photo.Value
            ).ToList();

        if (photos.Count == 0) return Result<IReadOnlyCollection<Photo>>
            .Failure(new Error("PhotoRepository.PhotosByAlbumAreNotFound",
                "Не найдены фотографии по соответствующиму альбому"));

        return Result<IReadOnlyCollection<Photo>>.Success(photos.AsReadOnly());
    }

    /// <summary>
    ///     Получение фотографий по идентификаторам тегов.
    /// </summary>
    /// <param name="tagIds">идентификаторы тегов.</param>
    /// <returns>
    ///     Возвращает фотографии.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public Result<IReadOnlyCollection<Photo>> GetByTags(IEnumerable<int> tagIds)
    {
        var photos = (
            from tagId in tagIds 
            from pair in _data
            from photoTagId in pair.Value.TagIds
            where photoTagId == tagId
            select pair.Value
            ).ToList();

        if (photos.Count == 0)
            return Result<IReadOnlyCollection<Photo>>
                .Failure(new Error("PhotoRepository.PhotosByTagsAreNotFound",
                    "Не найдены фотографии по соответствующим тегам"));
        
        return Result<IReadOnlyCollection<Photo>>.Success(photos.AsReadOnly());
    }
}