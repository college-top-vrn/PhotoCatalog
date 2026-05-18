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
        ///     Ошибка доступа к файлу или директории.
        ///     Возникает при отсутствии прав на чтение/запись/удаление.
        /// </summary>
        public static readonly Error AccessDenied = new(
            "FileStorage.AccessDenied",
            "Отказано в доступе к файлу или директории. Проверьте права приложения.");

        /// <summary>
        ///     Ошибка при переполнении дискового пространства.
        ///     Возникает, когда на целевом диске недостаточно места для сохранения файла.
        /// </summary>
        public static readonly Error DiskFull = new(
            "FileStorage.DiskFull",
            "Недостаточно свободного места на диске для выполнения операции.");

        /// <summary>
        ///     Общая ошибка ввода-вывода.
        ///     Возникает при непредвиденных проблемах с файловой системой.
        /// </summary>
        public static readonly Error IOError = new(
            "FileStorage.IOError",
            "Произошла ошибка ввода-вывода при работе с файлом.");

        /// <summary>
        ///     Ошибка некорректного пути.
        ///     Возникает, когда указанный путь содержит недопустимые символы или имеет неверный формат.
        /// </summary>
        public static readonly Error InvalidPath = new(
            "FileStorage.InvalidPath",
            "Указанный путь к файлу имеет недопустимый формат.");

        /// <summary>
        ///     Ошибка, возникающая при попытке выхода за пределы базовой директории хранилища.
        ///     Возникает, когда относительный путь содержит ".." для навигации вверх.
        /// </summary>
        public static readonly Error PathTraversalAttempt = new(
            "FileStorage.PathTraversalAttempt",
            "Обнаружена попытка выхода за пределы разрешенной директории хранилища.");

        /// <summary>
        ///     Ошибка при генерации миниатюры изображения.
        /// </summary>
        public static readonly Error ThumbnailGenerationFailed = new(
            "FileStorage.ThumbnailGenerationFailed",
            "Не удалось создать миниатюру изображения.");
    }

    /// <summary>
    ///     Ошибки, возникающие при извлечении метаданных из файлов.
    /// </summary>
    public static class MetadataExtractor
    {
        /// <summary>
        ///     Ошибка блокировки файла.
        ///     Возникает, когда файл занят другим процессом (обычно при записи).
        /// </summary>
        public static readonly Error FileLocked = new(
            "MetadataExtractor.FileLocked",
            "Файл заблокирован другим процессом. Повторите попытку позже.");

        /// <summary>
        ///     Ошибка поврежденного файла.
        ///     Возникает, когда структура файла нарушена или он неполный.
        /// </summary>
        public static readonly Error FileCorrupted = new(
            "MetadataExtractor.FileCorrupted",
            "Файл поврежден и не может быть прочитан.");

        /// <summary>
        ///     Ошибка неверного формата.
        ///     Возникает, когда файл не является изображением в поддерживаемом формате.
        /// </summary>
        public static readonly Error NotAnImage = new(
            "MetadataExtractor.NotAnImage",
            "Файл не является изображением или его формат не поддерживается.");

        // TODO: Добавить XML документацию
        /// <summary>
        /// </summary>
        public static readonly Error MetadataNotFound = new(
            "MetadataExtractor.MetadataNotFound", "Метаданные не найдены. ");
    }
}