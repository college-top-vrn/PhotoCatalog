using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Errors;

/// <summary>
///     Единый статический класс (реестр),
///     который содержит все ошибки уровня Infrastructure
///     в виде заранее определенных структур <see cref="Error" />.
/// </summary>
/// <remarks>
///     В отличие от <see cref="DomainErrors" /> (бизнес-логика) и
///     <see cref="ApplicationErrors" /> (ошибки потока/поиска),
///     этот реестр содержит ошибки, связанные с физическими сбоями систем
///     (базы данных, диска, сети).
/// </remarks>
public static class InfrastructureErrors
{
    /// <summary>
    ///     Ошибки, связанные с работой базы данных.
    /// </summary>
    public static class Database
    {
        /// <summary>
        ///     Ошибка, когда не удалось установить соединение с базой данных.
        /// </summary>
        public static readonly Error ConnectionFailed = new(
            "Database.ConnectionFailed",
            "Не удалось установить соединение с базой данных.");

        /// <summary>
        ///     Ошибка, когда нарушена целостность данных
        ///     (например, дублирование уникального ключа, нарушение внешнего ключа).
        /// </summary>
        public static readonly Error ConstraintViolation = new(
            "Database.ConstraintViolation",
            "Нарушение целостности данных (например, дублирование уникального ключа).");

        /// <summary>
        ///     Ошибка, возникающая при попытке начать новую транзакцию,
        ///     когда уже существует активная транзакция в текущем UnitOfWork.
        ///     Гарантирует, что в рамках одного UnitOfWork может быть только одна транзакция.
        /// </summary>
        public static readonly Error TransactionAlreadyExists = new(
            "Database.TransactionAlreadyExists",
            "Транзакция уже активна");

        /// <summary>
        ///     Ошибка, возникающая при попытке зафиксировать (Commit) или откатить (Rollback)
        ///     транзакцию, когда активной транзакции не существует.
        ///     Предотвращает некорректные операции с неинициализированным состоянием транзакции.
        /// </summary>
        public static readonly Error NoActiveTransaction =
            new("Database.NoActiveTransaction", "Нет активной транзакции");

        /// <summary>
        ///     Ошибка, соответствующая <see cref="Microsoft.Data.Sqlite.SqliteException" />.
        ///     Возникает при ошибке в SQLite запросе.
        /// </summary>
        public static readonly Error Sqlite =
            new("Database.Sqlite", "Ошибка Sqlite.");

        /// <summary>
        ///     Ошибка, когда элемент таблицы базы данных не найден.
        /// </summary>
        public static readonly Error NotFound =
            new("Database.NotFound", "Элемент таблицы базы данных не найден.");

        /// <summary>
        ///     Ошибка, когда при удалении у папки имеются дочерние объекты.
        /// </summary>
        public static readonly Error HasChildren =
            new("Database.HasChildren", "У папки имеются дочерние объекты.");
    }
    
    /// <summary>
    ///     Ошибки, связанные с кэшированием.
    /// </summary>
    public static class Cache
    {
        /// <summary>
        ///     Ошибка, когда не получается произвести кеширование.
        /// </summary>
        public static readonly Error UnknownError = new(
            "Cache.UnknownError",
            "Непредвиденная ошибка при получении элемента из кэша.");
    }

    /// <summary>
    ///     Ошибки, связанные с файловым хранилищем.
    /// </summary>
    public static class FileStorage
    {
        /// <summary>
        ///     Ошибка, когда отказано в доступе к файловой системе.
        /// </summary>
        public static readonly Error AccessDenied = new(
            "FileStorage.AccessDenied",
            "Отказано в доступе к файловой системе.");

        /// <summary>
        ///     Ошибка, когда на диске недостаточно свободного места.
        /// </summary>
        public static readonly Error DiskFull = new(
            "FileStorage.DiskFull",
            "На диске недостаточно свободного места.");

        /// <summary>
        ///     Ошибка, когда произошла ошибка ввода-вывода при работе с файлом.
        /// </summary>
        public static readonly Error IOError = new(
            "FileStorage.IOError",
            "Произошла ошибка ввода-вывода при работе с файлом.");
    }
}