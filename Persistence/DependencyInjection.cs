using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persistence.Context;
using Persistence.Identity;
using Persistence.Options;


namespace Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
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
            services.ConfigureOptions<IdentityOptionsSetup>();

            services.AddIdentityApiEndpoints<ApplicationUser>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }

    }
}