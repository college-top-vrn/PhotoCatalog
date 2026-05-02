namespace PhotoCatalog.Domain.Primitives;

/// <summary>
///     Единый статический класс (реестр),
///     который содержит все возможные бизнес-ошибки предметной области
///     в виде заранее определенных структур <see cref="Error" />>.
/// </summary>
public static class DomainErrors
{
    /// <summary>
    ///     Ошибки для <see cref="Dimensions" />.
    /// </summary>
    public static class Dimensions
    {
        /// <summary>
        ///     Ошибка, когда ширина и высота меньше или равны нулю или меньше разрешенного предела размера.
        /// </summary>
        public static readonly Error Invalid = new(
            "Dimensions.Invalid",
            "Ширина и высота должны быть строго больше нуля или меньше разрешенного предела размера.");
    }

    /// <summary>
    ///     Ошибки для <see cref="Tag" />.
    /// </summary>
    public static class Tag
    {
        /// <summary>
        ///     Ошибка, когда имя тега пустое или состоит только из пробелов.
        /// </summary>
        public static readonly Error EmptyName = new(
            "Tag.EmptyName",
            "Имя тега не может быть пустым или состоять только из пробелов.");

        /// <summary>
        ///     Ошибка, когда имя тега превышает 50 символов.
        /// </summary>
        public static readonly Error TooLong = new(
            "Tag.TooLong",
            "Имя тега не должно превышать 50 символов.");
    }

    /// <summary>
    ///     Ошибки для <see cref="Photo" />.
    /// </summary>
    public static class Photo
    {
        /// <summary>
        ///     Ошибка, когда путь к файлу фотографии пустой.
        /// </summary>
        public static readonly Error EmptyPath = new(
            "Photo.EmptyPath",
            "Путь к файлу фотографии не может быть пустым.");

        /// <summary>
        ///     Ошибка, когда данный тег уже привязан к этой фотографии.
        /// </summary>
        public static readonly Error DuplicateTag = new(
            "Photo.DuplicateTag",
            "Данный тег уже привязан к этой фотографии.");
    }

    /// <summary>
    ///     Ошибки для <see cref="Folder" />.
    /// </summary>
    public static class Folder
    {
        /// <summary>
        ///     Ошибка, когда имя папки пустое.
        /// </summary>
        public static readonly Error EmptyName = new(
            "Folder.EmptyName",
            "Имя папки не может быть пустым.");

        /// <summary>
        ///     Ошибка, когда папка перемещается внутрь самой себя (циклическая ссылка).
        /// </summary>
        public static readonly Error CannotMoveToSelf = new(
            "Folder.CannotMoveToSelf",
            "Папка не может быть перемещена внутрь самой себя (циклическая ссылка).");
    }

    /// <summary>
    ///     Ошибки для <see cref="Album" />
    /// </summary>
    public static class Album
    {
        /// <summary>
        ///     Ошибка, когда имя альбома пустое.
        /// </summary>
        public static readonly Error EmptyName = new(
            "Album.EmptyName",
            "Имя альбома не может быть пустым.");

        /// <summary>
        ///     Ошибка, когда эта фотография уже находится в данном альбоме.
        /// </summary>
        public static readonly Error DuplicatePhoto = new(
            "Album.DuplicatePhoto",
            "Эта фотография уже находится в данном альбоме.");
    }
}