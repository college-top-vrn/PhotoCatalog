using Dapper;

using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.UnitOfWork;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <inheritdoc />
public class SqliteFolderCommandRepository : IFolderCommandRepository
{
    SqliteFolderCommandRepository(string connectionString, ILogger<SqliteUnitOfWork> logger)
    {
        _unitOfWork = new SqliteUnitOfWork(connectionString, logger);
        _logger = logger;
    }

    private readonly SqliteUnitOfWork _unitOfWork;
    private readonly ILogger<SqliteUnitOfWork> _logger;

    /// <inheritdoc />
    public ResultVoid Add(Folder folder)
    {
        _unitOfWork.BeginTransaction();

        var result = _unitOfWork.Connection!
            .Execute(
                """
                INSERT INTO Folders (Id, ParentFolderId, Name)
                VALUES (@Id, @ParentFolderId, @Name)
                """,
                new { folder.Id, folder.ParentFolderId, folder.Name })
            .ToResult()
            .OnSuccess(_ =>
            {
                _logger.LogInformation("Папка с Id = {FolderId} успешно добавлена", folder.Id);
                _unitOfWork.Commit();
            })
            .OnFailure(_ =>
            {
                _logger.LogError("Ошибка SQLite при добавлении папки с Id = {FolderId}", folder.Id);
                _unitOfWork.Rollback();
            });

        _unitOfWork.Dispose();

        return result;
    }

    /// <inheritdoc />
    public ResultVoid Update(Folder folder)
    {
        _unitOfWork.BeginTransaction();

        var result = _unitOfWork.Connection!
            .Execute(
                """
                UPDATE Folders
                SET ParentFolderId = @ParentFolderId,
                Name = @Name
                WHERE Id = @Id
                """,
                new { folder.Id, folder.ParentFolderId, folder.Name }
            )
            .ToResult()
            .Check(affectedRows => (affectedRows != 0).ToResult())
            .OnSuccess(_ =>
            {
                _logger.LogInformation("Папка с Id = {FolderId} успешно обновлена", folder.Id);
                _unitOfWork.Commit();
            })
            .OnFailure(_ =>
            {
                _logger.LogWarning("Не удалось обновить несуществующую папку с Id = {FolderId}", folder.Id);
                _unitOfWork.Rollback();
            });

        _unitOfWork.Dispose();

        return result;
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        _unitOfWork.BeginTransaction();

        var result = _unitOfWork.Connection!
            .Execute(
                """
                DELETE FROM Folders
                WHERE Id = @Id
                """,
                new { Id = id })
            .ToResult()
            .Check(affectedRows => (affectedRows != 0).ToResult())
            .OnSuccess(_ =>
            {
                _logger.LogInformation("Папка с Id = {FolderId} успешно удалена", id);
                _unitOfWork.Commit();
            })
            .OnFailure(_ =>
            {
                _logger.LogError("Ошибка SQLite при удалении папки с Id = {FolderId}", id);
                _unitOfWork.Rollback();
            });

        _unitOfWork.Dispose();

        return result;
    }
}