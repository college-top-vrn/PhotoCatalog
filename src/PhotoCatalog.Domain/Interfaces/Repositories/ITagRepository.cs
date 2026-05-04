using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для работы с тегами.
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Получает тег по ID.
    /// </summary>
    /// <param name="id">ID тега</param>
    /// <returns>Тег или ошибка</returns>
    Result<Tag> GetById(int id);

    /// <summary>
    /// Получает тег по имени.
    /// </summary>
    /// <param name="name">Имя тега</param>
    /// <returns>Тег или ошибка</returns>
    Result<Tag> GetByName(string name);

    /// <summary>
    /// Добавляет тег.
    /// </summary>
    /// <param name="tag">Тег для добавления</param>
    /// <returns>Результат операции</returns>
    ResultVoid Add(Tag tag);

    /// <summary>
    /// Удаляет тег по ID.
    /// </summary>
    /// <param name="id">ID тега</param>
    /// <returns>Удаленный тег или ошибка</returns>
    Result<Tag> Delete(int id);
}