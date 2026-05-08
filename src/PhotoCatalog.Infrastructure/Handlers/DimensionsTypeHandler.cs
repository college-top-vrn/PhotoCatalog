using System.Data;

using Dapper;

using PhotoCatalog.Domain.ValueObjects;

namespace PhotoCatalog.Infrastructure.Handlers;

/// <summary>
///     Обработчик типа Dapper для
///     value object <see cref="PhotoCatalog.Domain.ValueObjects.Dimensions" />.
///     Обеспечивает двустороннее преобразование между строкой
///     в формате "ширина×высота" (например, "1920x1080")
///     и объектом <see cref="Dimensions" /> при чтении/записи в базу данных.
/// </summary>
/// <remarks>
///     <para>
///         <b>Формат хранения:</b> в колонке БД хранится строка вида <c>"1920x1080"</c>.
///         Разделитель — символ 'x' (строчная латинская буква X).
///         Ширина и высота — положительные целые числа.
///     </para>
/// </remarks>
public class DimensionsTypeHandler : SqlMapper.TypeHandler<Dimensions>
{
    /// <summary>
    ///     Преобразует объект <see cref="Dimensions" /> в значение для сохранения в базе данных (SQLite).
    /// </summary>
    /// <param name="parameter">Параметр команды, в который будет записано значение.</param>
    /// <param name="value">>Экземпляр <see cref="Dimensions" />, подлежащий сериализации.</param>
    public override void SetValue(IDbDataParameter parameter, Dimensions? value)
    {
        if (value is null)
        {
            parameter.Value = DBNull.Value;
        }
        else
        {
            parameter.Value = $"{value.Width}x{value.Height}";
        }
    }

    /// <summary>
    ///     Преобразует значение из базы данных в объект <see cref="Dimensions" />.
    /// </summary>
    /// <param name="value">
    ///     Значение, полученное из колонки БД (ожидается строка формата "ширина×высота").
    /// </param>
    /// <returns>Экземпляр <see cref="Dimensions" /> с соответствующими шириной и высотой.</returns>
    /// <exception cref="FormatException">
    ///     Выбрасывается, если строка имеет неверный формат или не может быть разобрана.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Выбрасывается, если полученные из строки значения не проходят проверку инвариантов
    ///     (ширина или высота ≤ 0). Такая ситуация должна трактоваться как повреждение данных.
    /// </exception>
    public override Dimensions? Parse(object value)
    {
        if (value is not string stringValue || string.IsNullOrWhiteSpace(stringValue))
        {
            throw new FormatException(
                $"Невозможно преобразовать значение '{value}' в Dimensions. Ожидается строка формата 'ширина×высота'.");
        }

        string[] parts = stringValue.Split('x');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int width) ||
            !int.TryParse(parts[1], out int height))
        {
            throw new FormatException(
                $"Строка '{stringValue}' не соответствует формату Dimensions ('ширина×высота', например '1920x1080').");
        }

        return Dimensions.Create(width, height).Value;
    }
}