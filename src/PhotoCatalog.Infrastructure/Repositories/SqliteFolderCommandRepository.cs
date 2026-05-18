using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <inheritdoc />
public class SqliteFolderCommandRepository(string connectionString, ILogger logger) : IFolderCommandRepository
{
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
    public ResultVoid Add(Folder folder, int id)
    {
        throw new System.NotImplementedException();
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