using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.Errors;

/// <summary>
///     Единый статический класс (реестр),
///     который содержит все ошибки уровня Application
///     в виде заранее определенных структур <see cref="Error" />.
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
        public static readonly Error NotFound = new("General.NotFound", "Запрашиваемая сущность не найдена.");
    }

    /// <summary>
    ///     Ошибки, связанные с файлами.
    /// </summary>
    public static class Files
    {
        /// <summary>
        ///     Ошибка, когда физический файл по указанному пути не найден.
        /// </summary>
        public static readonly Error FileNotFound =
            new("Files.FileNotFound", "Физический файл по указанному пути не найден.");

        /// <summary>
        ///     ошибка, когда фаил остается осиротевшим на диске
        /// </summary>
        public static readonly Error OrphanedFile = new("Files.OrphanedFile", "Файл остался осиротевшим на диске");
    }

    /// <summary>
    ///     Ошибки, связанные с папками.
    /// </summary>
    public static class Folders
    {
        /// <summary>
        ///     Ошибка, когда обнаружена циклическая зависимост:
        ///     нельзя переместить папку внутрь её собственного потомка.
        /// </summary>
        public static readonly Error CycleDetected =
            new("Folders.CycleDetected",
                "Обнаружена циклическая зависимость: нельзя переместить папку внутрь её собственного потомка.");
    }
}