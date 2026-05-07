using System;
using System.Collections.Generic;

namespace PhotoCatalog.Application.DTOs;

/// <summary>
///     DTO record для ответа с фото.
/// </summary>
/// <param name="Id">Идентификатор фото.</param>
/// <param name="RealPath">Путь к файлу в файловой системе.</param>
/// <param name="FileHash">Хеш-сумма файла.</param>
/// <param name="Width">Ширина фото.</param>
/// <param name="Height">Высота фото.</param>
/// <param name="AddedAt">Дата добавления.</param>
/// <param name="TagIds">
///     Список из идентификаторов тегов,
///     доступный только для чтения.
/// </param>
public record PhotoResponse(
    int Id,
    string RealPath,
    string? FileHash,
    int Width,
    int Height,
    DateTime AddedAt,
    IReadOnlyCollection<int> TagIds);