using System;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

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
    private readonly ILogger _logger;

    /// <summary>
    /// Создает репозиторий с указанной строкой подключения к SQLite.
    /// </summary>
    /// <param name="connectionString">
    /// Строка подключения вида "Data Source=photo.db".
    /// </param>
    /// <param name="logger">Логгер для записи диагностических сообщений и ошибок.</param>
    public SqliteTagRepository(string connectionString, ILogger logger)
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
            _logger.Error(ex, "Ошибка SQLite в методе GetById для тега {Id}", id);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetById для тега {Id}", id);
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
            _logger.Error(ex, "Ошибка SQLite в методе GetByName для тега {Name}", name);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе GetByName для тега {Name}", name);
            return Result<Tag>.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }




    /// <inheritdoc />
    public ResultVoid Add(Tag teg)
    {
        try
        {
            using SqliteConnection connection = new(_connectionString);
            connection.Open();
            const string sql = """
                               UPDATE Folders
                               SET ParentFolderId = @ParentFolderId,
                                   Name = @Name
                               WHERE Id = @Id
                               """;

            int affectedRows = connection.Execute(sql, new { teg.Id, teg.Name });

            if (affectedRows == 0)
            {
                _logger.Warning("Не удалось обновить несуществующий тег с Id = {Id}", teg.Id);
                return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
            }

            _logger.Information("Тег с Id = {Id} успешно обновлен", teg.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite при обновлении тега с Id = {Id}", teg.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        const int sqliteConstraintErrorCode = 19;
        try
        {
            using SqliteConnection connection = new(_connectionString);
            connection.Open();

            const string sql = "DELETE FROM Folders WHERE Id = @Id";
            int affectedRows = connection.Execute(sql, new { Id = id });

            if (affectedRows == 0)
            {
                _logger.Warning("Попытка удалить несуществующий тег с Id = {FolderId}", id);
                return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
            }

            _logger.Information("Тег с Id = {Id} успешно удалена", id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == sqliteConstraintErrorCode)
        {
            _logger.Warning(ex, "Невозможно удалить тег с Id = {Id}: имеются дочерние объекты", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.HasChildren);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite при удалении тега с Id = {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }
}