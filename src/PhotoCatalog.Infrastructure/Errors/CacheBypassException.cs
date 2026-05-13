namespace PhotoCatalog.Infrastructure.Errors;

/// <summary>
///     Исключение, которое выбрасывается внутри фабричного метода <see cref="HybridCache"/>
///     при получении провального результата от внутреннего репозитория.
///     Используется для того, чтобы избежать сохранения ошибок в кэше.
/// </summary>
/// <remarks>
///     Декоратор <see cref="CachedFolderRepository"/> перехватывает это исключение
///     и возвращает исходный провальный <see cref="Result"/>.
/// </remarks>
public class CacheBypassException: Exception
{
    /// <summary>
    ///     Инициализирует новый экземпляр с сообщением по умолчанию.
    /// </summary>
    public CacheBypassException()
        : base("Результат операции является провальным – кэширование не выполняется.") { }

    /// <summary>
    ///     Инициализирует новый экземпляр с заданным сообщением.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    public CacheBypassException(string message) : base(message) { }
}