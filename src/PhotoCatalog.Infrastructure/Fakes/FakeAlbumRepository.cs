using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

public class FakeAlbumRepository : IAlbumRepository
{
    private readonly Dictionary<int, Album> _storage = []; 
    
    public Result<Album> GetById(int id)
    {
        throw new System.NotImplementedException();
    }

    public ResultVoid Add(Album album)
    {
        throw new System.NotImplementedException();
    }

    public ResultVoid Update(Album album)
    {
        throw new System.NotImplementedException();
    }

    public ResultVoid Delete(int id)
    {
        throw new System.NotImplementedException();
    }
}