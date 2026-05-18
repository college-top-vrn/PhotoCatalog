using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет тег для фотографий.
/// </summary>
public class Tag
{
    /// <summary>
    ///     Конструктор для Dapper.
    /// </summary>
    private Tag() { }

    /// <summary>
    ///     Приватный конструктор с нормализованным именем.
    /// </summary>
    /// <param name="name">
    ///     Нормализованное имя тега.
    /// </param>
    private Tag(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Уникальный идентификатор тега.
    /// </summary>
    public int Id { get; private init; }

    /// <summary>
    ///     Нормализованное имя тега (в нижнем регистре).
    /// </summary>
    public string Name { get; private init; } = null!;

    /// <summary>
    ///     Создает новый валидный тег.
    /// </summary>
    /// <param name="name">
    ///     Имя тега (будет нормализовано).
    /// </param>
    /// <returns>
    ///     Результат создания с тегом или ошибкой.
    /// </returns>
    public static Result<Tag> Create(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Result.Failure<Tag>(DomainErrors.Tag.EmptyName);
        }

        string trimmedName = name.Trim();

        if (trimmedName.Length > 50)
        {
            return Result.Failure<Tag>(DomainErrors.Tag.TooLong);
        }

        string normalizedName = trimmedName.ToLowerInvariant();

        return Result.Success(new Tag(normalizedName));
    }


    /// <summary>
    ///     Метод для глубокого копирования
    /// </summary>
    /// <returns> возвращает копию объекта <see cref="Tag" /> </returns>
    public Tag DeepCopy()
    {
        return new Tag { Id = Id, Name = Name };
    }
}