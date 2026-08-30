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
public sealed record Metadata
{
    /// <summary>
    ///     Производитель камеры.
    /// </summary>
    public string? Make { get; private set; }

    /// <summary>
    ///     Модель устройства.
    /// </summary>
    public string? Model { get; private set; }

    /// <summary>
    ///     Модель объектива.
    /// </summary>
    public string? LensModel { get; private set; }

    /// <summary>
    ///     Фокусное расстояние объектива в миллиметрах.
    /// </summary>
    public double? FocalLength { get; private set; }

    /// <summary>
    ///     Диафрагма.
    /// </summary>
    public double? Aperture { get; private set; }

    /// <summary>
    ///     Выдержка.
    /// </summary>
    public string? ExposureTime { get; private set; }

    /// <summary>
    ///     Светочувствительность сенсора.
    /// </summary>
    public double? Iso { get; private set; }

    /// <summary>
    ///     Широта.
    /// </summary>
    public double? Latitude { get; private set; }

    /// <summary>
    ///     Долгота.
    /// </summary>
    public double? Longitude { get; private set; }

    /// <summary>
    ///     Высота.
    /// </summary>
    public double? Altitude { get; private set; }

    /// <summary>
    ///     Ориентация съёмки изображения.
    /// </summary>
    public Orientation? Orientation { get; private set; }

    /// <summary>
    ///     Цветовое пространство.
    /// </summary>
    public string? ColorSpace { get; private set; }

    /// <summary>
    ///     Дата и время съемки с часовым поясом.
    /// </summary>
    public DateTimeOffset? ShotAt { get; private set; }

    /// <summary>
    ///     Имеет ли вспышку.
    /// </summary>
    public bool? HasFlashfire { get; private set; }

    /// <summary>
    ///     Имеет ли расширенный динамический диапазон.
    /// </summary>
    public bool? HasHdr { get; private set; }

    /// <summary>
    ///     Является ли панорамой.
    /// </summary>
    public bool? IsPanorama { get; private set; }

    /// <summary>
    ///     Автор.
    /// </summary>
    public string? Artist { get; private set; }

    private Metadata() { }

    /// <summary>
    ///     Вложенный класс-строитель, позволяющий создать экземпляр класса <see cref="Metadata"/>.
    /// </summary>
    public sealed class Builder
    {
        private Metadata _metadata;

        /// <summary>
        ///     Конструктор.
        /// </summary>
        public Builder() { _metadata = new Metadata(); }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.Make"/> переданное значение.
        /// </summary>
        /// <param name="make">производитель камеры.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetMake(string make)
        {
            _metadata.Make = make;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.Model"/> переданное значение.
        /// </summary>
        /// <param name="model">модель устройства.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetModel(string model)
        {
            _metadata.Model = model;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.LensModel"/> переданное значение.
        /// </summary>
        /// <param name="lensModel">модель объектива.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetLensModel(string lensModel)
        {
            _metadata.LensModel = lensModel;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.FocalLength"/> переданное значение.
        /// </summary>
        /// <param name="focalLength">фокусное расстояние объектива в миллиметрах.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetFocalLength(double focalLength)
        {
            _metadata.FocalLength = focalLength;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.Aperture"/> переданное значение.
        /// </summary>
        /// <param name="aperture">диафрагма.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetAperture(double aperture)
        {
            _metadata.Aperture = aperture;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.ExposureTime"/> переданное значение.
        /// </summary>
        /// <param name="exposureTime">выдержка.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetExposureTime(string exposureTime)
        {
            _metadata.ExposureTime = exposureTime;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.Iso"/> переданное значение.
        /// </summary>
        /// <param name="iso">светочувствительность сенсора.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetIso(double iso)
        {
            _metadata.Iso = iso;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.Latitude"/> переданное значение.
        /// </summary>
        /// <param name="latitude">широта.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetLatitude(double latitude)
        {
            _metadata.Latitude = latitude;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.Longitude"/> переданное значение.
        /// </summary>
        /// <param name="longitude">долгота.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetLongitude(double longitude)
        {
            _metadata.Longitude = longitude;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.Altitude"/> переданное значение.
        /// </summary>
        /// <param name="altitude">высота.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetAltitude(double altitude)
        {
            _metadata.Altitude = altitude;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.Orientation"/> переданное значение.
        /// </summary>
        /// <param name="orientation">ориентация съёмки изображения.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetOrientation(Orientation orientation)
        {
            _metadata.Orientation = orientation;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.ColorSpace"/> переданное значение.
        /// </summary>
        /// <param name="colorSpace">цветовое пространство.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetColorSpace(string colorSpace)
        {
            _metadata.ColorSpace = colorSpace;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.ShotAt"/> переданное значение.
        /// </summary>
        /// <param name="shotAt">дата и время съемки с часовым поясом.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetShotAt(DateTimeOffset shotAt)
        {
            _metadata.ShotAt = shotAt;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.HasFlashfire"/> переданное значение.
        /// </summary>
        /// <param name="hasFlashfire">имеет ли вспышку.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetHasFlashfire(bool hasFlashfire)
        {
            _metadata.HasFlashfire = hasFlashfire;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.HasHdr"/> переданное значение.
        /// </summary>
        /// <param name="hasHdr">имеет ли расширенный динамический диапазон.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetHasHdr(bool hasHdr)
        {
            _metadata.HasHdr = hasHdr;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.IsPanorama"/> переданное значение.
        /// </summary>
        /// <param name="isPanorama">является ли панорамой.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetIsPanorama(bool isPanorama)
        {
            _metadata.IsPanorama = isPanorama;
            return this;
        }

        /// <summary>
        ///     Присваивает свойству <see cref="Metadata.Artist"/> переданное значение.
        /// </summary>
        /// <param name="artist">автор.</param>
        /// <returns>самого строителя.</returns>
        public Builder SetArtist(string artist)
        {
            _metadata.Artist = artist;
            return this;
        }

        /// <summary>
        ///     Меняет текущий экземпляр <see cref="Metadata"/> на новый.
        /// </summary>
        public void Reset() => _metadata = new Metadata();

        /// <summary>
        ///     Создаёт новые метаданные.
        /// </summary>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>Успех с новым экземпляром метаданных.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public Result<Metadata> Build()
        {
            Metadata result = _metadata;

            _metadata = new Metadata();

            return Result.Success(result);
        }
    }
}