using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Репозиторий для записи фотографий, имитирующий БД, и хранящий данные в оперативной памяти.
/// </summary>
public class FakePhotoCommandRepository : IPhotoCommandRepository
{
    /// <summary>
    ///     Словарь альбомов.
    /// </summary>
    private readonly ConcurrentDictionary<int, Photo> _photos = new();

    /// <summary>
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;

    /// <summary>
    ///     Добавление фотографии.
    /// </summary>
    /// <param name="photo">фотография.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Add(Photo? photo)
    {
        if (photo is null)
        {
            return ResultVoid.Failure(new Error("PhotoRepository.CantAddPhoto",
                "Не удалось добавить фото"));
        }

        _lastId += 1;

        _photos.TryAdd(_lastId, photo);

        return ResultVoid.Success();
    }


    /// <inheritdoc />
    public ResultVoid Update(Photo? photo)
    {
        if (photo is null)
        {
            return ResultVoid.Failure(new Error("PhotoRepository.PhotoIsNull",
                "Фото является null"));
        }

        ResultVoid deleteResult = Delete(photo.Id);

        if (deleteResult.IsFailure)
        {
            return ResultVoid.Failure(deleteResult.Error);
        }

        Add(photo, photo.Id);

        return ResultVoid.Success();
    }


    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        if (_photos.ContainsKey(id))
        {
            return ResultVoid.Failure(new Error("PhotoRepository.CantDeletePhoto",
                "Не удалось удалить фото"));
        }

        _photos.Remove(id, out _);

        return ResultVoid.Success();
    }

    /// <summary>
    /// Добавление фото с альбомом.
    /// </summary>
    /// <param name="photo">фотография.</param>
    /// <param name="id">идентификатор альбома</param>
    /// <returns></returns>
    private ResultVoid Add(Photo? photo, int id)
    {
        if (photo is null)
        {
            return ResultVoid.Failure(new Error("PhotoRepository.PhotoIsNull",
                "Фото является null"));
        }

        if (_photos.TryAdd(id, photo).ToResult().IsFailure)
        {
            return ResultVoid
                .Failure(new Error("PhotoRepository.PhotoWithSameIdAlreadyExist",
                    "Фото с похожим идентификатором уже существует"));
        }

        return ResultVoid.Success();
    }
}