using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace GMForce.Bricks.Initialization.Documentation;

[ExcludeFromCodeCoverage]
public static class SwaggerExtensions
{
    public static void AddSwaggerIn(this IServiceCollection services, IWebHostEnvironment environment, OpenApiSettings settings)
    {
        if (!environment.IsProduction())
        {
            _ = services.AddEndpointsApiExplorer();
            _ = services.AddSwaggerGen(options =>
            {
                settings.OnConfiguringSwagger(options);

                var apiDetails = settings.ApiDetails;
                options.SwaggerDoc(apiDetails.Version, new OpenApiInfo
                {
                    Version = apiDetails.Version,
                    Title = settings.Title,
                    Description = settings.Description,
                    Contact = new OpenApiContact
                    {
                        Name = apiDetails.Description,
                        Url = new Uri(settings.Location),
                        Email = settings.Email
                    }
                });

                var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
            _ = services.AddFluentValidationRulesToSwagger();
        }
    }

    public static void UseSwaggerIn(this IApplicationBuilder app, IWebHostEnvironment environment, params ApiDetails[] apiDetails)
    {
        if (!environment.IsProduction())
        {
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api";
                foreach (var apiDetail in apiDetails)
                {
                    c.SwaggerEndpoint($"/swagger/{apiDetail.Version}/swagger.json", apiDetail.Description);
                }
            });
        }
    }
}
