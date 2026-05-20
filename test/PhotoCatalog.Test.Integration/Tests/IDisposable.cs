using Serilog.Core;

namespace PhotoCatalog.Tests.Integration.UnitOfWork;

using System;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;
using Xunit;

/// <summary>
///     Заглушка логгера для тестирования.
///     Реализует <see cref="ILogger{T}" /> и ничего не логирует.
///     Не требует внешних зависимостей, кроме Microsoft.Extensions.Logging.Abstractions.
/// </summary>
/// <typeparam name="T">Категория логгера.</typeparam>
internal sealed class NullLogger<T> : ILogger<T>
{
    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => false;

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
    }
}

/// <summary>
///     Интеграционные тесты для <see cref="SqliteUnitOfWork" />.
///     Проверяют управление транзакциями, атомарность, автоматический откат и обработку ошибок.
/// </summary>
/// <remarks>
///     Все тесты используют in-memory базу данных (<c>Data Source=:memory:;</c>), полностью изолированы.
///     Доступ к <see cref="SqliteConnection" /> внутри <see cref="SqliteUnitOfWork" /> осуществляется через рефлексию,
///     так как публичный API не предоставляет соединения для выполнения произвольных SQL-команд.
/// </remarks>
public class SqliteUnitOfWorkTests
{
    private const string InMemoryConnectionString = "Data Source=:memory:;";
    private readonly Logger _logger;

    /// <summary>
    ///     Инициализирует тестовый класс, создавая заглушку логгера.
    /// </summary>
    public SqliteUnitOfWorkTests()
    {
        _logger = new NullLogger<SqliteUnitOfWork>();
    }

    /// <summary>
    ///     Возвращает внутреннее <see cref="SqliteConnection" /> из <see cref="SqliteUnitOfWork" /> через рефлексию.
    /// </summary>
    /// <param name="uow">Экземпляр <see cref="SqliteUnitOfWork" />.</param>
    /// <returns>Внутреннее подключение к базе данных.</returns>
    /// <exception cref="InvalidOperationException">Если не удалось получить подключение.</exception>
    private static SqliteConnection GetConnection(SqliteUnitOfWork uow)
    {
        var field = typeof(SqliteUnitOfWork).GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(uow) as SqliteConnection
               ?? throw new InvalidOperationException("Не удалось получить подключение из SqliteUnitOfWork");
    }

