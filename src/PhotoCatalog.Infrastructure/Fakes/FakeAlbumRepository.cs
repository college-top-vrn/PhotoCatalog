using System.Collections.Concurrent;

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
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;
    
    /// <summary>
    ///     Словарь альбомов.
    /// </summary>
    private readonly ConcurrentDictionary<int, Album> _data = new();
    
    /// <summary>
    ///     Получение альбома по идентификатору.
    /// </summary>
    /// <param name="id">идентификатор альбома.</param>
    /// <returns>Альбом.</returns>
    public Result<Album> GetById(int id)
    {
        foreach (var pair in _data)
        {
            if (pair.Key == id) return pair.Value.ToResult();
        }

        return Result<Album>.Failure(new Error("AlbumRepository.AlbumNotFound",
            "Не удалось найти альбом по идентификатору"));
    }

    /// <summary>
    ///     Добавление альбома.
    /// </summary>
    /// <param name="folder">альбом.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Add(Album album)
    {
        var result = _data
            .TryAdd(_lastId, album)
            .ToResult();

        if (result.IsFailure) return ResultVoid.Failure(new Error("AlbumRepository.CantAddAlbum",
            "Не удалось добавить альбом"));

        _lastId += 1;
        
        return ResultVoid.Success();
    }

    /// <summary>
    ///     Обновление альбома.
    /// </summary>
    /// <param name="album">альбом.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Update(Album album)
    {
        var deleteResult = Delete(album.Id);
        
        if (deleteResult.IsFailure) return ResultVoid.Failure(deleteResult.Error);
        
        var addResult = Add(album);

        if (addResult.IsFailure) return ResultVoid.Failure(addResult.Error);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаление альбома.
    /// </summary>
    /// <param name="id">идентификатор альбома.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Delete(int id)
    {
        var result = _data
            .TryRemove(id, out var album)
            .ToResult();

        if (result.IsFailure)
            return ResultVoid.Failure(new Error("AlbumRepository.CantDeleteAlbum",
                "Не удалось удалить альбом"));

        return ResultVoid.Success();
    }
}