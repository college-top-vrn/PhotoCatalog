using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Fake-репозиторий для операций чтения альбомов, имитирующий БД в оперативной памяти.
/// </summary>
/// <remarks>
///     Используется для модульного тестирования и разработки без реальной базы данных.
///     Данные хранятся в ConcurrentDictionary и не сохраняются между запусками.
/// </remarks>
public class FakeAlbumQueryRepository : IAlbumQueryRepository
{
    private readonly ConcurrentDictionary<int, Album> _albums;

    /// <summary>
    ///     Инициализирует новый экземпляр fake-репозитория для чтения.
    /// </summary>
    /// <param name="albums">Словарь альбомов (может быть передан для предзаполнения).</param>
    public FakeAlbumQueryRepository(ConcurrentDictionary<int, Album>? albums = null)
    {
        _albums = albums ?? new ConcurrentDictionary<int, Album>();
    }

    /// <inheritdoc />
    public Result<Album> GetById(int id)
    {
        if (_albums.TryGetValue(id, out Album? album))
        {
            // Возвращаем глубокую копию, чтобы избежать модификации оригинала
            return Result<Album>.Success(album.DeepCopy());
        }

        return Result<Album>.Failure(InfrastructureErrors.Database.NotFound);
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<Album>> GetByFolderId(int folderId)
    {
        List<Album> albums = _albums.Values
            .Where(album => album.FolderId == folderId)
            .Select(album => album.DeepCopy())
            .ToList();

        return Result<IReadOnlyCollection<Album>>.Success(albums.AsReadOnly());
    }
}