using Api;
using Application;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Context;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddJsonFile("appsettings.Local.json", optional: true);

        builder.Services
            .AddApiServices()
            .AddApplicationServices()
            .AddInfrastructureServices(builder.Configuration);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                if (context.Database.IsRelational())
                {
                    await context.Database.MigrateAsync();
                }

                await IdentityDataSeeder.SeedAllAsync(services);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during DB migration or seeding: {ex.Message}");
            }
        }

        app.UseApiMiddleware();

        app.MapGroup("/api/auth")
            .WithTags("Auth")
            .MapCustomIdentityApi<ApplicationUser>();

        app.MapControllers();

        app.Run();
    }
}