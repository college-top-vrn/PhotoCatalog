using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.Errors;

/// <summary>
///     Единый статический класс (реестр),
///     который содержит все ошибки уровня Application
///     в виде заранее определенных структур <see cref="ResultError" />.
/// </summary>
public static class ApplicationErrors
{
    /// <summary>
    ///     Общие ошибки приложения.
    /// </summary>
    public static class General
    {
        /// <summary>
        ///     Ошибка, когда запрашиваемая сущность не найдена.
        /// </summary>
        public static readonly ResultError NotFound = new("General.NotFound", "Запрашиваемая сущность не найдена.");
    }

    /// <summary>
    ///     Ошибки, связанные с файлами.
    /// </summary>
    public static class Files
    {
        /// <summary>
        ///     Ошибка, когда физический файл по указанному пути не найден.
        /// </summary>
        public static readonly ResultError FileNotFound =
            new("Files.FileNotFound", "Физический файл по указанному пути не найден.");

        /// <summary>
        ///     ошибка, когда фаил остается осиротевшим на диске
        /// </summary>
        public static readonly ResultError
            OrphanedFile = new("Files.OrphanedFile", "Файл остался осиротевшим на диске");
    }

    /// <summary>
    ///     Ошибки, связанные с папками.
    /// </summary>
    public static class Folders
    {
        /// <summary>
        ///     Ошибка, когда обнаружена циклическая зависимость:
        ///     нельзя переместить папку внутрь её собственного потомка.
        /// </summary>
        public static readonly ResultError CycleDetected =
            new("Folders.CycleDetected",
                "Обнаружена циклическая зависимость: нельзя переместить папку внутрь её собственного потомка.");
    }

    /// <summary>
    ///     Ошибки, связанные с транзакциями.
    /// </summary>
    public static class Transactions
    {
        /// <summary>
        ///     Ошибка, которая обозначает, что при начале транзакции
        ///     что то пошло не так
        /// </summary>
        public static readonly ResultError StartTransactions =
            new("Transactions.BeginFailed",
                "Ошибка начала транзакции");

        /// <summary>
        ///     Ошибка, возникающая при попытке зафиксировать (commit) транзакцию.
        /// </summary>
        public static readonly ResultError CommitFailed =
            new(
                "Transactions.CommitFailed",
                "Не удалось зафиксировать изменения в транзакции (commit).");
    }

    /// <summary>
    ///     Ошибка, связаная с обновлением альбома
    /// </summary>
    public static class Albums


    {
        /// <summary>
        ///     Ошибка, которая обозначает, что при обновлении
        ///     что то пошло не так
        /// </summary>
        public static readonly ResultError UpdateFailed =
            new("Albums.UpdateFailed",
                "Ошибка обновления альбома");
    }

    /// <summary>
    ///     Ошибки, связанные с выполнением use-cases (сценариев приложения).
    /// </summary>
    public static class UseCases
    {
        /// <summary>
        ///     Ошибка, возникающая при непредвиденном системном сбое внутри use-case.
        /// </summary>
        public static readonly ResultError SystemFailure =
            new(
                "UseCases.SystemFailure",
                "Произошла системная ошибка при выполнении сценария приложения (UseCase).");
    }

    /// <summary>
    ///     Ошибки, связанные с HTTP-запросами.
    /// </summary>
    public static class Http
    {
        /// <summary>
        ///     Ошибка, когда запрос не соответствует формату multipart/form-data.
        /// </summary>
        public static readonly ResultError InvalidMultipartRequest = new(
            "Http.InvalidMultipartRequest",
            "Ожидается multipart/form-data запрос");

        /// <summary>
        ///     Ошибка, когда файл не загружен или пуст.
        /// </summary>
        public static readonly ResultError FileNotUploaded = new(
            "Http.FileNotUploaded",
            "Файл не загружен или пуст");
    }
}