using System.Text.Json;
using Api.Middleware;
using Application.Interfaces;
using Domain.Constants;
using Hangfire;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Api.Filters;

namespace Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddAuthentication().AddBearerToken();
            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin))
                .AddPolicy("VendorOnly", policy => policy.RequireRole(Roles.Vendor))
                .AddPolicy("ActiveVendorOnly", policy => policy.RequireRole(Roles.Vendor).RequireClaim("IsActive", "true"));

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "E-Commerce Multi-Vendor API",
                    Version = "v1",
                    Description = "Clean Architecture implementation with MediatR and EF Core"
                });

                options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Name = "Authorization",
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement

                {

                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });
            });




            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Policy specifically for SMS/OTP endpoints
                options.AddPolicy("SmsPolicy", httpContext =>
                {
                    var identity = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(identity, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    });
                });
            });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
            return services;



        }

        public static WebApplication UseApiMiddleware(this WebApplication app)
        {

            app.UseExceptionHandler();
            app.UseHttpsRedirection();


            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger(options =>
                {
                    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
                });
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce Multi-Vendor API");
                });
            }

            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[]
                   {
                        new HangfireAuthorizationFilter("AdminOnly")
                    }
            });

            using (var scope = app.Services.CreateScope())
            {
                var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();

                jobService.AddOrUpdateRecurring<IInventoryNotificationService>(
                    "low-stock-check",
                    service => service.CheckLowStockAndNotifyVendors(),
                    Cron.Daily(8)
               );
            }
            return app;
        }
    }
}
