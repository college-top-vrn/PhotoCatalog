using System;
using System.Collections.Generic;

using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
/// Представляет основную сущность фотографии в системе, привязанную к физическому файлу.
/// </summary>
public class Photo
{
    /// <summary>
    /// Уникальный идентификатор фотографии.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Полный путь к файлу на диске.
    /// </summary>
    public string RealPath { get; private set; }

    /// <summary>
    /// контрольная-сумма файла для проверки целостности.
    /// </summary>
    public string FileHash { get; private set; }

    /// <summary>
    /// Размеры изображения (ширина/высота).
    /// </summary>
    public Dimensions Dimensions { get; private set; }

    /// <summary>
    /// Дата и время добавления фотографии в каталог (UTC).
    /// </summary>
    public DateTime AddedAt { get; private set; }

    private readonly List<int> _tagIds = new();

    /// <summary>
    /// Список идентификаторов тегов, присвоенных данной фотографии.
    /// </summary>    
    public IReadOnlyCollection<int> TagIds => _tagIds.AsReadOnly();

    /// <summary>
    /// Конструктор для инициализации через ORM (Dapper).
    /// </summary>
    private Photo() { }

    /// <summary>
    /// Восстанавливает состояние списка тегов из базы данных в обход бизнес-правил.
    /// </summary>
    /// <param name="tags">Список ID тегов.</param>
    internal ResultVoid RestoreTags(IEnumerable<int> tags)
    {
        _tagIds.Clear();
        _tagIds.AddRange(tags);
        return ResultVoid.Success();
    }

    /// <summary>
    /// Фабричный метод для создания нового экземпляра фотографии.
    /// </summary>
    /// <param name="realPath">Путь к файлу на диске.</param>
    /// <returns>Результат выполнения операции с объектом <see cref="Photo"/> или ошибкой <c>Photo.EmptyPath</c>.</returns>
    public static Result<Photo> Create(string realPath)
    {
        if (string.IsNullOrEmpty(realPath))
        {
            return Result<Photo>.Failure(DomainErrors.Photo.EmptyPath);
        }

        return Result<Photo>.Success(default); // TODO Сделать сборку фотографии зная её путь.
    }

    /// <summary>
    /// Обновляет хеш-сумму файла.
    /// </summary>
    /// <param name="newHash">Новое строковое значение хеша.</param>
    /// <returns>Результат выполнения операции.</returns>
    public ResultVoid UpdateHash(string newHash)
    {
        FileHash = newHash;
        return ResultVoid.Success();
    }

    /// <summary>
    /// Добавляет тег к фотографии.
    /// </summary>
    /// <param name="tagId">Идентификатор тега.</param>
    /// <returns>Успех или ошибка <c>Photo.DuplicateTag</c>, если тег уже добавлен.</returns>
    public ResultVoid AddTag(int tagId)
    {
        // TODO Сделать проверку на несуществующий тега

        if (_tagIds.Contains(tagId))
        {
            return ResultVoid.Failure(DomainErrors.Photo.DuplicateTag);
        }

        _tagIds.Add(tagId);
        return ResultVoid.Success();
    }

    /// <summary>
    /// Удаляет тег из коллекции фотографии.
    /// </summary>
    /// <param name="tagId">Идентификатор тега для удаления.</param>
    /// <returns>Успех или ошибка <c>Photo.TagNotExists</c>, если тег не найден.</returns>
    public ResultVoid RemoveTag(int tagId)
    {
        if (!_tagIds.Contains(tagId))
        {
            return ResultVoid.Failure(DomainErrors.Photo.TagNotExists);
        }

        _tagIds.Remove(tagId);
        return ResultVoid.Success();
    }
}