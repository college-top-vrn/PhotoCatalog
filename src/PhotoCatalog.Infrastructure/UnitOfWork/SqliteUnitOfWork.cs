using System;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.UnitOfWork;

/// <summary>
///     Реализация <see cref="IUnitOfWork" /> для SQLite.
///     Инкапсулирует подключение к SQLite, строго контролирует его жизненный цикл
///     и гарантирует включение поддержки внешних ключей.
///     При открытии соединения автоматически выполняется команда PRAGMA foreign_keys = ON.
/// </summary>
public class SqliteUnitOfWork : IUnitOfWork
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteUnitOfWork> _logger;
    private SqliteConnection? _connection;
    private SqliteTransaction? _transaction;
    private bool _disposed;

    /// <summary>
    ///     Инициализирует новый экземпляр <see cref="SqliteUnitOfWork" />.
    /// </summary>
    /// <param name="connectionString">Строка подключения к базе данных SQLite.</param>
    /// <param name="logger">Логгер для записи ошибок транзакций.</param>
    public SqliteUnitOfWork(string connectionString, ILogger<SqliteUnitOfWork> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Получает текущее активное подключение к базе данных.
    ///     Доступно только внутри сборки для использования репозиториями.
    /// </summary>
    internal SqliteConnection? Connection => _connection;

    /// <summary>
    ///     Получает текущую активную транзакцию.
    ///     Доступно только внутри сборки для использования репозиториями.
    /// </summary>
    internal SqliteTransaction? Transaction => _transaction;

    /// <summary>
    ///     Начинает новую транзакцию.
    /// </summary>
    /// <returns>
    ///     <see cref="ResultVoid.Success" />, если транзакция успешно начата,
    ///     или <see cref="ResultVoid.Failure" /> с соответствующей ошибкой.
    /// </returns>
    public ResultVoid BeginTransaction()
    {
        try
        {
            if (_transaction != null)
            {
                _logger.LogWarning("Попытка начать новую транзакцию, когда уже есть активная транзакция");
                var error = new Error("Database.TransactionAlreadyExists", "Транзакция уже существует.");
                return ResultVoid.Failure(error);
            }

            if (_connection == null)
            {
                _connection = new SqliteConnection(_connectionString);
                _connection.Open();
                
                using var command = _connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
                
                _logger.LogDebug("Соединение с БД открыто, PRAGMA foreign_keys = ON");
            }

            _transaction = _connection.BeginTransaction();
            _logger.LogDebug("Начата новая транзакция");
            
            return ResultVoid.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при начале транзакции");
            var error = new Error("Database.ConnectionFailed", "Не удалось установить соединение с базой данных.");
            return ResultVoid.Failure(error);
        }
    }

    /// <summary>
    ///     Фиксирует все изменения текущей транзакции.
    /// </summary>
    /// <returns>
    ///     <see cref="ResultVoid.Success" />, если транзакция успешно зафиксирована,
    ///     или <see cref="ResultVoid.Failure" /> с соответствующей ошибкой.
    /// </returns>
    public ResultVoid Commit()
    {
        try
        {
            if (_transaction == null)
            {
                _logger.LogWarning("Попытка зафиксировать транзакцию, когда нет активной транзакции");
                var error = new Error("Database.NoActiveTransaction", "Нет активной транзакции.");
                return ResultVoid.Failure(error);
            }

            _transaction.Commit();
            _transaction = null;
            _logger.LogDebug("Транзакция успешно зафиксирована");
            
            return ResultVoid.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при фиксации транзакции");
            var error = new Error("Database.ConnectionFailed", "Не удалось установить соединение с базой данных.");
            return ResultVoid.Failure(error);
        }
    }

    /// <summary>
    ///     Отменяет все изменения текущей транзакции.
    /// </summary>
    /// <returns>
    ///     <see cref="ResultVoid.Success" />, если транзакция успешно отменена,
    ///     или <see cref="ResultVoid.Failure" /> с соответствующей ошибкой.
    /// </returns>
    public ResultVoid Rollback()
    {
        try
        {
            if (_transaction == null)
            {
                _logger.LogWarning("Попытка откатить транзакцию, когда нет активной транзакции");
                var error = new Error("Database.NoActiveTransaction", "Нет активной транзакции.");
                return ResultVoid.Failure(error);
            }

            _transaction.Rollback();
            _transaction = null;
            _logger.LogDebug("Транзакция успешно откачена");
            
            return ResultVoid.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при откате транзакции");
            var error = new Error("Database.ConnectionFailed", "Не удалось установить соединение с базой данных.");
            return ResultVoid.Failure(error);
        }
    }

    /// <summary>
    ///     Освобождает ресурсы, используемые <see cref="SqliteUnitOfWork" />.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
                _logger.LogDebug("Соединение с БД закрыто");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при освобождении ресурсов");
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}