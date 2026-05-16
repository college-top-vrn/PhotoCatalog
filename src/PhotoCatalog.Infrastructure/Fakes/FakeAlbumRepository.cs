using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Репозиторий альбомов, имитирующий БД, и хранящий данные в оперативной памяти.
/// </summary>
public class FakeAlbumRepository : IAlbumRepository
{
    /// <summary>
    ///     Словарь альбомов.
    /// </summary>
    private readonly ConcurrentDictionary<int, Album> _albums = new();

    /// <summary>
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;


    /// <inheritdoc />
    public Result<Album> GetById(int id)
    {
        foreach (KeyValuePair<int, Album> pair in _albums)
        {
            if (pair.Key == id)
            {
                return Result<Album>.Success(pair.Value);
            }
        }

        return Result<Album>.Failure(new Error("AlbumRepository.AlbumNotFound",
            "Не удалось найти альбом по идентификатору"));
    }
    
    /// <inheritdoc />
    public ResultVoid Add(Album? album)
    {
        if (album is null)
        {
            return ResultVoid.Failure(new Error("AlbumRepository.CantAddAlbum",
                "Не удалось добавить альбом"));
        }

        _lastId += 1;

        _albums.TryAdd(_lastId, album);

        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Update(Album? album)
    {
        if (album is null)
        {
            return ResultVoid.Failure(new Error("AlbumRepository.AlbumIsNull",
                "Альбом является null"));
        }

        ResultVoid deleteResult = Delete(album.Id);

        if (deleteResult.IsFailure)
        {
            return ResultVoid.Failure(deleteResult.Error);
        }

        Add(album, album.Id);

        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        Result<Album> searchResult = GetById(id);

        if (searchResult.IsFailure)
        {
            return ResultVoid.Failure(new Error("AlbumRepository.CantDeleteAlbum",
                "Не удалось удалить альбом"));
        }

        _albums.Remove(id, out _);

        return ResultVoid.Success();
    }


    /// <inheritdoc />
    public Result<IReadOnlyCollection<Album>> GetByFolderId(int id)
    {
        List<Album> albums = _albums.Values.Where(album => album.FolderId == id).ToList();

        return Result<IReadOnlyCollection<Album>>.Success(albums);
    }
    
    /// <inheritdoc />
    public ResultVoid AddPhoto(int albumId, int photoId)
    {
        Result<Album> searchResult = GetById(albumId);

        if (searchResult.IsFailure)
        {
            return ResultVoid.Failure(searchResult.Error);
        }

        foreach (KeyValuePair<int, Album> pair in _albums)
        {
            if (pair.Key == albumId)
            {
                pair.Value.AddPhoto(photoId);
            }
        }

        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid DeletePhoto(int albumId, int photoId)
    {
        Result<Album> searchResult = GetById(albumId);

        if (searchResult.IsFailure)
        {
            return ResultVoid.Failure(searchResult.Error);
        }

        foreach (KeyValuePair<int, Album> pair in _albums)
        {
            if (pair.Key == albumId)
            {
                pair.Value.RemovePhoto(photoId);
            }
        }

        return ResultVoid.Success();
    }


    /// <inheritdoc />
    public ResultVoid Add(Album? album, int id)
    {
        if (album is null)
        {
            return ResultVoid.Failure(new Error("AlbumRepository.AlbumIsNull",
                "Альбом является null"));
        }

        if (_albums.TryAdd(id, album).ToResult().IsFailure)
        {
            return ResultVoid
                .Failure(new Error("AlbumRepository.AlbumWithSameIdAlreadyExist",
                    "Альбом с похожим идентификатором уже существует"));
        }

        return ResultVoid.Success();
    }
}