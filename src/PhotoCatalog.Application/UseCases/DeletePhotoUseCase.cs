using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;

using Microsoft.Extensions.Logging;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Primitives;

using Serilog;

namespace PhotoCatalog.Application.UseCases;

public class DeletePhotoUseCase(IPhotoRepository photoRepository, IFileStorage fileStorage, IUnitOfWork unitOfWork, ILogger<DeletePhotoUseCase> logger)
{
    private readonly IPhotoRepository _photoRepository = photoRepository;
    private readonly IFileStorage _fileStorage = fileStorage;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<DeletePhotoUseCase> _logger = logger;

    public ResultVoid Execute(int photoId)
    {
        return _photoRepository
            .GetById(photoId)
            .ToResult(ApplicationErrors.General.NotFound)
            .Then(photo => _unitOfWork.BeginTransaction())
            .Then(() => _photoRepository.Delete(photoId))
            .Then(() => _unitOfWork.Commit()).ToResult(new Error("Тест", "Тест"));
    }
}