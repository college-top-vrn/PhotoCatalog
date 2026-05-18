using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Интерфейс для изменения тегов в базе данных.
///     Методы этого интерфейса выполняют SQL-команды в базу данных,
///     но не фиксируют транзакцию самостоятельно.
/// </summary>
public interface ITagCommandRepository
{
    /// <summary>
    ///     Добавляет новый тег в систему.
    /// </summary>
    /// <param name="tag">Тег для добавления.</param>
    /// <returns>Результат добавления тега.</returns>
    ResultVoid Add(Tag tag);

    /// <summary>
    ///     Обновляет существующий тег в системе.
    /// </summary>
    /// <param name="tag">Обновленный тег.</param>
    /// <returns>Результат обновления тега.</returns>
    ResultVoid Update(Tag tag);

    /// <summary>
    ///     Удаляет тег из системы по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор тега.</param>
    /// <returns>Результат удаления тега.</returns>
    ResultVoid Delete(int id);
}