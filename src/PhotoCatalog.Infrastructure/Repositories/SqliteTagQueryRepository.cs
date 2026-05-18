using System;
using System.Collections.Generic;
using System.Linq;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Репозиторий для получения тегов из базы данных SQLite.
///     Методы этого репозитория гарантированно не изменяют состояние базы данных.
/// </summary>
/// <param name="connectionString">Строка подключения к базе данных.</param>
/// <param name="logger">Логгер для записи диагностических сообщений и ошибок.</param>
public class SqliteTagQueryRepository(string connectionString, ILogger logger): ITagQueryRepository
{
    /// <inheritdoc />
    public Result<Tag> GetById(int id)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        try
        {
            var tag = connection.QuerySingleOrDefault<Tag>(
                "SELECT Id, Name FROM Tags WHERE Id = @Id",
                new { Id = id });

            return tag == null
                ? Result<Tag>.Failure(DomainErrors.Tag.EmptyName)
                : Result<Tag>.Success(tag);
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при добавлении тега с {Id}.", id);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Неожиданная ошибка при добавлении тега с {Id}.", id);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <inheritdoc />
    public Result<IEnumerable<Tag>> GetAll()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        try
        {
            var tags = connection.Query<Tag>("SELECT Id, Name FROM Tags");

            IEnumerable<Tag> enumerable = tags.ToArray();
            return enumerable.Any()
                ? Result<IEnumerable<Tag>>.Success(enumerable)
                : Result<IEnumerable<Tag>>.Failure(InfrastructureErrors.Database.NotFound);
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при получении всех тегов.");
            return Result<IEnumerable<Tag>>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Неожиданная ошибка в методе при получении всех тегов.");
            return Result<IEnumerable<Tag>>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <inheritdoc />
    public Result<Tag> GetByName(string name)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        try
        {
            var tag = connection.QuerySingleOrDefault<Tag>(
                "SELECT Id, Name FROM Tags WHERE Name = @Name",
                new { Name = name });

            return tag == null
                ? Result<Tag>.Failure(DomainErrors.Tag.EmptyName)
                : Result<Tag>.Success(tag);
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при получении тега с именем {Name}.", name);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Неожиданная ошибка в методе при получении тега с именем {Name}.", name);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }
}