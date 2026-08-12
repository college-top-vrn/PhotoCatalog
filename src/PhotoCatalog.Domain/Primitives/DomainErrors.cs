namespace PhotoCatalog.Domain.Primitives;

/// <summary>
///     Единый статический класс (реестр),
///     который содержит все возможные бизнес-ошибки предметной области
///     в виде заранее определенных структур <see cref="ResultError" />>.
/// </summary>
public static class DomainErrors
{
    /// <summary>
    ///     Ошибки для <see cref="EventBus"/>.
    /// </summary>
    public static class EventBus
    {
        /// <summary>
        ///     Ошибка, обозначающая невозможность добавить пару.
        /// </summary>
        public static readonly ResultError UnableToAddPair = new(
            "EventBus.UnableToAddPair",
            "Невозможно добавить пару"
        );

        /// <summary>
        ///     Ошибка, обозначающая невозможность обновить пару.
        /// </summary>
        public static readonly ResultError UnableToUpdatePair = new(
            "EventBus.UnableToUpdatePair",
            "Невозможно обновить пару"
        );

        /// <summary>
        ///     Ошибка, обозначающая отсутствие ключа в словаре.
        /// </summary>
        public static readonly ResultError KeyNotExists = new(
            "EventBus.KeyNotExists",
            "Ключ не существует"
        );
    }

    /// <summary>
    ///     Ошибки для <see cref="Entity"/>.
    /// </summary>
    public static class Entity
    {
        /// <summary>
        ///     Ошибка, обозначающая наличие похожего доменного события.
        /// </summary>
        public static readonly ResultError SuchDomainEventAlreadyExists = new(
            "Entity.SuchDomainEventAlreadyExists",
            "Такое доменное событие уже существует"
        );

        /// <summary>
        ///     Ошибка, обозначающая, что список доменных событий уже пуст.
        /// </summary>
        public static readonly ResultError DomainEventListIsAlreadyEmpty = new(
            "Entity.DomainEventListIsAlreadyEmpty",
            "Список доменных событий уже пуст"
        );
    }

    /// <summary>
    ///     Ошибки для <see cref="Observable"/>
    /// </summary>
    public static class Observable
    {
        /// <summary>
        ///     Ошибка, обозначающая наличия похожего наблюдателя.
        /// </summary>
        public static readonly ResultError SuchObserverAlreadyExists = new(
            "ObservableEntity.SuchObserverAlreadyExists",
            "Такой наблюдатель уже существует"
        );

        /// <summary>
        ///     Ошибка, обозначающая отсутствия конкретного наблюдателя.
        /// </summary>
        public static readonly ResultError SuchObserverNotExists = new(
            "ObservableEntity.SuchObserverNotExists",
            "Такого наблюдателя не существует"
        );

        /// <summary>
        ///     Ошибка, обозначающая отсутствия наблюдателей.
        /// </summary>
        public static readonly ResultError ObserversNotExist = new(
            "ObservableEntity.ObserversNotExist",
            "Нет существующих наблюдателей"
        );
    }

    /// <summary>
    ///     Ошибки для <see cref="Name"/>
    /// </summary>
    public static class Name
    {
        /// <summary>
        ///     Ошибка, обозначающая отсутствия имени.
        /// </summary>
        public static readonly ResultError IsEmpty = new(
            "Name.IsEmpty",
            "Имя пустое"
        );

        /// <summary>
        ///     Ошибка, обозначающая слишком длинное имя.
        /// </summary>
        public static readonly ResultError IsTooLong = new(
            "Name.IsTooLong",
            "Имя слишком длинное"
        );
    }

    /// <summary>
    ///     Ошибки для <see cref="Metadata"/>
    /// </summary>
    public static class Metadata
    {
        /// <summary>
        ///     Ошибка, обозначающая отсутствие метаданных фотографии.
        /// </summary>
        public static readonly ResultError IsEmpty = new(
            "Metadata.IsEmpty",
            "Метаданные файла отсутствуют"
        );
    }

    /// <summary>
    ///     Ошибки для <see cref="CapturedAt"/>
    /// </summary>
    public static class CapturedAt
    {
        /// <summary>
        ///     Ошибка, обозначающая неправильный формат даты и времени.
        /// </summary>
        public static readonly ResultError IsInvalid = new(
            "CapturedAt.IsInvalid",
            "Формат даты и время съёмки неправильный."
        );
    }