    /// <summary>
    ///     Создаёт тестовую таблицу в переданном соединении.
    /// </summary>
    /// <param name="connection">Подключение к базе данных.</param>
    private static void CreateTestTable(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
                          CREATE TABLE IF NOT EXISTS TestEntities (
                              Id INTEGER PRIMARY KEY AUTOINCREMENT,
                              Name TEXT NOT NULL
                          );
                          """;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Вставляет тестовую запись в таблицу TestEntities.
    /// </summary>
    /// <param name="connection">Подключение к базе данных.</param>
    /// <param name="name">Значение поля Name для вставки.</param>
    private static void InsertTestEntity(SqliteConnection connection, string name)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO TestEntities (Name) VALUES (@name);";
        cmd.Parameters.AddWithValue("@name", name);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Проверяет существование записи в таблице TestEntities, используя новое (отдельное) соединение.
    ///     Используется для верификации фиксации транзакции.
    /// </summary>
    /// <param name="name">Имя для поиска.</param>
    /// <returns><c>true</c>, если запись существует; иначе <c>false</c>.</returns>
    private static bool RecordExistsInNewConnection(string name)
    {
        using var connection = new SqliteConnection(InMemoryConnectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM TestEntities WHERE Name = @name;";
        cmd.Parameters.AddWithValue("@name", name);
        var count = (long)cmd.ExecuteScalar()!;
        return count > 0;
    }

    /// <summary>
    ///     Проверяет, что <see cref="IUnitOfWork.Commit" /> успешно фиксирует изменения в базе данных.
    /// </summary>
    /// <remarks>
    ///     Тест-кейс:
    ///     <list type="number">
    ///         <item><description>Вызвать <see cref="IUnitOfWork.BeginTransaction" /> — ожидается <c>IsSuccess = true</c>.</description></item>
    ///         <item><description>Выполнить вставку тестовой записи через соединение UnitOfWork.</description></item>
    ///         <item><description>Вызвать <see cref="IUnitOfWork.Commit" /> — ожидается <c>IsSuccess = true</c>.</description></item>
    ///         <item><description>Открыть новое соединение и проверить наличие записи — ожидается существование.</description></item>
    ///     </list>
    /// </remarks>
    [Fact]
    public void Commit_WhenTransactionIsActive_ShouldPersistDataToDatabase()
    {
        // Arrange
        using var uow = new SqliteUnitOfWork(InMemoryConnectionString, _logger);
        var connection = GetConnection(uow);
        CreateTestTable(connection);

        // Act
        var beginResult = uow.BeginTransaction();
        Assert.True(beginResult.IsSuccess, "BeginTransaction должен вернуть Success");

        InsertTestEntity(connection, "commit_test");

        var commitResult = uow.Commit();
        Assert.True(commitResult.IsSuccess, "Commit должен вернуть Success");

        // Assert
        Assert.True(RecordExistsInNewConnection("commit_test"), "Запись должна быть видна из другого соединения после Commit");
    }

    /// <summary>
    ///     Проверяет, что <see cref="IUnitOfWork.Rollback" /> отменяет все изменения, сделанные в транзакции.
    /// </summary>
    /// <remarks>
    ///     Тест-кейс:
    ///     <list type="number">
    ///         <item><description>Вызвать <see cref="IUnitOfWork.BeginTransaction" /> — ожидается <c>IsSuccess = true</c>.</description></item>
    ///         <item><description>Выполнить вставку тестовой записи через соединение UnitOfWork.</description></item>
    ///         <item><description>Вызвать <see cref="IUnitOfWork.Rollback" /> — ожидается <c>IsSuccess = true</c>.</description></item>
    ///         <item><description>В том же соединении проверить отсутствие записи.</description></item>
    ///     </list>
    /// </remarks>
    [Fact]
    public void Rollback_WhenTransactionIsActive_ShouldCancelChanges()
    {
        // Arrange
        using var uow = new SqliteUnitOfWork(InMemoryConnectionString, _logger);
        var connection = GetConnection(uow);
        CreateTestTable(connection);

        // Act
        var beginResult = uow.BeginTransaction();
        Assert.True(beginResult.IsSuccess, "BeginTransaction должен вернуть Success");

        InsertTestEntity(connection, "rollback_test");

        var rollbackResult = uow.Rollback();
        Assert.True(rollbackResult.IsSuccess, "Rollback должен вернуть Success");

        // Assert
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM TestEntities WHERE Name = 'rollback_test';";
        var count = (long)cmd.ExecuteScalar()!;
        Assert.Equal(0, count);
    }

    /// <summary>
    ///     Проверяет, что <see cref="SqliteUnitOfWork.Dispose" /> автоматически откатывает незакоммиченную транзакцию.
    /// </summary>
    /// <remarks>
    ///     Тест-кейс:
    ///     <list type="number">
    ///         <item><description>Создать UnitOfWork внутри блока <c>using</c>.</description></item>
    ///         <item><description>Вызвать <see cref="IUnitOfWork.BeginTransaction" />.</description></item>
    ///         <item><description>Выполнить вставку тестовой записи.</description></item>
    ///         <item><description><b>Не вызывать <see cref="IUnitOfWork.Commit" /></b> — просто выйти из блока <c>using</c>.</description></item>
    ///         <item><description>После выхода из блока проверить через новое соединение — запись отсутствует.</description></item>
    ///     </list>
    /// </remarks>
    [Fact]
    public void Dispose_WhenTransactionNotCommitted_ShouldAutoRollbackChanges()
    {
        // Arrange
        var testName = "dispose_auto_rollback";

        // Act
        using (var uow = new SqliteUnitOfWork(InMemoryConnectionString, _logger))
        {
            var connection = GetConnection(uow);
            CreateTestTable(connection);

            uow.BeginTransaction();
            InsertTestEntity(connection, testName);
            // Commit не вызывается
        }

        // Assert
        Assert.False(RecordExistsInNewConnection(testName), "Запись не должна существовать после Dispose без Commit");
    }

    /// <summary>
    ///     Проверяет, что повторный вызов <see cref="IUnitOfWork.BeginTransaction" /> возвращает Failure с ошибкой
    ///     <see cref="InfrastructureErrors.Database.TransactionAlreadyExists" />.
    /// </summary>
    [Fact]
    public void BeginTransaction_WhenAlreadyStarted_ShouldReturnFailure()
    {
        // Arrange
        using var uow = new SqliteUnitOfWork(InMemoryConnectionString, _logger);

        // Act
        var firstResult = uow.BeginTransaction();
        Assert.True(firstResult.IsSuccess, "Первый BeginTransaction должен быть успешным");

        var secondResult = uow.BeginTransaction();

        // Assert
        Assert.True(secondResult.IsFailure, "Второй BeginTransaction должен вернуть Failure");
        Assert.Equal(InfrastructureErrors.Database.TransactionAlreadyExists.Code, secondResult.Error.Code);
    }

    /// <summary>
    ///     Проверяет, что вызов <see cref="IUnitOfWork.Commit" /> без предварительного BeginTransaction
    ///     возвращает Failure с ошибкой <see cref="InfrastructureErrors.Database.NoActiveTransaction" />.
    /// </summary>
    [Fact]
    public void Commit_WithoutActiveTransaction_ShouldReturnFailure()
    {
        // Arrange
        using var uow = new SqliteUnitOfWork(InMemoryConnectionString, _logger);

        // Act
        var result = uow.Commit();

        // Assert
        Assert.True(result.IsFailure, "Commit без транзакции должен вернуть Failure");
        Assert.Equal(InfrastructureErrors.Database.NoActiveTransaction.Code, result.Error.Code);
    }

    /// <summary>
    ///     Проверяет, что вызов <see cref="IUnitOfWork.Rollback" /> без предварительного BeginTransaction
    ///     возвращает Failure с ошибкой <see cref="InfrastructureErrors.Database.NoActiveTransaction" />.
    /// </summary>
    [Fact]
    public void Rollback_WithoutActiveTransaction_ShouldReturnFailure()
    {
        // Arrange
        using var uow = new SqliteUnitOfWork(InMemoryConnectionString, _logger);

        // Act
        var result = uow.Rollback();

        // Assert
        Assert.True(result.IsFailure, "Rollback без транзакции должен вернуть Failure");
        Assert.Equal(InfrastructureErrors.Database.NoActiveTransaction.Code, result.Error.Code);
    }

    /// <summary>
    ///     Проверяет, что повторный вызов <see cref="IUnitOfWork.Commit" /> после успешного коммита
    ///     возвращает Failure с ошибкой <see cref="InfrastructureErrors.Database.NoActiveTransaction" />.
    /// </summary>
    [Fact]
    public void Commit_WhenCalledTwice_SecondCallShouldReturnFailure()
    {
        // Arrange
        using var uow = new SqliteUnitOfWork(InMemoryConnectionString, _logger);
        var connection = GetConnection(uow);
        CreateTestTable(connection);

        // Act
        uow.BeginTransaction();
        InsertTestEntity(connection, "double_commit_test");

        var firstCommit = uow.Commit();
        Assert.True(firstCommit.IsSuccess, "Первый Commit должен быть успешным");

        var secondCommit = uow.Commit();

        // Assert
        Assert.True(secondCommit.IsFailure, "Второй Commit должен вернуть Failure");
        Assert.Equal(InfrastructureErrors.Database.NoActiveTransaction.Code, secondCommit.Error.Code);
    }

    /// <summary>
    ///     Проверяет, что вызов <see cref="IUnitOfWork.Rollback" /> после успешного Commit
    ///     возвращает Failure с ошибкой <see cref="InfrastructureErrors.Database.NoActiveTransaction" />.
    /// </summary>
    [Fact]
    public void Rollback_AfterCommit_ShouldReturnFailure()
    {
        // Arrange
        using var uow = new SqliteUnitOfWork(InMemoryConnectionString, _logger);
        var connection = GetConnection(uow);
        CreateTestTable(connection);

        // Act
        uow.BeginTransaction();
        InsertTestEntity(connection, "rollback_after_commit_test");
        uow.Commit();

        var rollbackResult = uow.Rollback();

        // Assert
        Assert.True(rollbackResult.IsFailure, "Rollback после Commit должен вернуть Failure");
        Assert.Equal(InfrastructureErrors.Database.NoActiveTransaction.Code, rollbackResult.Error.Code);
    }
}