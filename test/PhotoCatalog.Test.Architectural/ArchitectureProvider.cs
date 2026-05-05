using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET;

using System;

using ArchUnitNET.Fluent.Syntax.Elements.Types.Classes;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

using Xunit;

namespace PhotoCatalog.Test.Architectural;

using ArchUnitNET;

public static class ArchitectureProvider
{
    public static readonly ArchUnitNET.Domain.Architecture ApplicationLayer =
        new ArchLoader()
            .LoadAssemblies(
                typeof(ApplicationErrors).Assembly)
            .Build();

    public static readonly ArchUnitNET.Domain.Architecture DomainLayer =
        new ArchLoader()
            .LoadAssemblies(typeof(Album).Assembly,
                typeof(Folder).Assembly,
                typeof(Photo).Assembly,
                typeof(Tag).Assembly,
                typeof(ResultExtensions).Assembly,
                typeof(IAlbumRepository).Assembly,
                typeof(IFolderRepository).Assembly,
                typeof(IPhotoRepository).Assembly,
                typeof(ITagRepository).Assembly,
                typeof(IFileMetadataExtractor).Assembly,
                typeof(IFileStorage).Assembly,
                typeof(DomainErrors).Assembly,
                typeof(Error).Assembly,
                typeof(Result<>).Assembly,
                typeof(ResultVoid).Assembly,
                typeof(SystemErrors).Assembly,
                typeof(Dimensions).Assembly
                )
            .Build();
    
    [Fact]
    public static void Domain_ShouldNot_HaveDependencies_OnOtherLayers()
    {
    }
}