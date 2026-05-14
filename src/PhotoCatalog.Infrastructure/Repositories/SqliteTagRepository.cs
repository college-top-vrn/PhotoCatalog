using System;

using Dapper;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

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
    private readonly ILogger<SqliteAlbumRepository> _logger;

    /// <summary>
    /// Создает репозиторий с указанной строкой подключения к SQLite.
    /// </summary>
    /// <param name="connectionString">
    /// Строка подключения вида "Data Source=photo.db".
    /// </param>
    /// <param name="logger">Логгер для записи диагностических сообщений и ошибок.</param>
    public SqliteTagRepository(string connectionString, ILogger<SqliteAlbumRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Получить тег по уникальному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор тега (PRIMARY KEY).</param>
    /// <returns>
    /// При успехе возвращает объект <see cref="Result{Tag}"/> или ошибка, если не найден.
    /// </returns>
    public Result<Tag> GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        try
        {
            var teg = connection.QuerySingleOrDefault<Tag>(
                "SELECT * FROM Tags WHERE Id = @Id",
                new { Id = id });


            if (teg == null)
            {
                return Result<Tag>.Failure(DomainErrors.Tag.EmptyName);
            }

            return Result<Tag>.Success(teg);
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Ошибка SQLite в методе GetById для тега {Id}", id);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка в методе GetById для тега {Id}", id);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    /// Получает тег по имени из SQLite.
    /// </summary>
    /// <param name="name">Имя тега.</param>
    /// <returns>
    /// При успехе возвращает объект <see cref="Result{Tag}"/> или ошибка, если не найден.
    /// </returns>
    public Result<Tag> GetByName(string name)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        try
        {
            var teg = connection.QuerySingleOrDefault<Tag>(
                "SELECT * FROM Tags WHERE Name = @Name",
                new { Name = name });


            if (teg == null)
            {
                return Result<Tag>.Failure(DomainErrors.Tag.EmptyName);
            }

            return Result<Tag>.Success(teg);
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Ошибка SQLite в методе GetByName для тега {Name}", name);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка в методе GetByName для тега {Name}", name);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }


    public ResultVoid Add(Tag tag)
    {
        throw new NotImplementedException();
    }

    public ResultVoid Delete(int id)
    {
        throw new NotImplementedException();
    }
}