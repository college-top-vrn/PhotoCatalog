using System;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

namespace PhotoCatalog.Infrastructure.UnitOfWork;

/// <summary>
///     Реализация <see cref="IUnitOfWork" /> для SQLite.
/// </summary>
/// <remarks>
///     Инкапсулирует подключение к SQLite, строго контролирует его жизненный цикл
///     и гарантирует включение поддержки внешних ключей.
///     <para>
///         <b>Важно:</b> При открытии соединения автоматически выполняется команда
///         <c>PRAGMA foreign_keys = ON</c> для обеспечения целостности данных.
///     </para>
/// </remarks>
public class SqliteUnitOfWork : IUnitOfWork, IDisposable
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
    /// <exception cref="ArgumentNullException">
    ///     Выбрасывается, если <paramref name="connectionString" /> или <paramref name="logger" /> равен <c>null</c>.
    /// </exception>
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
    ///     или <see cref="ResultVoid.Failure" /> с ошибкой.
    /// </returns>
    public ResultVoid BeginTransaction()
    {
        if (_transaction != null)
        {
            _logger.LogWarning("Попытка начать новую транзакцию, когда уже есть активная транзакция");
            return ResultVoid.Failure(InfrastructureErrors.Database.TransactionAlreadyExists);
        }

        return OpenConnectionIfNeeded()
            .Then(() =>
            {
                try
                {
                    _transaction = _connection!.BeginTransaction();
                    _logger.LogDebug("Начата новая транзакция");
                    return ResultVoid.Success();
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Ошибка SQLite при начале транзакции");
                    return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Неверная операция при начале транзакции");
                    return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
                }
            });
    }

    /// <summary>
    ///     Фиксирует все изменения текущей транзакции.
    /// </summary>
    /// <returns>
    ///     <see cref="ResultVoid.Success" />, если транзакция успешно зафиксирована,
    ///     или <see cref="ResultVoid.Failure" /> с ошибкой.
    /// </returns>
    public ResultVoid Commit()
    {

        if (_transaction == null)
        {
            _logger.LogWarning("Попытка зафиксировать транзакцию, когда нет активной транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.NoActiveTransaction);
        }

        try
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
            _logger.LogDebug("Транзакция успешно зафиксирована");
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Ошибка SQLite при фиксации транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Неверная операция при фиксации транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Отменяет все изменения текущей транзакции.
    /// </summary>
    /// <returns>
    ///     <see cref="ResultVoid.Success" />, если транзакция успешно отменена,
    ///     или <see cref="ResultVoid.Failure" /> с ошибкой.
    /// </returns>
    public ResultVoid Rollback()
    {
        if (_transaction == null)
        {
            _logger.LogWarning("Попытка откатить транзакцию, когда нет активной транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.NoActiveTransaction);
        }

        try
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
            _logger.LogDebug("Транзакция успешно откатана");
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Ошибка SQLite при откате транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Неверная операция при откате транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Открывает соединение, если оно еще не открыто.
    /// </summary>
    private ResultVoid OpenConnectionIfNeeded()
    {
        if (_connection != null)
            return ResultVoid.Success();

        try
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();

            using var command = _connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();

            _logger.LogDebug("Соединение с БД открыто, PRAGMA foreign_keys = ON");
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Ошибка SQLite при открытии соединения");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Неверная операция при открытии соединения");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Освобождает ресурсы, используемые <see cref="SqliteUnitOfWork" />.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
                _logger.LogDebug("Активная транзакция откатана при освобождении ресурсов");
            }

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
            _logger.LogError(ex, "Ошибка при освобождении ресурсов SqliteUnitOfWork");
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}