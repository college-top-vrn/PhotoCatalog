using System;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

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
public class SqliteUnitOfWork : IUnitOfWork
{
    private readonly string _connectionString;
    private readonly ILogger _logger;
    private bool _disposed;

    /// <summary>
    ///     Инициализирует новый экземпляр <see cref="SqliteUnitOfWork" />.
    /// </summary>
    /// <param name="connectionString">Строка подключения к базе данных SQLite.</param>
    /// <param name="logger">Логгер для записи ошибок транзакций.</param>
    /// <exception cref="ArgumentNullException">
    ///     Выбрасывается, если <paramref name="connectionString" /> или <paramref name="logger" /> равен <c>null</c>.
    /// </exception>
    public SqliteUnitOfWork(string connectionString, ILogger logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Получает текущее активное подключение к базе данных.
    ///     Доступно только внутри сборки для использования репозиториями.
    /// </summary>
    internal SqliteConnection? Connection { get; private set; }

    /// <summary>
    ///     Получает текущую активную транзакцию.
    ///     Доступно только внутри сборки для использования репозиториями.
    /// </summary>
    internal SqliteTransaction? Transaction { get; private set; }

    /// <summary>
    ///     Начинает новую транзакцию.
    /// </summary>
    /// <returns>
    ///     <see cref="ResultVoid.Success" />, если транзакция успешно начата,
    ///     или <see cref="ResultVoid.Failure" /> с ошибкой.
    /// </returns>
    public ResultVoid BeginTransaction()
    {
        if (Transaction == null)
        {
            return OpenConnectionIfNeeded()
                .Then(() =>
                {
                    try
                    {
                        Transaction = Connection!.BeginTransaction();
                        _logger.Debug("Начата новая транзакция");
                        return ResultVoid.Success();
                    }
                    catch (SqliteException ex)
                    {
                        _logger.Error(ex, "Ошибка SQLite при начале транзакции");
                        return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.Error(ex, "Неверная операция при начале транзакции");
                        return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
                    }
                });
        }

        _logger.Warning("Попытка начать новую транзакцию, когда уже есть активная транзакция");
        return ResultVoid.Failure(InfrastructureErrors.Database.TransactionAlreadyExists);
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
        if (Transaction == null)
        {
            _logger.Warning("Попытка зафиксировать транзакцию, когда нет активной транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.NoActiveTransaction);
        }

        try
        {
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
            _logger.Debug("Транзакция успешно зафиксирована");
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite при фиксации транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (InvalidOperationException ex)
        {
            _logger.Error(ex, "Неверная операция при фиксации транзакции");
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
        if (Transaction == null)
        {
            _logger.Warning("Попытка откатить транзакцию, когда нет активной транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.NoActiveTransaction);
        }

        try
        {
            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
            _logger.Debug("Транзакция успешно откатана");
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite при откате транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (InvalidOperationException ex)
        {
            _logger.Error(ex, "Неверная операция при откате транзакции");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }

    /// <summary>
    ///     Освобождает ресурсы, используемые <see cref="SqliteUnitOfWork" />.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
                Transaction.Dispose();
                Transaction = null;
                _logger.Debug("Активная транзакция откатана при освобождении ресурсов");
            }

            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
                _logger.Debug("Соединение с БД закрыто");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при освобождении ресурсов SqliteUnitOfWork");
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Открывает соединение, если оно еще не открыто.
    /// </summary>
    private ResultVoid OpenConnectionIfNeeded()
    {
        if (Connection != null)
        {
            return ResultVoid.Success();
        }

        try
        {
            Connection = new SqliteConnection(_connectionString);
            Connection.Open();

            using SqliteCommand command = Connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();

            _logger.Debug("Соединение с БД открыто, PRAGMA foreign_keys = ON");
            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite при открытии соединения");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (InvalidOperationException ex)
        {
            _logger.Error(ex, "Неверная операция при открытии соединения");
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }
}