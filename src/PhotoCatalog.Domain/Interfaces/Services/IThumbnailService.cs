using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Services;
/// <summary>
/// Интерфейс сервиса высокопроизводительной генерации миниатюр (превью) для изображений.
/// </summary>
public interface IThumbnailService
{
    /// <summary>
    /// Генерирует легковесное превью-изображение с сохранением пропорций, 
    /// автоматическим применением EXIF-ориентации и конвертацией в цветовой профиль sRGB.
    /// </summary>
    /// <param name="sourcePath">Полный путь к исходному файлу изображения на диске.</param>
    /// <param name="targetPath">Полный путь для сохранения сгенерированного превью.</param>
    /// <param name="maxSize">Максимальный размер (в пикселях) по большей стороне для масштабирования.</param>
    /// <returns>
    /// Объект <see cref="ResultVoid"/>, указывающий на успешное завершение операции 
    /// или содержащий информацию об ошибке в случае сбоя.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Инициируется, если один из путей равен null или пуст.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Инициируется, если <paramref name="maxSize"/> меньше или равен нулю.</exception>
    public ResultVoid Generate(string sourcePath, string targetPath, int maxSize);
    
}