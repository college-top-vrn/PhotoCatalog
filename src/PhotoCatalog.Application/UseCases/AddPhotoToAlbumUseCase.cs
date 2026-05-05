using System.Collections.Generic;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;
using Serilog.Events;

namespace PhotoCatalog.Application.UseCases;

public class AddPhotoToAlbumUseCase
{
    private IAlbumRepository _albumRepository;
    private IPhotoRepository _photoRepository;
    private IUnitOfWork _unitOfWork;
    private ILogger _logger;

    public AddPhotoToAlbumUseCase(IAlbumRepository albumRepository, IPhotoRepository photoRepository,IUnitOfWork unitOfWork, ILogger logger)
    {
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    
    public ResultVoid Execute(int albumId, int photoId)
    {
        var photo = _photoRepository.GetById(photoId);
        
        if (photo.IsFailure)
            return ResultVoid.Failure(ApplicationErrors.General.NotFound);
        
        var album = _albumRepository.GetById(albumId);
        
        if (album.IsFailure)
            return ResultVoid.Failure(ApplicationErrors.General.NotFound);

        var addResult = album.Value.AddPhoto(photoId);
        
        if (addResult.IsFailure)
            return ResultVoid.Failure(addResult.Error);
        
        
    }
}