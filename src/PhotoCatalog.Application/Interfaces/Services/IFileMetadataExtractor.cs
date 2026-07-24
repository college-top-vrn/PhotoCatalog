using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.Interfaces.Services;

/// <summary>
///     Контракт для извлечения метаданных из файлов.
/// </summary>
public interface IFileMetadataExtractor
{
    /// <summary>
    ///     Возвращает хэш файла по указанному пути.
    /// </summary>
    /// <param name="filePath">Путь к файлу.</param>
    /// <returns>Хэш файла или ошибку.</returns>
    Result<string> CalculateHash(string filePath);
}