using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace YourMoney.Api.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? new[] { "http://localhost:4200", "http://127.0.0.1:4200" };

                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            return services;
        }

        public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeadersOptions.KnownProxies.Add(IPAddress.Loopback);
            forwardedHeadersOptions.KnownProxies.Add(IPAddress.IPv6Loopback);
            app.UseForwardedHeaders(forwardedHeadersOptions);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            var objectiveImagesPath = Path.Combine(env.ContentRootPath, "App_Data", "ObjectiveImages");
            Directory.CreateDirectory(objectiveImagesPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(objectiveImagesPath),
                RequestPath = "/uploads/objetivos"
            });
            app.UseRouting();
            app.UseCors();

            app.UseIdentityConfiguration();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallback(async context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            code = "api_route_not_found",
                            message = "A rota da API não foi encontrada. Atualize o servidor local e tente novamente."
                        });
                        return;
                    }

                    var indexPath = Path.Combine(env.ContentRootPath, "wwwroot", "index.html");
                    if (!File.Exists(indexPath))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    await context.Response.SendFileAsync(indexPath);
                });
            });

            return app;
        }
    }
}
