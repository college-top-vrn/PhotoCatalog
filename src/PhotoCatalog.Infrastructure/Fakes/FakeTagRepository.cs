using System.Collections.Generic;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

public class FakeTagRepository : ITagRepository
{
    private readonly Dictionary<int, Tag> _storage = [];

    /// <inheritdoc />
    public Result<Tag> GetById(int id)
    {
        if (_storage.TryGetValue(id, out var tag))
        {
            return Result<Tag>.Success(tag.DeepCopy());
        }

        return Result<Tag>.Failure(new Error("FakerTag.NotFound", "Тег с таким id отсутствует"));
    }

    /// <inheritdoc />
    public Result<Tag> GetByName(string name)
    {
        foreach (var tag in _storage)
        {
            if (tag.Value.Name == name)
            {
                var copy = tag.Value.DeepCopy();
                return Result<Tag>.Success(copy);
            }
        }

        return Result<Tag>.Failure(new Error("FakeTag.NameNotFound", "Не найдено"));
    }

    /// <inheritdoc />
    public ResultVoid Add(Tag tag)
    {
        var tagCopy = tag.DeepCopy();
        if (_storage.TryAdd(tag.Id, tagCopy))
        {
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeTag.FailedToAdd", "Не удалось добавить тег"));
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        if (_storage.Remove(id))
        {
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeFolder.FailedToRemove", "Не удалось удалить"));
    }
}