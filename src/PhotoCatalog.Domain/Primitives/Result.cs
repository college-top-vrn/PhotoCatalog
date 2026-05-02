namespace PhotoCatalog.Domain.Primitives;

/// <summary>
/// Представляет результат выполнения операции, возвращающей значение типа <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Тип возвращаемого значения.</typeparam>
/// <remarks>
/// ВНИМАНИЕ: Не создавайте объект через конструктор по умолчанию.
/// Используйте неявное приведение типов (return value; / return error;) или фабрики Success/Failure.
/// </remarks>
public class ResultVoid<T>
{
    private readonly T? _value;

    /// <summary>
    /// Указывает, завершилась ли операция успешно.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Указывает, завершилась ли операция с ошибкой.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Объект ошибки. Если операция успешна, содержит <see cref="Primitives.Error.None"/>.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Возвращает результат операции. 
    /// Если операция завершилась провалом (<see cref="IsFailure"/> равно true), возвращает значение по умолчанию (default/null).
    /// </summary>
    public T? Value => _value;


    /// <summary>
    /// Инициализирует внутреннее состояние объекта.
    /// </summary>
    /// <param name="value"> Результат операции. Имеет значение по умолчанию, если операция провалена.</param>
    /// <param name="isSuccess">Флаг, указывающий на успешное завершение операции.</param>
    /// <param name="error">Детализированная бизнес-ошибка. Равна <see cref="Primitives.Error.None"/> при успехе.</param>
    private ResultVoid(T? value, bool isSuccess, Error error)
    {
        _value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Создает успешный результат с переданным значением.
    /// </summary>
    public static ResultVoid<T> Success(T value) => new(value, true, Error.None);

    /// <summary>
    /// Создает провальный результат с указанной ошибкой и значением по умолчанию.
    /// </summary>
    public static ResultVoid<T> Failure(Error error) => new(default, false, error);


    /// <summary>
    /// Неявно преобразует значение в успешный результат.
    /// </summary>
    public static implicit operator ResultVoid<T>(T value) => Success(value);

    /// <summary>
    /// Неявно преобразует ошибку в провальный результат.
    /// </summary>
    public static implicit operator ResultVoid<T>(Error error) => Failure(error);

    /// <summary>
    /// Неявно преобразует ResultVoid обобщенного типа в базовый ResultVoid.
    /// Обеспечивает статическую диспетчеризацию (полиморфизм) на этапе компиляции.
    /// </summary>
    public static implicit operator ResultVoid(ResultVoid<T> resultVoid) =>
        resultVoid.IsSuccess ? ResultVoid.Success() : ResultVoid.Failure(resultVoid.Error);
}