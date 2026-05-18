using System;
using System.Collections.Generic;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Репозиторий для изменения тегов в базе данных SQLite.
///     Методы этого репозитория выполняют SQL-команды в базу данных,
///     но не фиксируют транзакцию самостоятельно.
/// </summary>
/// <param name="connectionString">Строка подключения к базе данных.</param>
/// <param name="logger">Логгер для записи диагностических сообщений и ошибок.</param>
public class SqliteTagCommandRepository(string connectionString, ILogger logger) : ITagCommandRepository
{
    /// <inheritdoc />
    public ResultVoid Add(Tag tag)
    {
        const int sqliteConstraintErrorCode = 19;
        try
        {
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            const string sql = """
                               INSERT INTO Tags(Id, Name)
                               VALUES (@Id, @Name)
                               """;

            int affectedRows = connection.Execute(sql, new { tag.Id, tag.Name });

            if (affectedRows == 0)
            {
                logger.Warning("Не удалось добавить новый тег с Id = {Id}.", tag.Id);
                return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
            }

            logger.Information("Тег с Id = {Id} успешно добавлен.", tag.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == sqliteConstraintErrorCode)
        {
            return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при добавлении тега с Id = {Id}.", tag.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Неожиданная ошибка при добавлении тега с Id = {Id}.", tag.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <inheritdoc />
    public ResultVoid Update(Tag tag)
    {
        try
        {
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            const string sql = """
                               UPDATE Tags
                               SET Name = @Name
                               WHERE Id = @Id
                               """;

            int affectedRows = connection.Execute(sql, new { tag.Id, tag.Name });

            if (affectedRows == 0)
            {
                logger.Warning("Не удалось обновить несуществующий тег с Id = {Id}.", tag.Id);
                return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
            }

            logger.Information("Тег с Id = {Id} успешно обновлен", tag.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при обновлении тега с Id = {Id}.", tag.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Неожиданная ошибка при обновлении тега с Id = {Id}.", tag.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        try
        {
            using SqliteConnection connection = new(connectionString);
            connection.Open();

            const string sql = "DELETE FROM Tags WHERE Id = @Id";
            int affectedRows = connection.Execute(sql, new { Id = id });

            if (affectedRows == 0)
            {
                logger.Warning("Попытка удалить несуществующий тег с Id = {Id}", id);
                return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
            }

            logger.Information("Тег с Id = {Id} успешно удалена", id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при удалении тега с Id = {Id}.", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Неожиданная при удалении тега с Id = {Id}.", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }
}