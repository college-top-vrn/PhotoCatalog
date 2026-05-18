using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Services;

/// <summary>
///     Интерфейс сервиса высокопроизводительной генерации миниатюр изображений.
/// </summary>
public interface IThumbnailService
{
    /// <summary>
    ///     Генерирует миниатюру изображения по указанным путям и максимальному размеру.
    /// </summary>
    /// <param name="sourcePath">Полный путь к исходному файлу изображения.</param>
    /// <param name="targetPath">Полный путь для сохранения миниатюры.</param>
    /// <param name="maxSize">Максимальный размер изображения по большей стороне в пикселях.</param>
    /// <returns>Результат выполнения операции.</returns>
    ResultVoid Generate(string sourcePath, string targetPath, int maxSize);
}