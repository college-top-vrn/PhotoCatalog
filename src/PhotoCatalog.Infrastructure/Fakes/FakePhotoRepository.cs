using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Репозиторий фотографий, имитирующий БД, и хранящий данные в оперативной памяти.
/// </summary>
public class FakePhotoRepository(IAlbumRepository fakeAlbumRepository) : IPhotoRepository
{
    /// <summary>
    ///     Словарь альбомов.
    /// </summary>
    private readonly ConcurrentDictionary<int, Photo> _photos = new();

    /// <summary>
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;

    /// <summary>
    ///     Получение фотографии по идентификатору.
    /// </summary>
    /// <param name="id">идентификатор фотографии.</param>
    /// <returns>Фотография.</returns>
    public Result<Photo> GetById(int id)
    {
        foreach (KeyValuePair<int, Photo> pair in _photos)
        {
            if (pair.Key == id)
            {
                return Result.Success(pair.Value);
            }
        }

        return Result.Failure<Photo>(new Error("PhotoRepository.PhotoNotFound",
            "Не удалось найти фото по идентификатору"));
    }

    /// <summary>
    ///     Получение фотографии по заданному пути.
    /// </summary>
    /// <param name="realPath">путь фотографии.</param>
    /// <returns>Фотография.</returns>
    public Result<Photo> GetByPath(string realPath)
    {
        foreach (KeyValuePair<int, Photo> pair in _photos)
        {
            if (pair.Value.RealPath == realPath)
            {
                return Result.Success(pair.Value);
            }
        }

        return Result.Failure<Photo>(new Error("PhotoRepository.PhotoNotFound",
            "Не удалось найти фото по заданному пути"));
    }

    /// <summary>
    ///     Добавление фотографии.
    /// </summary>
    /// <param name="photo">фотография.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Add(Photo? photo)
    {
        if (photo is null)
        {
            return ResultVoid.Failure(new Error("PhotoRepository.CantAddPhoto",
                "Не удалось добавить фото"));
        }

        _lastId += 1;

        _photos.TryAdd(_lastId, photo);

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
    public ResultVoid Update(Photo? photo)
    {
        if (photo is null)
        {
            return ResultVoid.Failure(new Error("PhotoRepository.PhotoIsNull",
                "Фото является null"));
        }

        ResultVoid deleteResult = Delete(photo.Id);

        if (deleteResult.IsFailure)
        {
            return ResultVoid.Failure(deleteResult.Error);
        }

        Add(photo, photo.Id);

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
        Result<Photo> searchResult = GetById(id);

        if (searchResult.IsFailure)
        {
            return ResultVoid.Failure(new Error("PhotoRepository.CantDeletePhoto",
                "Не удалось удалить фото"));
        }

        _photos.Remove(id, out _);

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
        Result<Album> album = fakeAlbumRepository.GetById(albumId);

        if (album.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<Photo>>(album.Error);
        }

        List<Photo> photos = (
            from photoId in album.Value!.PhotoIds
            from photo in _photos
            where photo.Value.Id == photoId
            select photo.Value
        ).ToList();

        if (photos.Count == 0)
        {
            return Result
                .Failure<IReadOnlyCollection<Photo>>(new Error("PhotoRepository.PhotosByAlbumAreNotFound",
                    "Не найдены фотографии по соответствующиму альбому"));
        }

        return Result.Success<IReadOnlyCollection<Photo>>(photos.AsReadOnly());
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
        List<Photo> photos = (
            from tagId in tagIds
            from photo in _photos.Values
            from photoTagId in photo.TagIds
            where photoTagId == tagId
            select photo
        ).ToList();

        if (photos.Count == 0)
        {
            return Result
                .Failure<IReadOnlyCollection<Photo>>(new Error("PhotoRepository.PhotosByTagsAreNotFound",
                    "Не найдены фотографии по соответствующим тегам"));
        }

        return Result.Success<IReadOnlyCollection<Photo>>(photos.AsReadOnly());
    }

    /// <summary>
    ///     Добавление фото.
    /// </summary>
    /// <param name="photo">фото.</param>
    /// <param name="id">идентификатор фото.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    private ResultVoid Add(Photo? photo, int id)
    {
        if (photo is null)
        {
            return ResultVoid.Failure(new Error("PhotoRepository.PhotoIsNull",
                "Фото является null"));
        }

        if (_photos.TryAdd(id, photo).ToResult().IsFailure)
        {
            return ResultVoid
                .Failure(new Error("PhotoRepository.PhotoWithSameIdAlreadyExist",
                    "Фото с похожим идентификатором уже существует"));
        }

        return ResultVoid.Success();
    }
}