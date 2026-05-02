using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Interfaces.Repositories;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public class PhotoRepositoryMock : IPhotoRepository
{
    private readonly List<Photo> _photoTestList = new();

    public Photo? GetById(int id)
    {
        foreach (var photo in _photoTestList)
        {
            if (photo.Id == id)
            {
                return photo;
            }
        }

        return null;
    }

    public Photo? GetByPath(string realPath)
    {
        return _photoTestList.FirstOrDefault();
    }

    public void Add(Photo photo)
    {
        _photoTestList.Add(photo);
    }

    public void Update(Photo photo)
    {
        foreach (var photos in _photoTestList)
        {
            _photoTestList.Remove(photos);
            _photoTestList.Add(photo);
            return;
        }

        _photoTestList.Add(photo);
    }

    public void Delete(int id)
    {
        foreach (var photo in _photoTestList)
        {
            if (photo.Id == id)
            {
                _photoTestList.Remove(photo);
                return;
            }
        }
    }

    public IReadOnlyCollection<Photo> GetByAlbumId(int albumId)
    {
        return _photoTestList.AsReadOnly();
    }

    public IReadOnlyCollection<Photo> GetByTags(IEnumerable<int> tagIds)
    {
        return _photoTestList.AsReadOnly();
    }
}

public class PhotoRepositoryMockTests
{
    private readonly PhotoRepositoryMock _repo = new();


    [Fact]
    public void Add_WorksCorrectly()
    {
        var photo = new Photo(1, "test-tag");


        _repo.Add(photo);

        var result = _repo.GetById(1);
        Assert.NotNull(result);
        Assert.Equal("test-tag", result.Tag);
    }

    [Fact]
    public void GetById_FindsExisting()
    {
        _repo.Add(new Photo(42, "found"));

        var result = _repo.GetById(42);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
    }

    [Fact]
    public void GetById_NotFound_ReturnsNull()
    {
        var result = _repo.GetById(999);
        Assert.Null(result);
    }

    [Fact]
    public void Update_ReplacesExisting()
    {
        var original = new Photo(1, "old");
        var updated = new Photo(1, "new");
        _repo.Add(original);

        _repo.Update(updated);

        var result = _repo.GetById(1);
        Assert.Equal("new", result.Tag);
    }

    [Fact]
    public void Delete_RemovesExisting()
    {
        _repo.Add(new Photo(10, "to-delete"));

        _repo.Delete(10);

        Assert.Null(_repo.GetById(10));
    }

    [Fact]
    public void GetByAlbumId_ReturnsAll()
    {
        _repo.Add(new Photo(1, "a"));
        _repo.Add(new Photo(2, "b"));

        var result = _repo.GetByAlbumId(999);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetByTags_ReturnsAll()
    {
        _repo.Add(new Photo(1, "tag1"));

        var result = _repo.GetByTags(new[] { 100 });
        Assert.Single(result);
    }
}