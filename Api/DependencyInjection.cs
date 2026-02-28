using Api.Middleware;
using Domain.Constants;
using Microsoft.OpenApi;

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

            services.AddControllers();
            return services;



        }

        public static WebApplication UseApiMiddleware(this WebApplication app)
        {

            app.UseExceptionHandler();


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

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
