using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects;

/// <summary>
///     ValueObject, представляющий собой HEX-код цвета.
/// </summary>
public record ColorHex
{
    /// <summary>
    ///     Значение HEX-кода.
    /// </summary>
    public string Value { get; }

    private ColorHex(string value) => Value = value;

    /// <summary>
    ///     Создаёт новый HEX-код.
    /// </summary>
    /// <param name="value">значение HEX-кода.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с новым экземпляром HEX-кодом.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<ColorHex> Create(string value)
    {
        return Result.Success(new ColorHex(value));
    }
}