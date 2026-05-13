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
///     Инициализирует экземпляр репозитория
///     с переданным подключением к SQLite и логгером.
///     В строке подключения должен быть включена настройка
///     <c>Foreign Keys=True</c>.
/// </summary>
/// <param name="connectionString">Строка для открытия соединения с базой данных SQLite.</param>
/// <param name="logger">Логгер для записи диагностических сообщений и ошибок.</param>
public class SqliteFolderRepository(string connectionString, ILogger logger) : IFolderRepository
{
    /// <inheritdoc />
    public Result<Folder> GetById(int id)
    {
        try
        {
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            const string sql = "SELECT Id, ParentFolderId, Name FROM Folders WHERE Id = @Id";
            Folder? folder = connection.QueryFirstOrDefault<Folder>(sql, new { Id = id });

            return folder is null
                ? Result<Folder>.Failure(InfrastructureErrors.Database.NotFound)
                : Result<Folder>.Success(folder);
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при получении папки с Id = {FolderId}.", id);
            return Result<Folder>.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }

    /// <inheritdoc />
    public Result<IEnumerable<Folder>> GetAllFolders()
    {
        try
        {
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            const string sql = "SELECT Id, ParentFolderId, Name FROM Folders";
            IEnumerable<Folder> folders = connection.Query<Folder>(sql);
            IEnumerable<Folder> enumerableFolders = folders.ToList();

            return enumerableFolders.Any()
                ? Result<IEnumerable<Folder>>.Failure(InfrastructureErrors.Database.NotFound)
                : Result<IEnumerable<Folder>>.Success(enumerableFolders);
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при получении всех папок.");
            return Result<IEnumerable<Folder>>.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }

    /// <inheritdoc />
    public ResultVoid Add(Folder folder)
    {
        try
        {
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            const string sql = """
                               INSERT INTO Folders (Id, ParentFolderId, Name)
                               VALUES (@Id, @ParentFolderId, @Name)
                               """;
            connection.Execute(sql, new { folder.Id, folder.ParentFolderId, folder.Name });

            logger.Information("Папка с Id = {FolderId} успешно добавлена", folder.Id);

            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при добавлении папки с Id = {FolderId}", folder.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }

    /// <inheritdoc />
    public ResultVoid Update(Folder folder)
    {
        try
        {
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            const string sql = """
                               UPDATE Folders
                               SET ParentFolderId = @ParentFolderId,
                                   Name = @Name
                               WHERE Id = @Id
                               """;

            int affectedRows = connection.Execute(sql, new { folder.Id, folder.ParentFolderId, folder.Name });

            if (affectedRows == 0)
            {
                logger.Warning("Не удалось обновить несуществующую папку с Id = {FolderId}", folder.Id);
                return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
            }

            logger.Information("Папка с Id = {FolderId} успешно обновлена", folder.Id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при обновлении папки с Id = {FolderId}", folder.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        const int sqliteConstraintErrorCode = 19;
        try
        {
            using SqliteConnection connection = new(connectionString);
            connection.Open();

            const string sql = "DELETE FROM Folders WHERE Id = @Id";
            int affectedRows = connection.Execute(sql, new { Id = id });

            if (affectedRows == 0)
            {
                logger.Warning("Попытка удалить несуществующую папку с Id = {FolderId}", id);
                return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
            }

            logger.Information("Папка с Id = {FolderId} успешно удалена", id);
            return ResultVoid.Success();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == sqliteConstraintErrorCode)
        {
            logger.Warning(ex, "Невозможно удалить папку с Id = {FolderId}: имеются дочерние объекты", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.HasChildren);
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при удалении папки с Id = {FolderId}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }
}