using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PhotoCatalog.Application.DTOs;
using PhotoCatalog.Application.DTOs.Folders;
using PhotoCatalog.Application.Fakes;
using PhotoCatalog.Application.UseCases;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Extensions;
using PhotoCatalog.Infrastructure.Fakes;

using Serilog;

try
{
    const string version = "v1";
    const string name = "PhotoCatalog";

    Log.Information("Запуск веб-хоста...");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHealthChecks();

    builder.Services.AddSingleton<IFolderRepository, FakeFolderRepository>();
    builder.Services.AddSingleton<IPhotoCommandRepository, FakePhotoRepository>();
    builder.Services.AddSingleton<IAlbumRepository, FakeAlbumRepository>();
    builder.Services.AddSingleton<ITagQueryRepository, FakeTagQueryRepository>();
    builder.Services.AddSingleton<ITagCommandRepository, FakeTagCommandRepository>();

    builder.Services.AddSingleton<IFileStorage, FakeFileStorage>();
    builder.Services.AddSingleton<IFileMetadataExtractor, FakeFileMetadataExtractor>();
    builder.Services.AddSingleton<IFolderHierarchyValidator, FakeFolderHierarchyValidator>();
    builder.Services.AddSingleton<IUnitOfWork, FakeUnitOfWork>();

    builder.Services.AddTransient<CreateFolderUseCase>();
    builder.Services.AddTransient<DeletePhotoUseCase>();
    builder.Services.AddTransient<AddTagToPhotoUseCase>();
    builder.Services.AddTransient<MoveFolderUseCase>();
    builder.Services.AddTransient<ImportPhotoUseCase>();
    builder.Services.AddTransient<AddPhotoToAlbumUseCase>();

    WebApplication app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();

        app.UseSwaggerUI(swagg =>
        {
            swagg.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{name}");
        });
    }

    app.MapGet("/test", () => "Hello World!");

    app.MapGroup("/api/tags");

    app.MapGet("/{id}", (int id) =>
    {
        FakeTagQueryRepository repository = new();
        Result<Tag> tag = repository.GetById(id);
        if (tag.IsSuccess)
        {
            return Results.Ok(tag);
        }

        return Results.NotFound();
    });

    app.MapPost("/{name}", (string name) =>
    {
        FakeTagQueryRepository repository = new();
        Result<Tag> tag = repository.GetByName(name);
        if (tag.IsSuccess)
        {
            return Results.Ok(tag);
        }

        return Results.NotFound();
    });

    app.MapDelete("/{id}", (int id) =>
    {
        FakeTagCommandRepository repository = new();
        ResultVoid tag = repository.Delete(id);
        if (tag.IsSuccess)
        {
            return Results.Ok(tag);
        }

        return Results.NotFound();
    });


    app.MapHealthChecks("/health");

    RouteGroupBuilder albumEndpointsGroup = app.MapGroup("/api/albums").WithTags("Альбомы");

    albumEndpointsGroup.MapGet("/{folderId:int}/albums", (int folderId, IAlbumRepository albumRepository) =>
        albumRepository
            .GetByFolderId(folderId)
            .ToHttpResult());

    albumEndpointsGroup.MapPost("/", (AlbumResponse album, IAlbumRepository albumRepository) => albumRepository
        .Add(Album.Create(album.Name, album.Id).Value!)
        .ToHttpResult());

    albumEndpointsGroup.MapPost("/{albumId:int}/photos/{photoId:int}",
        (int albumId, int photoId, IAlbumRepository albumRepository, IPhotoCRepository photoRepository) =>
        {
            Result<Photo> searchResult = photoRepository.GetById(photoId);

            if (searchResult.IsFailure)
            {
                return searchResult.Error.ToHttpResult();
            }

            return albumRepository
                .AddPhoto(albumId, photoId)
                .ToHttpResult();
        });

    albumEndpointsGroup.MapDelete("/{albumId:int}/photos/{photoId:int}",
        (int albumId, int photoId, IAlbumRepository albumRepository, IPhotoRepository photoRepository) =>
        {
            Result<Photo> searchResult = photoRepository.GetById(photoId);

            if (searchResult.IsFailure)
            {
                return searchResult.Error.ToHttpResult();
            }

            return albumRepository
                .DeletePhoto(albumId, photoId)
                .ToHttpResult();
        });

    albumEndpointsGroup.MapDelete("/{id:int}",
        (int id, IAlbumRepository albumRepository) => albumRepository.Delete(id).ToHttpResult());

    RouteGroupBuilder foldersGroup = app.MapGroup("/api/folders");

    foldersGroup.MapGet("/tree", (IFolderRepository folderRepository) =>
    {
        Result<IEnumerable<Folder>> result = folderRepository.GetAllFolders();
        return result.ToHttpResult();
    });

    foldersGroup.MapPost("/", (CreateFolderRequest request, CreateFolderUseCase useCase) =>
    {
        Result<FolderResponse> result = useCase.Execute(request);
        return result.ToHttpResult();
    });

    foldersGroup.MapPut("/{folderId:int}/move", (int folderId, MoveFolderRequest request, MoveFolderUseCase useCase) =>
    {
        ResultVoid result = useCase.Execute(folderId, request.NewParentId);
        return result.ToHttpResult();
    });

    foldersGroup.MapDelete("/{folderId:int}", (int folderId, IFolderRepository folderRepository) =>
    {
        ResultVoid result = folderRepository.Delete(folderId);
        return result.ToHttpResult();
    });

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Необработанное исключение.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}