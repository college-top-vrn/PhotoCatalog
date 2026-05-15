using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PhotoCatalog.Application.Fakes;
using PhotoCatalog.Application.UseCases;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
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
    builder.Services.AddSingleton<IPhotoRepository, FakePhotoRepository>();
    builder.Services.AddSingleton<IAlbumRepository, FakeAlbumRepository>();
    builder.Services.AddSingleton<ITagRepository, FakeTagRepository>();

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
        FakeTagRepository repository = new FakeTagRepository();
        var tag = repository.GetById(id);
        if (tag.IsSuccess)
        {
            return Results.Ok(tag);
        }

        return Results.NotFound();
    });

    app.MapPost("/{name}", (string name) =>
    {
        FakeTagRepository repository = new FakeTagRepository();
        var tag = repository.GetByName(name);
        if (tag.IsSuccess)
        {
            return Results.Ok(tag);
        }

        return Results.NotFound();
    });

    app.MapDelete("/{id}", (int id) =>
    {
        FakeTagRepository repository = new FakeTagRepository();
        var tag = repository.Delete(id);
        if (tag.IsSuccess)
        {
            return Results.Ok(tag);
        }

        return Results.NotFound();
    });


    app.MapHealthChecks("/health");
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