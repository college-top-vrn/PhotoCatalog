using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects.Photo;

/// <summary>
///     Ориентация съёмки изображения.
/// </summary>
public enum Orientation : byte
{
    /// <summary>
    ///     Горизонтальная.
    /// </summary>
    Horizontal,

    /// <summary>
    ///     Вертикальная.
    /// </summary>
    Vertical
}

/// <summary>
///     ValueObject, представляющий собой метаданные фотографии.
/// </summary>
public record Metadata
{
    /// <summary>
    ///     Производитель камеры.
    /// </summary>
    public string? Make { get; }

    /// <summary>
    ///     Модель устройства.
    /// </summary>
    public string? Model { get; }

    /// <summary>
    ///     Модель объектива.
    /// </summary>
    public string? LensModel { get; }

    /// <summary>
    ///     Фокусное расстояние объектива в миллиметрах.
    /// </summary>
    public double? FocalLength { get; }

    /// <summary>
    ///     Диафрагма.
    /// </summary>
    public double? Aperture { get; }

    /// <summary>
    ///     Выдержка.
    /// </summary>
    public string? ExposureTime { get; }

    /// <summary>
    ///     Светочувствительность сенсора.
    /// </summary>
    public double? Iso { get; }

    /// <summary>
    ///     Имеет ли вспышку.
    /// </summary>
    public bool? HasFlashfire { get; }

    /// <summary>
    ///     Широта.
    /// </summary>
    public double? Latitude { get; }

    /// <summary>
    ///     Долгота.
    /// </summary>
    public double? Longitude { get; }

    /// <summary>
    ///     Высота.
    /// </summary>
    public double? Altitude { get; }

    /// <summary>
    ///     Ориентация съёмки изображения.
    /// </summary>
    public Orientation? Orientation { get; }

    /// <summary>
    ///     Цветовое пространство.
    /// </summary>
    public string? ColorSpace { get; }

    /// <summary>
    ///     Дата и время съемки с часовым поясом.
    /// </summary>
    public DateTime DateTimeOffset { get; }

    /// <summary>
    ///     Имеет ли расширенный динамический диапазон.
    /// </summary>
    public bool HasHdr { get; }

    /// <summary>
    ///     Является ли панорамой.
    /// </summary>
    public bool IsPanorama { get; }

    /// <summary>
    ///     Автор.
    /// </summary>
    public string Artist { get; }

    public Metadata(
        string make,
        string model,
        string lensModel,
        double focalLength,
        double aperture,
        string exposureTime,
        double iso,
        bool hasFlashfire,
        double latitude,
        double longitude,
        double altitude,
        Orientation orientation,
        string colorSpace,
        DateTime dateTimeOffset,
        bool hasHdr,
        bool isPanorama,
        string artist
    )
    {
        Make = make;
        Model = model;
        LensModel = lensModel;
        FocalLength = focalLength;
        Aperture = aperture;
        ExposureTime = exposureTime;
        Iso = iso;
        HasFlashfire = hasFlashfire;
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
        Orientation = orientation;
        ColorSpace = colorSpace;
        DateTimeOffset = dateTimeOffset;
        HasHdr = hasHdr;
        IsPanorama = isPanorama;
        Artist = artist;
    }

    /// <summary>
    ///     Создаёт новые метаданные.
    /// </summary>
    /// <param name="make">производитель камеры.</param>
    /// <param name="model">модель устройства.</param>
    /// <param name="lensModel">модель объектива.</param>
    /// <param name="focalLength">фокусное расстояние объектива в миллиметрах.</param>
    /// <param name="aperture">диафрагма.</param>
    /// <param name="exposureTime">выдержка.</param>
    /// <param name="iso">светочувствительность сенсора.</param>
    /// <param name="hasFlashfire">имеет ли вспышку.</param>
    /// <param name="latitude">широта.</param>
    /// <param name="longitude">долгота.</param>
    /// <param name="altitude">высота.</param>
    /// <param name="orientation">ориентация съёмки изображения.</param>
    /// <param name="colorSpace">цветовое пространство.</param>
    /// <param name="dataTimeOffset">дата и время съемки с часовым поясом.</param>
    /// <param name="hasHdr">имеет ли расширенный динамический диапазон.</param>
    /// <param name="isPanorama">является ли панорамой.</param>
    /// <param name="artist">автор.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с новым экземпляром метаданных.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public Result<Metadata> Create(
        string make,
        string model,
        string lensModel,
        double focalLength,
        double aperture,
        string exposureTime,
        double iso,
        bool hasFlashfire,
        double latitude,
        double longitude,
        double altitude,
        Orientation orientation,
        string colorSpace,
        DateTime dataTimeOffset,
        bool hasHdr,
        bool isPanorama,
        string artist
    )
    {
        Metadata metadata = new(
             make,
             model,
             lensModel,
             focalLength,
             aperture,
             exposureTime,
             iso,
             hasFlashfire,
             latitude,
             longitude,
             altitude,
             orientation,
             colorSpace,
             dataTimeOffset,
             hasHdr,
             isPanorama,
             artist
        );

        return Result.Success(metadata);
    }
}