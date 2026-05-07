using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.UseCases;

public class ImportPhotoUseCase(
    IPhotoRepository photoRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ILogger<ImportPhotoUseCase> logger)
{
    public Result<PhotoResponse> Execute(ImportPhotoRequest request)
    {
        
    }
}