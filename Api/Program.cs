using Api;
using Application;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Identity;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddApiServices()
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);
    


var app = builder.Build();

app.UseApiMiddleware();

app.MapGroup("/api/auth")
    .WithTags("Auth")
    .MapCustomIdentityApi<ApplicationUser>();

app.MapControllers();

app.Run();
