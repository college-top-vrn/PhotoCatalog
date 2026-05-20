using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PhotoCatalog.Application.DTOs;
using PhotoCatalog.Application.Errors;
using PhotoCatalog.Application.Fakes;
using PhotoCatalog.Application.UseCases;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
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

    builder.Services.AddSingleton<FakeDatabase>();
    builder.Services.AddSingleton<IPhotoCommandRepository, FakePhotoCommandRepository>();
    builder.Services.AddSingleton<IPhotoQueryRepository, FakePhotoQueryRepository>();
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

        app.UseSwaggerUI(swagger =>
        {
            swagger.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{name}");
        });
    }

    app.MapGet("/test", () => "Hello World!");

    app.MapGroup("/api/tags");

    app.MapGet("/{id:int}", (int id) =>
    {
        FakeTagQueryRepository repository = new();
        Result<Tag> tag = repository.GetById(id);
        return tag.IsSuccess
            ? Results.Ok(tag)
            : Results.NotFound();
    });

    app.MapPost("/", () =>
    {
        FakeTagQueryRepository repository = new();
        Result<Tag> tag = repository.GetByName(name);
        return tag.IsSuccess
            ? Results.Ok(tag)
            : Results.NotFound();
    });

    app.MapDelete("/{id:int}", (int id) =>
    {
        FakeTagCommandRepository repository = new();
        ResultVoid tag = repository.Delete(id);
        return tag.IsSuccess
            ? Results.Ok(tag)
            : Results.NotFound();
    });

    app.MapHealthChecks("/health");

    RouteGroupBuilder photosGroup = app.MapGroup("/api/photos").WithTags("Фотографии");

    photosGroup.MapGet("/", (IPhotoQueryRepository photoQuery) =>
    {
        Result<IEnumerable<Photo>> result = photoQuery.GetAll();
        return result.ToHttpResult();
    });

    photosGroup.MapPost("/import", (HttpRequest request, ImportPhotoUseCase importPhotoUseCase) =>
    {
        if (!request.HasFormContentType)
        {
            return ApplicationErrors.Http.InvalidMultipartRequest
                .ToResult()
                .ToHttpResult();
        }

        var file = request.Form.Files.GetFile("file");

        if (file == null || file.Length == 0)
        {
            return ApplicationErrors.Http.FileNotUploaded
                .ToResult()
                .ToHttpResult();
        }

        var tempFilePath = Path.GetTempFileName();

        try
        {
            using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(stream);
            }

            var importRequest = new ImportPhotoRequest(tempFilePath);

            return importPhotoUseCase.Execute(importRequest)
                .ToResult()
                .ToHttpResult();
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    });

    RouteGroupBuilder albumEndpointsGroup = app.MapGroup("/api/albums").WithTags("Альбомы");

    // albumEndpointsGroup.MapGet("/{folderId:int}/albums", (int folderId, IAlbumRepository albumRepository) =>
    //     albumRepository
    //         .GetByFolderId(folderId)
    //         .ToHttpResult());
    //
    // albumEndpointsGroup.MapPost("/", (AlbumResponse album, IAlbumRepository albumRepository) => albumRepository
    //     .Add(Album.Create(album.Name, album.Id).Value!)
    //     .ToHttpResult());
    //
    // albumEndpointsGroup.MapPost("/{albumId:int}/photos/{photoId:int}",
    //     (int albumId, int photoId, IAlbumRepository albumRepository, IPhotoCRepository photoRepository) =>
    //     {
    //         Result<Photo> searchResult = photoRepository.GetById(photoId);
    //
    //         if (searchResult.IsFailure)
    //         {
    //             return searchResult.Error.ToHttpResult();
    //         }
    //
    //         return albumRepository
    //             .AddPhoto(albumId, photoId)
    //             .ToHttpResult();
    //     });
    //
    // albumEndpointsGroup.MapDelete("/{albumId:int}/photos/{photoId:int}",
    //     (int albumId, int photoId, IAlbumRepository albumRepository, IPhotoRepository photoRepository) =>
    //     {
    //         Result<Photo> searchResult = photoRepository.GetById(photoId);
    //
    //         if (searchResult.IsFailure)
    //         {
    //             return searchResult.Error.ToHttpResult();
    //         }
    //
    //         return albumRepository
    //             .DeletePhoto(albumId, photoId)
    //             .ToHttpResult();
    //     });

    albumEndpointsGroup.MapDelete("/{id:int}",
        (int id, IAlbumRepository albumRepository) => albumRepository.Delete(id).ToHttpResult());

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