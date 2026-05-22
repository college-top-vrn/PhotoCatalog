using System.Collections.Concurrent;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Fake-репозиторий для операций записи альбомов, имитирующий БД в оперативной памяти.
/// </summary>
/// <remarks>
///     <para>
///         Используется для модульного тестирования и разработки без реальной базы данных.
///         Данные хранятся в ConcurrentDictionary и не сохраняются между запусками.
///     </para>
///     <para>
///         <b>Важно:</b> Для имитации транзакционной целостности, методы не фиксируют изменения
///         автоматически — это должно делать приложение через IUnitOfWork (в реальном сценарии).
///         В fake-реализации изменения применяются немедленно.
///     </para>
/// </remarks>
public class FakeAlbumCommandRepository : IAlbumCommandRepository
{
    private readonly ConcurrentDictionary<int, Album> _albums;
    private int _lastId;

    /// <summary>
    ///     Инициализирует новый экземпляр fake-репозитория для записи.
    /// </summary>
    /// <param name="albums">Словарь альбомов (может быть передан для предзаполнения).</param>
    /// <param name="initialId">Начальное значение для генерации идентификаторов.</param>
    public FakeAlbumCommandRepository(ConcurrentDictionary<int, Album>? albums = null, int initialId = 0)
    {
        _albums = albums ?? new ConcurrentDictionary<int, Album>();
        _lastId = initialId;
    }

    /// <inheritdoc />
    public ResultVoid Add(Album album)
    {
        int newId = ++_lastId;

        // Создаем копию, чтобы сохранить состояние на момент добавления
        Album albumToAdd = album.DeepCopy();

        // Устанавливаем Id (если альбом создавался без Id)
        if (albumToAdd.Id == 0)
        {
            typeof(Album).GetProperty("Id")?.SetValue(albumToAdd, newId);
        }

        return !_albums.TryAdd(albumToAdd.Id, albumToAdd)
            ? ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation)
            : ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Update(Album album)
    {
        if (!_albums.ContainsKey(album.Id))
        {
            return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
        }

        // Обновляем через DeepCopy, чтобы не было ссылочных проблем
        _albums[album.Id] = album.DeepCopy();

        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        return !_albums.TryRemove(id, out _)
            ? ResultVoid.Failure(InfrastructureErrors.Database.NotFound)
            : ResultVoid.Success();
    }
}