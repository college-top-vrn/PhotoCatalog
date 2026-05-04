namespace PhotoCatalog.Domain.Primitives;

/// <summary>
/// Реестр критических системных ошибок, которые не связаны с бизнес-правилами,
/// а возникают из-за сбоев в потоке выполнения или инфраструктуре.
/// </summary>
public static class SystemErrors
{
    /// <summary>
    ///     Ошибка, возникающая при получение null значение из объекта обернутого Result.
    /// </summary>
    public static readonly Error NullResult = new(
        "System.NullResult",
        "Получен пустой объект результата (null). Цепочка вычислений прервана.");

    /// <summary>
    ///     Ошибка, возникающая при попытке инициализировать успешный результат пустой ссылкой.
    /// </summary>
    public static readonly Error NullValue = new(
        "System.NullValue",
        "Попытка инициализировать успешный результат пустым значением (null).");
}