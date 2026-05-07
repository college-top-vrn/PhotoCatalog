using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;

using Microsoft.Extensions.Logging;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Primitives;

using Serilog;

namespace PhotoCatalog.Application.UseCases;

public class DeletePhotoUseCase(
    IPhotoRepository photoRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ILogger<DeletePhotoUseCase> logger)
{
    private readonly IPhotoRepository _photoRepository = photoRepository;
    private readonly IFileStorage _fileStorage = fileStorage;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<DeletePhotoUseCase> _logger = logger;

    public ResultVoid Execute(int photoId)
    {
        var photo = _photoRepository.GetById(photoId)
            .ToResult(ApplicationErrors.General.NotFound)
            .Value;

        _unitOfWork.BeginTransaction();

        _photoRepository.Delete(photoId);

        if (_unitOfWork.Commit().IsSuccess)
        {
            _fileStorage.DeleteFile(photo!.Value!.RealPath);
            return ResultVoid.Success();
        }

        _logger.LogError("Orphaned file left on disk: {Path}", photo!.Value!.RealPath);

        return ResultVoid.Failure(ApplicationErrors);
    }
}