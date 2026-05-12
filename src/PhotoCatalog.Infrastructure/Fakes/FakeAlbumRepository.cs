using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Фальшивый репозиторий альбомов для врменной работы с frontend
/// </summary>
public class FakeAlbumRepository : IAlbumRepository
{
    private readonly Dictionary<int, Album> _storage = [];

    /// <inheritdoc />
    public Result<Album> GetById(int id)
    {
        if (_storage.TryGetValue(id, out var album))
        {
            return Result<Album>.Success(album.DeepCopy());
        }

        return Result<Album>.Failure(new Error("FakeAlbum.NotFound", "Альбом не найден"));
    }


    /// <inheritdoc />
    public ResultVoid Add(Album album)
    {
        var albumCopy = album.DeepCopy();
        if (_storage.TryAdd(album.Id, albumCopy))
        {
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeAlbum.FailedToAdd", "Не удалось добавить"));
    }

    /// <inheritdoc />
    public ResultVoid Update(Album album)
    {
        if (_storage.ContainsKey(album.Id))
        {
            var albumCopy = album.DeepCopy();
            _storage[album.Id] = albumCopy;

            return ResultVoid.Success(); 
        }

        return ResultVoid.Failure(new Error("FakeAlbum.FailedUpdate", "Не удалось обновить"));
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        if (_storage.Remove(id))
        {
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeAlbum.FailedRemove", "Не удалось удалить"));
    }
}