    /// <summary>
    ///     Ошибки для <see cref="Mime"/>
    /// </summary>
    public static class Mime
    {
        /// <summary>
        ///     Ошибка, обозначающая отсутсвие MIME у файла.
        /// </summary>
        public static readonly ResultError IsEmpty = new(
            "Mime.IsEmpty",
            "MIME файла пустой."
        );
    }

    /// <summary>
    ///     Ошибки для <see cref="StorageKey"/>
    /// </summary>
    public static class StorageKey
    {
        /// <summary>
        ///     Ошибка, обозначающая отсутствия ключа от физического файла в S3-хранилище.
        /// </summary>
        public static readonly ResultError IsEmpty = new(
            "StorageKey.IsEmpty",
            "Ключ от физического файла в S3 пустой"
        );
    }

    /// <summary>
    ///     Ошибки для <see cref="Photo"/> и <see cref="Album"/>
    /// </summary>
    public static class Ids
    {
        /// <summary>
        ///     Ошибка, означающая наличие дупликата данного идентификатора.
        /// </summary>
        public static readonly ResultError DuplicatedId = new(
            "Ids.DuplicatedId",
            "Данный идентификатор уже есть"
        );

        /// <summary>
        ///     Ошибка, означающая отсутствие данного идентификатора.
        /// </summary>
        public static readonly ResultError IdNotFound = new(
            "Ids.IdNotFound",
            "Данный идентификатор не найден"
        );
    }

    /// <summary>
    ///     Ошибки для <see cref="Tag" />.
    /// </summary>
    public static class Tag
    {
        /// <summary>
        ///     Ошибка, когда имя тега пустое или состоит только из пробелов.
        /// </summary>
        public static readonly ResultError EmptyName = new(
            "Tag.EmptyName",
            "Имя тега не может быть пустым или состоять только из пробелов.");

        /// <summary>
        ///     Ошибка, когда имя тега превышает 50 символов.
        /// </summary>
        public static readonly ResultError TooLong = new(
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
        public static readonly ResultError EmptyPath = new(
            "Photo.EmptyPath",
            "Путь к файлу фотографии не может быть пустым.");

        /// <summary>
        ///     Ошибка, когда данный тег уже привязан к этой фотографии.
        /// </summary>
        public static readonly ResultError DuplicateTag = new(
            "Photo.DuplicateTag",
            "Данный тег уже привязан к этой фотографии.");

        /// <summary>
        ///     Ошибка, когда данный тег не существует в этой фотографии.
        /// </summary>
        public static readonly ResultError TagNotExists = new(
            "Photo.TagNotExists",
            "Данный тег не существует в этой фотографии.");

        /// <summary>
        ///     Ошибка, когда данная фотография не найдена.
        /// </summary>
        public static readonly ResultError NotFound = new(
            "Photo.NotFound",
            "Данная фотография не найдена");

        /// <summary>
        ///     Ошибка, когда данная фотография пустая.
        /// </summary>
        public static readonly ResultError NullPhoto = new(
            "Photo.NullPhoto",
            "Данная фотография пустая");
    }

    /// <summary>
    ///     Ошибки для <see cref="Album" />
    /// </summary>
    public static class Album
    {
        /// <summary>
        ///     Ошибка, когда имя альбома пустое.
        /// </summary>
        public static readonly ResultError EmptyName = new(
            "Album.EmptyName",
            "Имя альбома не может быть пустым.");

        /// <summary>
        ///     Ошибка, когда эта фотография уже находится в данном альбоме.
        /// </summary>
        public static readonly ResultError DuplicatePhoto = new(
            "Album.DuplicatePhoto",
            "Эта фотография уже находится в данном альбоме.");

        /// <summary>
        ///     Ошибка, когда альбом не найден.
        /// </summary>
        public static readonly ResultError NotFound = new(
            "Album.NotFound",
            "Альбом не найден.");

        /// <summary>
        ///     Ошибка, когда передан пустой альбом.
        /// </summary>
        public static readonly ResultError NullAlbum = new(
            "Album.NullAlbum",
            "Альбом не может быть null.");
    }
}