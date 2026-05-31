using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PhotoCatalog_WebApi>("photocatalog-webapi");

builder.Build().Run();