using System.Collections.Concurrent;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Репозиторий тегов, имитирующий БД, и хранящий данные в оперативной памяти.
/// </summary>
public class FakeTagRepository : ITagRepository
{
    /// <summary>
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;
    
    /// <summary>
    ///     Словарь тегов.
    /// </summary>
    private readonly ConcurrentDictionary<int, Tag> _tags = new();
    
    /// <summary>
    ///     Получение тега по идентификатору.
    /// </summary>
    /// <param name="id">идентификатор тега.</param>
    /// <returns>Тег.</returns>
    public Result<Tag> GetById(int id)
    {
        foreach (var pair in _tags)
        {
            if (pair.Key == id) return pair.Value.ToResult();
        }

        return Result<Tag>.Failure(new Error("TagRepository.TagNotFound",
            "Не удалось найти тег по идентификатору"));
    }

    /// <summary>
    ///     Получение тега по имени.
    /// </summary>
    /// <param name="name">имя тега.</param>
    /// <returns>Тег.</returns>
    public Result<Tag> GetByName(string name)
    {
        foreach (var pair in _tags)
        {
            if (pair.Value.Name == name) return pair.Value.ToResult();
        }

        return Result<Tag>.Failure(new Error("TagRepository.TagNotFound",
            "Не удалось найти тег по имени"));
    }

    /// <summary>
    ///     Добавление тега.
    /// </summary>
    /// <param name="tag">тег.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Add(Tag tag)
    {
        var result = _tags
            .TryAdd(_lastId, tag)
            .ToResult();

        if (result.IsFailure) return ResultVoid.Failure(new Error("TagRepository.CantAddTag",
            "Не удалось добавить тег"));

        _lastId += 1;
        
        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаление тега.
    /// </summary>
    /// <param name="id">идентификатор тега.</param>
    /// <returns>
    ///     Возвращает тег.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public Result<Tag> Delete(int id)
    {
        var result = _tags
            .TryRemove(id, out var tag)
            .ToResult();

        if (result.IsFailure)
            return Result<Tag>.Failure(new Error("TagRepository.CantDeleteTag",
                "Не удалось удалить тег"));

        return Result<Tag>.Success(tag!);
    }
}