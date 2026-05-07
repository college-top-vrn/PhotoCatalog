namespace PhotoCatalog.Application.DTOs;

/// <summary>
///     DTO record для запроса импорта фото.
/// </summary>
/// <param name="SourcePath">Путь к файлу.</param>
public record ImportPhotoRequest(string SourcePath);