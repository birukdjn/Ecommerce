using Application.Common.Interfaces;
using Application.Interfaces;
using Domain.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Identity;
using Infrastructure.Options;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();


            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ISmsSender, SmsSender>();
            services.AddHttpClient("AfroSms", client =>
            {
                client.BaseAddress = new Uri("https://api.afromessage.com/");
            });

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Configure options
            services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(DatabaseOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<AfroSmsOptions>()
                .Bind(configuration.GetSection(AfroSmsOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.ConfigureOptions<IdentityOptionsSetup>();




            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var dbOptions = sp
                .GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

                options.UseNpgsql(dbOptions.ConnectionString);
            });

            services.AddScoped<IApplicationDbContext>(provider =>
              provider.GetRequiredService<ApplicationDbContext>());


            services.AddIdentityApiEndpoints<ApplicationUser>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }

    }
}