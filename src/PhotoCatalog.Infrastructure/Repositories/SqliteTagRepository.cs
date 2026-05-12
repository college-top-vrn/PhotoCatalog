using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория тегов с использованием SQLite и Dapper.
/// Работает с таблицей Tags (Id, Name).
/// </summary>
/// <remarks>
/// Все операции используют Result-паттерн для обработки ошибок.
/// Имена тегов уникальны и нормализованы к нижнему регистру.
/// </remarks>
public class SqliteTagRepository : ITagRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Создает репозиторий с указанной строкой подключения к SQLite.
    /// </summary>
    /// <param name="connectionString">
    /// Строка подключения вида "Data Source=photo.db".
    /// </param>
    public SqliteTagRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Получить тег по уникальному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор тега (PRIMARY KEY).</param>
    /// <returns>
    /// Успех с объектом <see cref="Tag"/> или ошибка, если не найден.
    /// </returns>
    public Result<Tag> GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var teg = connection.QuerySingleOrDefault<Tag>(
            "SELECT * FROM Tags WHERE Id = @Id",
            new { Id = id });
        if (teg == null)
            return Result<Tag>.Failure(DomainErrors.Tag.EmptyName);

        return Result<Tag>.Success(teg);
    }

    public Result<Tag> GetByName(string name)
    {
        throw new NotImplementedException();
    }

    public ResultVoid Add(Tag tag)
    {
        throw new NotImplementedException();
    }

    public Result<Tag> Delete(int id)
    {
        throw new NotImplementedException();
    }
}