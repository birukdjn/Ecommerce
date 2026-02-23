using Api;
using Application;
using Domain.Entities;
using Infrastructure;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddApiServices()
    .AddApplicationServices()
    .AddInfrastructureServices()
    .AddPersistenceServices(builder.Configuration);


var app = builder.Build();

app.UseApiMiddleware();

app.MapGroup("/api/auth")
    .WithTags("Auth")
    .MapIdentityApi<ApplicationUser>();

app.MapControllers();

app.Run();
