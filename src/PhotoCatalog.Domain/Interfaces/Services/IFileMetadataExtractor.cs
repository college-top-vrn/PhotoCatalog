using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

namespace PhotoCatalog.Domain.Interfaces.Services;

/// <summary>
/// Контракт для извлечения метаданных из файлов.
/// </summary>
public interface IFileMetadataExtractor
{
    /// <summary>
    /// Возвращает хэш файла по указанному пути.
    /// </summary>
    /// <param name="filePath">Путь к файлу.</param>
    /// <returns>Хэш файла или ошибку.</returns>
    Result<string> CalculateHash(string filePath);

    /// <summary>
    /// Возвращает размеры файла (например, ширина и высота).
    /// </summary>
    /// <param name="filePath">Путь к файлу.</param>
    /// <returns>Размеры файла или ошибку.</returns>
    Result<Dimensions> GetDimensions(string filePath);
}