using Api;
using Application;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Identity;


public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
                await IdentityDataSeeder.SeedAllAsync(services);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred seeding the DB: {ex.Message}");
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