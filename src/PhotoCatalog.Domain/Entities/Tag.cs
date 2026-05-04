using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
/// Представляет тег для фотографий.
/// </summary>
public class Tag
{
    /// <summary>Уникальный идентификатор тега.</summary>
    public int Id { get; private set; }

    /// <summary>Нормализованное имя тега (в нижнем регистре).</summary>
    public string Name { get; private set; }

    /// <summary>Конструктор для Dapper.</summary>
    private Tag() { }

    /// <summary>Приватный конструктор с нормализованным именем.</summary>
    /// <param name="name">Нормализованное имя тега.</param>
    private Tag(string name)
    {
        Name = name;
    }

    /// <summary>Создает новый валидный тег.</summary>
    /// <param name="name">Имя тега (будет нормализовано).</param>
    /// <returns>Результат создания с тегом или ошибкой.</returns>
    public static Result<Tag> Create(string name)
    {
        if (string.IsNullOrEmpty(name))
            return DomainErrors.Tag.EmptyName;

        var trimmedName = name.Trim();

        if (trimmedName.Length > 50)
            return DomainErrors.Tag.TooLong;

        var normalizedName = trimmedName.ToLowerInvariant();

        return new Tag(normalizedName);
    }
}