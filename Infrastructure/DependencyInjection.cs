using Application.Interfaces;
using Domain.Entities;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.BackgroundJobs;
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
using Stripe;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            var databaseSection = configuration.GetSection(DatabaseOptions.SectionName);
            var smsSection = configuration.GetSection(AfroSmsOptions.SectionName);
            var emailSection = configuration.GetSection(EmailOptions.SectionName);
            var connectionString = databaseSection["ConnectionString"];
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
            services.Configure<StripeOptions>(configuration.GetSection("Stripe"));

            // 1. Register Hangfire with Postgres
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(connectionString),
                    new PostgreSqlStorageOptions
                    {
                        SchemaName = "hangfire"
                    }));

            // 2. Add the background worker server
            services.AddHangfireServer();


            services.AddScoped<IStripeService, StripeService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IInventoryNotificationService, InventoryNotificationService>();
            services.AddScoped<ISmsSender, SmsSender>();
            services.AddScoped<IJobService, HangfireJobService>();

            services.AddHttpClient("AfroSms", (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<AfroSmsOptions>>().Value;

                    client.BaseAddress = new Uri(options.BaseUrl);
                });

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetConnectionString("Redis");
                    options.InstanceName = "Ecommerce_";
                });

            // Configure options
            services.AddOptions<DatabaseOptions>()
                .Bind(databaseSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<AfroSmsOptions>()
                .Bind(smsSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<EmailOptions>()
                .Bind(emailSection)
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