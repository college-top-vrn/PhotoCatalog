using System;
using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Фальшивый репозиторий фото для врменной работы с frontend
/// </summary>
public class FakePhotoRepository : IPhotoRepository
{
    private readonly Dictionary<int, Photo> _photoStorage = [];
    private readonly Dictionary<int, Album> _albumStorage = [];

    /// <inheritdoc />
    public Result<Photo> GetById(int id)
    {
        if (_photoStorage.TryGetValue(id, out var photo))
        {
            var photoCopy = photo.DeepCopy();
            return Result<Photo>.Success(photoCopy);
        }

        return Result<Photo>.Failure(new Error("FakePhoto.NotFound", "Фото не найдена"));
    }

    /// <inheritdoc />
    public Result<Photo> GetByPath(string realPath)
    {
        foreach (var photo in _photoStorage)
        {
            if (photo.Value.RealPath == realPath)
            {
                var photoCopy = photo.Value.DeepCopy();

                return Result<Photo>.Success(photoCopy);
            }
        }

        return Result<Photo>.Failure(new Error("FakePhoto.NotFound", "Фото по такому пути не найдено"));
    }

    /// <inheritdoc />
    public ResultVoid Add(Photo photo)
    {
        if (_photoStorage.TryAdd(photo.Id, photo.DeepCopy()))
        {
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakePhoto", "Не удалось добавить фото"));
    }

    /// <inheritdoc />
    public ResultVoid Update(Photo photo)
    {
        if (_photoStorage.ContainsKey(photo.Id))
        {
            var photoCopy = photo.DeepCopy();
            _photoStorage[photo.Id] = photoCopy;
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakePhoto.FailedUpdate", "Не удалось обновить"));
    }

    public ResultVoid Delete(int id)
    {
        if (_photoStorage.Remove(id))
        {
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakePhoto", "Не удалось удалить фото"));
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<Photo>> GetByAlbumId(int albumId)
    {
        if (!_albumStorage.TryGetValue(albumId, out var album))
        {
            return Result<IReadOnlyCollection<Photo>>.Failure(new Error("FakeAlbum.NotFound", "Альбом не найден"));
        }

        var photoIds = album.PhotoIds.ToHashSet();

        IReadOnlyCollection<Photo> result = _photoStorage.Values
            .Join(album.PhotoIds, photo => photo.Id, id => id, (photo, id) => photo)
            .Select(p => p.DeepCopy())
            .ToList();

        return Result<IReadOnlyCollection<Photo>>.Success(result);
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<Photo>> GetByTags(IEnumerable<int> tagIds)
    {
        // TODO: Доделать мето получения колекции фото по тегу

        throw new NotImplementedException(); 
    }
}