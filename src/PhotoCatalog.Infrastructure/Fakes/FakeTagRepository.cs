using System.Collections.Concurrent;
using System.Collections.Generic;

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
        foreach (KeyValuePair<int, Tag> pair in _tags)
        {
            if (pair.Key == id)
            {
                return Result<Tag>.Success(pair.Value);
            }
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
        foreach (Tag pair in _tags.Values)
        {
            if (pair.Name == name)
            {
                return Result<Tag>.Success(pair);
            }
        }

        return Result<Tag>.Failure(new Error("TagRepository.TagNotFound",
            "Не удалось найти тег по идентификатору"));
    }

    /// <summary>
    ///     Добавление тега.
    /// </summary>
    /// <param name="tag">тег.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Add(Tag? tag)
    {
        if (tag is null)
        {
            return ResultVoid.Failure(new Error("TagRepository.CantAddTag",
                "Не удалось добавить тег"));
        }

        _lastId += 1;

        _tags.TryAdd(_lastId, tag);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Добавление папки.
    /// </summary>
    /// <param name="tag">папка.</param>
    /// <param name="id">идентификатор папки.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Add(Tag? tag, int id)
    {
        if (tag is null)
        {
            return ResultVoid.Failure(new Error("TagRepository.TagIsNull",
                "Тег является null"));
        }

        if (_tags.TryAdd(id, tag).ToResult().IsFailure)
        {
            return ResultVoid
                .Failure(new Error("TagRepository.TagWithSameIdAlreadyExist",
                    "Тег с похожим идентификатором уже существует"));
        }

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
        Result<Tag> searchResult = GetById(id);

        if (searchResult.IsFailure)
        {
            return Result<Tag>.Failure(new Error("FolderRepository.CantDeleteFolder",
                "Не удалось удалить папку"));
        }

        _tags.Remove(id, out Tag tag);

        return Result<Tag>.Success(tag!);
    }
}