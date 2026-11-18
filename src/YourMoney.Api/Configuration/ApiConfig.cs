using System.Net;
using System.Text.RegularExpressions;
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

            // Render terminates TLS and redirects HTTP at its edge.
            if (!app.ApplicationServices.GetRequiredService<IConfiguration>()
                    .GetValue<bool>("TLS_TERMINATED_BY_PROXY"))
                app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    var name = context.File.Name;
                    context.Context.Response.Headers.CacheControl = name is "index.html" or "ngsw.json" or "ngsw-worker.js"
                        ? "no-cache"
                        : Regex.IsMatch(name, @"-[A-Z0-9]{8,}\.(js|css|woff2?)$", RegexOptions.IgnoreCase)
                            ? "public, max-age=31536000, immutable"
                            : "public, max-age=86400";
                }
            });
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

                    context.Response.Headers.CacheControl = "no-cache";
                    await context.Response.SendFileAsync(indexPath);
                });
            });

            return app;
        }
    }
}
