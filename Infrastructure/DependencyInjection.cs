using Application.Common.Interfaces;
using Domain.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Infrastructure.Context;
using Infrastructure.Options;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            services.AddScoped<IFileService, FileService>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection("DatabaseOptions"))
                .ValidateDataAnnotations()
                .ValidateOnStart();



            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var dbOptions = sp
                .GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

                options.UseNpgsql(dbOptions.ConnectionString);
            });

            services.AddScoped<IApplicationDbContext>(provider =>
              provider.GetRequiredService<ApplicationDbContext>());

            services.ConfigureOptions<IdentityOptionsSetup>();

            services.AddIdentityApiEndpoints<ApplicationUser>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }

    }
}