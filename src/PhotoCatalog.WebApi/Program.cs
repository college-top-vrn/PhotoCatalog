using System;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PhotoCatalog.Application.DTOs;
using PhotoCatalog.Application.Fakes;
using PhotoCatalog.Application.UseCases;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Extensions;
using PhotoCatalog.Infrastructure.Fakes;
using PhotoCatalog.Infrastructure.Services;

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
    builder.Services.AddSingleton<IThumbnailService, ThumbnailService>();

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

    app.MapGet("/api/photos/{id:int}/thumbnail",
            (int id, IPhotoQueryRepository photoRepository, IFileStorage fileStorage) =>
            {
                var photoResult = photoRepository.GetById(id);

                if (photoResult.IsFailure)
                {
                    return photoResult.Error.Code == DomainErrors.Photo.NotFound.Code
                        ? Results.NotFound()
                        : Results.StatusCode(500);
                }

                var photo = photoResult.Value;
                var directory = Path.GetDirectoryName(photo.RealPath);
                var fileName = Path.GetFileNameWithoutExtension(photo.RealPath);
                var extension = Path.GetExtension(photo.RealPath);
                var thumbnailPath =
                    Path.Combine(directory ?? string.Empty, ".thumbnails", $"{fileName}_thumb{extension}");

                var existsResult = fileStorage.FileExists(thumbnailPath);

                return existsResult.IsSuccess && existsResult.Value
                    ? Results.File(thumbnailPath, "image/jpeg")
                    : Results.File(photo.RealPath, "image/jpeg");
            })
        .WithTags("Фотографии");

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