using System.Data;

using Dapper;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

namespace PhotoCatalog.Infrastructure.Persistence;

/// <summary>
///     Реализация Unit of Work для SQLite.
///     Управляет временем жизни соединения и транзакции.
/// </summary>
public sealed class SqliteUnitOfWork : IUnitOfWork
{
    private readonly ILogger<SqliteUnitOfWork> _logger;
    private bool _disposed;

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="SqliteUnitOfWork"/>.
    /// </summary>
    /// <param name="connectionString">Строка подключения к SQLite.</param>
    /// <param name="logger">Логгер для записи инфраструктурных ошибок.</param>
    public SqliteUnitOfWork(string connectionString, ILogger<SqliteUnitOfWork> logger)
    {
        _logger = logger;
        CurrentConnection = new SqliteConnection(connectionString);
        CurrentConnection.Open();
        CurrentConnection.Execute("PRAGMA foreign_keys = ON;");
    }

    /// <summary>
    ///     Текущее активное соединение с SQLite.
    /// </summary>
    public SqliteConnection CurrentConnection { get; }

    /// <summary>
    ///     Текущая активная транзакция, если она открыта.
    /// </summary>
    public IDbTransaction? CurrentTransaction { get; private set; }

    /// <inheritdoc />
    public ResultVoid BeginTransaction()
    {
        try
        {
            CurrentTransaction ??= CurrentConnection.BeginTransaction();
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Не удалось начать транзакцию SQLite.");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <inheritdoc />
    public ResultVoid Commit()
    {
        if (CurrentTransaction is null)
        {
            return ResultVoid.Success();
        }

        try
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Не удалось зафиксировать транзакцию SQLite.");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <inheritdoc />
    public ResultVoid Rollback()
    {
        if (CurrentTransaction is null)
        {
            return ResultVoid.Success();
        }

        try
        {
            CurrentTransaction.Rollback();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Не удалось откатить транзакцию SQLite.");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Освобождает ресурсы соединения и транзакции.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        CurrentTransaction?.Dispose();
        CurrentConnection.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
