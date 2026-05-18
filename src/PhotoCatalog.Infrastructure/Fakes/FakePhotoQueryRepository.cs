using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;
/// <summary>
///     Репозиторий для получения фотографий, имитирующий БД, и хранящий данные в оперативной памяти.
/// </summary>
public class FakePhotoQueryRepository(IAlbumRepository fakeAlbumRepository) : IPhotoQueryRepository
{
    /// <summary>
    ///     Словарь альбомов.
    /// </summary>
    private readonly ConcurrentDictionary<int, Photo> _photos = new();


    /// <inheritdoc />
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


    /// <inheritdoc />
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


    /// <inheritdoc />
    public Result<IReadOnlyCollection<Photo>> GetByAlbumId(int albumId)
    {
        Result<Album> album = fakeAlbumRepository.GetById(albumId);

        if (album.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<Photo>>(album.Error);
        }

        List<Photo> photos = (
            from photoId in album.Value.PhotoIds
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


    /// <inheritdoc />
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

    /// <inheritdoc />
    public Result<IEnumerable<Photo>> GetAll()
    {
        return Result.Success<IEnumerable<Photo>>(_photos.Values);
    }
}