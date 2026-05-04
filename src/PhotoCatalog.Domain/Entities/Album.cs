using System.Collections.Generic;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
/// Представляет доменную сущность виртуального альбома.
/// </summary>
/// <remarks>
/// Альбом отвечает за строгий контроль своего содержимого.
/// Альбом проверяет, чтобы одну и ту же фотографию нельзя было прикрепить дважды.
/// </remarks>
public class Album
{
    /// <summary>
    /// Получает уникальный идентификатор альбома.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Получает наименование альбома.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Получает идентификатор папки, в которой расположен альбом.
    /// </summary>
    /// <value>Идентификатор папки или null, если альбом не перемещен в папку.</value>
    public int? FolderId { get; private set; }

    private readonly List<int> _photoIds = new();

    /// <summary>
    /// Получает коллекцию идентификаторов фотографий, принадлежащих альбому, доступную только для чтения.
    /// </summary>
    public IReadOnlyCollection<int> PhotoIds => _photoIds.AsReadOnly();

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="Album"/> без параметров.
    /// </summary>
    /// <remarks>
    /// Конструктор является приватным и используется исключительно для материализации объектов библиотекой Dapper.
    /// </remarks>
    private Album() { }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="Album"/> с указанным наименованием.
    /// </summary>
    /// <param name="name">Наименование альбома.</param>
    private Album(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Восстанавливает коллекцию идентификаторов фотографий при материализации объекта из базы данных.
    /// </summary>
    /// <param name="photoIds">Коллекция идентификаторов фотографий для восстановления.</param>
    /// <returns>Успешный результат выполнения операции.</returns>
    /// <remarks>
    /// Метод имеет модификатор доступа internal.
    /// Метод используется только библиотекой Dapper.
    /// </remarks>
    internal ResultVoid RestorePhotos(IEnumerable<int> photoIds)
    {
        _photoIds.Clear();
        _photoIds.AddRange(photoIds);
        return ResultVoid.Success();
    }

    /// <summary>
    /// Создает новый экземпляр альбома с проверкой валидности наименования.
    /// </summary>
    /// <param name="name">Наименование создаваемого альбома.</param>
    /// <returns>
    /// Результат операции:
    /// <list type="bullet">
    /// <item><description>Успех с созданным альбомом;</description></item>
    /// <item><description>Ошибка <see cref="DomainErrors.Album.EmptyName"/>, если наименование пустое.</description></item>
    /// </list>
    /// </returns>
    public static Result<Album> Create(string name)
    {
        if (string.IsNullOrEmpty(name))
            return Result<Album>.Failure(DomainErrors.Album.EmptyName);

        var trimmedName = name.Trim();

        return Result<Album>.Success(new Album(trimmedName));
    }

    /// <summary>
    /// Изменяет наименование альбома на новое значение.
    /// </summary>
    /// <param name="newName">Новое наименование альбома.</param>
    /// <returns>
    /// Результат операции:
    /// <list type="bullet">
    /// <item><description>Успех при успешном переименовании;</description></item>
    /// <item><description>Ошибка <see cref="DomainErrors.Album.EmptyName"/>, если новое наименование пустое.</description></item>
    /// </list>
    /// </returns>
    public ResultVoid Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return ResultVoid.Failure(DomainErrors.Album.EmptyName);

        Name = newName.Trim();
        return ResultVoid.Success();
    }

    /// <summary>
    /// Перемещает альбом в указанную папку.
    /// </summary>
    /// <param name="folder">Папка назначения.</param>
    /// <returns>Успешный результат выполнения операции.</returns>
    public ResultVoid MoveToFolder(Folder folder)
    {
        FolderId = folder.Id;
        return ResultVoid.Success();
    }

    /// <summary>
    /// Добавляет фотографию в альбом.
    /// </summary>
    /// <param name="photoId">Идентификатор добавляемой фотографии.</param>
    /// <returns>
    /// Результат операции:
    /// <list type="bullet">
    /// <item><description>Успех при успешном добавлении фотографии;</description></item>
    /// <item><description>Ошибка <see cref="DomainErrors.Album.DuplicatePhoto"/>, если фотография уже есть в альбоме.</description></item>
    /// </list>
    /// </returns>
    public ResultVoid AddPhoto(int photoId)
    {
        if (_photoIds.Contains(photoId))
            return ResultVoid.Failure(DomainErrors.Album.DuplicatePhoto);

        _photoIds.Add(photoId);
        return ResultVoid.Success();
    }

    /// <summary>
    /// Удаляет фотографию из альбома.
    /// </summary>
    /// <param name="photoId">Идентификатор удаляемой фотографии.</param>
    /// <returns>Успешный результат выполнения операции.</returns>
    public ResultVoid RemovePhoto(int photoId)
    {
        _photoIds.Remove(photoId);
        return ResultVoid.Success();
    }
}