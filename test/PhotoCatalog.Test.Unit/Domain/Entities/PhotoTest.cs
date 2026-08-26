// using System;
// using System.Linq;
//
// using PhotoCatalog.Domain.Entities;
// using PhotoCatalog.Domain.Primitives;
// using PhotoCatalog.Domain.ValueObjects.Photo;
//
// using Xunit;
//
// namespace PhotoCatalog.Test.Unit.Domain.Entities;
//
// public class PhotoTest
// {
//     [Fact]
//     public void Create_CreatingPhotoWithCorrectValues_ReturnsSuccessWithPhoto()
//     {
//         Metadata.Builder builder = new();
//
//         Result<Photo> result = Photo
//             .Create(
//                 Guid.CreateVersion7(),
//                 Guid.CreateVersion7(),
//                 Size.Create(100).Value!,
//                 Mime.Create("mime").Value!,
//                 StorageKey.Create("photo").Value!,
//                 builder.Build().Value!,
//                 [
//                     Guid.CreateVersion7(),
//                     Guid.CreateVersion7()
//                 ]
//             );
//
//         Photo photo = result.Value!;
//
//         Assert.True(result.IsSuccess);
//         Assert.NotEmpty(photo.TagIds);
//     }
//
//     [Fact]
//     public void DeepCopy_DeeplyCopyingOriginalPhoto_ReturnsPhotoCopy()
//     {
//         Metadata.Builder builder = new();
//
//         Photo original = Photo.Create(
//             Guid.CreateVersion7(),
//             Guid.CreateVersion7(),
//             Size.Create(100).Value!,
//             Mime.Create("mime").Value!,
//             StorageKey.Create("photo").Value!,
//             builder.Build().Value!,
//             [
//                 Guid.CreateVersion7(),
//                 Guid.CreateVersion7()
//             ]
//         ).Value!;
//
//         Photo copy = original.DeepCopy();
//
//         Assert.Equal(original.Id, copy.Id);
//         Assert.Equal(original.UserId, copy.UserId);
//         Assert.Equal(original.Size, copy.Size);
//         Assert.Equal(original.Mime, copy.Mime);
//         Assert.Equal(original.StorageKey, copy.StorageKey);
//         Assert.True(original.TagIds.SequenceEqual(copy.TagIds));
//         Assert.NotSame(original.TagIds, copy.TagIds);
//     }
//
//     [Fact]
//     public void DeepCopy_ChangingPhotoCopyWithoutAffectingOriginalPhoto()
//     {
//         Metadata.Builder builder = new();
//
//         Photo original = Photo.Create(
//             Guid.CreateVersion7(),
//             Guid.CreateVersion7(),
//             Size.Create(100).Value!,
//             Mime.Create("mime").Value!,
//             StorageKey.Create("photo").Value!,
//             builder.Build().Value!,
//             [
//                 Guid.CreateVersion7(),
//                 Guid.CreateVersion7()
//             ]
//         ).Value!;
//
//         Photo copy = original.DeepCopy();
//
//         copy.Resize(Size.Create(200).Value!);
//         copy.ChangeMime(Mime.Create("mime2").Value!);
//         copy.ChangeStorageKey(StorageKey.Create("photo2").Value!);
//         copy.ChangeMetadata(builder.SetHasHdr(true).SetIsPanorama(true).Build().Value!);
//         copy.AddTag(Guid.CreateVersion7());
//
//         Assert.NotEqual(original.Size, copy.Size);
//         Assert.NotEqual(original.Mime, copy.Mime);
//         Assert.NotEqual(original.StorageKey, copy.StorageKey);
//         Assert.False(original.TagIds.SequenceEqual(copy.TagIds));
//     }
//
//     [Fact]
//     public void AddTag_AddingUniqueTagId_ReturnsSuccess()
//     {
//         Metadata.Builder builder = new();
//
//         Photo photo = Photo.Create(
//             Guid.CreateVersion7(),
//             Guid.CreateVersion7(),
//             Size.Create(100).Value!,
//             Mime.Create("mime").Value!,
//             StorageKey.Create("photo").Value!,
//             builder.Build().Value!,
//             []
//         ).Value!;
//
//         Guid tagId = Guid.CreateVersion7();
//
//         ResultVoid addResult = photo.AddTag(tagId);
//
//         Assert.True(addResult.IsSuccess);
//         Assert.Equal(photo.TagIds.FirstOrDefault(tagId), tagId);
//     }
//
//     [Fact]
//     public void AddTag_AddingSimilarTagId_ReturnsFailureWithError()
//     {
//         Metadata.Builder builder = new();
//
//         Guid tagId = Guid.CreateVersion7();
//
//         Photo photo = Photo.Create(
//             Guid.CreateVersion7(),
//             Guid.CreateVersion7(),
//             Size.Create(100).Value!,
//             Mime.Create("mime").Value!,
//             StorageKey.Create("photo").Value!,
//             builder.Build().Value!,
//             [tagId]
//         ).Value!;
//
//         ResultVoid addResult = photo.AddTag(tagId);
//
//         Assert.True(addResult.IsFailure);
//         Assert.Equal(addResult.ResultError, DomainErrors.Ids.DuplicatedId);
//         Assert.Single(photo.TagIds);
//     }
//
//     [Fact]
//     public void DeleteTag_DeletingExistingTagId_ReturnsSuccess()
//     {
//         Metadata.Builder builder = new();
//
//         Guid tagId = Guid.CreateVersion7();
//
//         Photo photo = Photo
//             .Create(
//                 Guid.CreateVersion7(),
//                 Guid.CreateVersion7(),
//                 Size.Create(100).Value!,
//                 Mime.Create("mime").Value!,
//                 StorageKey.Create("photo").Value!,
//                 builder.Build().Value!,
//                 [tagId]
//             ).Value!;
//
//         ResultVoid deleteResult = photo.DeleteTag(tagId);
//
//         Assert.True(deleteResult.IsSuccess);
//         Assert.Empty(photo.TagIds);
//     }
//
//     [Fact]
//     public void DeletePhoto_DeletingNonExistingPhotoId_ReturnsFailureWithError()
//     {
//         Metadata.Builder builder = new();
//
//         Photo photo = Photo.Create(
//             Guid.CreateVersion7(),
//             Guid.CreateVersion7(),
//             Size.Create(100).Value!,
//             Mime.Create("mime").Value!,
//             StorageKey.Create("photo").Value!,
//             builder.Build().Value!,
//             []
//         ).Value!;
//
//         Guid tagId = Guid.CreateVersion7();
//
//         ResultVoid deleteResult = photo.DeleteTag(tagId);
//
//         Assert.True(deleteResult.IsFailure);
//         Assert.Equal(deleteResult.ResultError, DomainErrors.Ids.IdNotFound);
//     }
// }