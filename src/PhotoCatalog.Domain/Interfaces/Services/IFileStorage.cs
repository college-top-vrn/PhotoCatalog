using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Services;

/// <summary>
/// Хранилище файлов.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Сохраняет файл из исходного пути с новым именем.
    /// </summary>
    /// <param name="sourcePath">Путь к исходному файлу</param>
    /// <param name="newFileName">Новое имя файла</param>
    /// <returns>Путь к сохранённому файлу или ошибка</returns>
    Result<string> StoreFile(string sourcePath, string newFileName);

    /// <summary>
    /// Удаляет файл по пути.
    /// </summary>
    /// <param name="filePath">Путь к файлу для удаления</param>
    /// <returns>Результат операции</returns>
    ResultVoid DeleteFile(string filePath);

    /// <summary>
    /// Проверяет, существует ли файл по указанному пути.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>True или false и ошибка при неудаче</returns>
    Result<bool> FileExists(string filePath);
}