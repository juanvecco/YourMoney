using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using YourMoney.Api.Extensions;
using YourMoney.Infrastructure.Data;
using YourMoney.Api.Services;

namespace YourMoney.Api.Configuration
{
    public static class IdentityConfig
    {
        private const string GoogleLinkReturnPath = "/api/identidade/externo/vincular/google/retorno";

        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                DatabaseProviderConfig.Configure(options, configuration, "IdentityConnection"));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddErrorDescriber<IdentityMensagensPortugues>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // JWT
            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings"));

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            if (string.IsNullOrWhiteSpace(jwtSettings?.Secret))
                throw new Exception("JWT Secret não configurado!");

            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

            var authentication = services.AddAuthentication(options =>
                {
                    // AddIdentity registers its application cookie as the default
                    // scheme. API clients (Flutter and the Angular interceptor)
                    // authenticate with the JWT returned by /autenticar, so the
                    // API must explicitly prefer Bearer instead of redirecting an
                    // unauthenticated request to /Account/Login.
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        RoleClaimType = "role",
                        ValidIssuer = jwtSettings.Emissor,
                        ValidAudience = jwtSettings.ValidoEm
                    };
                });

            if (ExternalProviderConfiguration.GoogleAvailable(configuration))
            {
                var google = ExternalProviderConfiguration.Google(configuration);
                authentication.AddOpenIdConnect("google", options =>
                {
                    options.Authority = "https://accounts.google.com";
                    options.ClientId = google.ClientId;
                    options.ClientSecret = google.ClientSecret;
                    options.CallbackPath = "/signin-google";
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ResponseType = "code";
                    options.UsePkce = true;
                    options.SaveTokens = false;
                    options.GetClaimsFromUserInfoEndpoint = false;
                    options.MapInboundClaims = true;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
                    options.Events = ExternalEvents(configuration, options.CallbackPath);
                });
            }

            if (ExternalProviderConfiguration.AppleAvailable(configuration))
            {
                var apple = ExternalProviderConfiguration.Apple(configuration);
                authentication.AddOpenIdConnect("apple", options =>
                {
                    options.Authority = "https://appleid.apple.com";
                    options.ClientId = apple.ClientId;
                    options.CallbackPath = "/signin-apple";
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ResponseType = "code";
                    options.ResponseMode = "form_post";
                    options.UsePkce = false;
                    options.SaveTokens = false;
                    options.GetClaimsFromUserInfoEndpoint = false;
                    options.MapInboundClaims = true;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    options.Scope.Add("name");
                    options.Events = ExternalEvents(configuration, options.CallbackPath);
                    options.Events.OnAuthorizationCodeReceived = context =>
                    {
                        context.TokenEndpointRequest.ClientSecret = AppleClientSecret.Create(apple);
                        return Task.CompletedTask;
                    };
                });
            }

            return services;
        }

        private static OpenIdConnectEvents ExternalEvents(IConfiguration configuration, PathString callbackPath) => new()
        {
            OnAuthorizationCodeReceived = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("YourMoney.ExternalIdentity");
                logger.LogWarning("External identity authorization code received. CallbackPath={CallbackPath}", callbackPath.Value);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("YourMoney.ExternalIdentity");
                logger.LogWarning("External identity token validated. CallbackPath={CallbackPath}", callbackPath.Value);
                return Task.CompletedTask;
            },
            OnTicketReceived = context =>
            {
                var isAccountLinkFlow = context.Properties?.Items.TryGetValue("account_link", out var linkMode) == true
                    && linkMode == "true";
                if (isAccountLinkFlow)
                {
                    context.ReturnUri = GoogleLinkReturnPath;
                    if (context.Properties != null) context.Properties.RedirectUri = GoogleLinkReturnPath;
                }

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("YourMoney.ExternalIdentity");
                logger.LogWarning(
                    "External identity ticket received. CallbackPath={CallbackPath}; AccountLinkFlow={AccountLinkFlow}; DedicatedReturnSelected={DedicatedReturnSelected}",
                    callbackPath.Value,
                    isAccountLinkFlow,
                    string.Equals(context.ReturnUri, GoogleLinkReturnPath, StringComparison.Ordinal));
                return Task.CompletedTask;
            },
            OnRedirectToIdentityProvider = context =>
            {
                // Render terminates TLS before the app receives the request. Use the
                // configured public origin so the provider sees the registered HTTPS callback.
                var publicOrigin = configuration["Frontend:BaseUrl"]?.TrimEnd('/');
                if (string.IsNullOrWhiteSpace(publicOrigin) && configuration.GetValue<bool>("TLS_TERMINATED_BY_PROXY"))
                    publicOrigin = "https://" + context.Request.Host;
                if (!string.IsNullOrWhiteSpace(publicOrigin))
                    context.ProtocolMessage.RedirectUri = publicOrigin + callbackPath;
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("YourMoney.ExternalIdentity");
                logger.LogError(
                    "External identity remote failure. CallbackPath={CallbackPath}; FailureType={FailureType}",
                    callbackPath.Value,
                    context.Failure?.GetType().Name ?? "none");
                context.HandleResponse();
                if (context.Properties?.Items.TryGetValue("account_link", out var linkMode) == true && linkMode == "true")
                {
                    context.Response.Redirect("/configuracao?secao=seguranca&google=erro");
                    return Task.CompletedTask;
                }
                var loginOnly = context.Properties?.Items.TryGetValue("login_only", out var mode) == true && mode == "true";
                context.Response.Redirect(loginOnly ? "/login?externo=erro" : "/cadastro?externo=erro");
                return Task.CompletedTask;
            },
            OnAccessDenied = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("YourMoney.ExternalIdentity");
                logger.LogWarning("External identity access denied. CallbackPath={CallbackPath}", callbackPath.Value);
                context.HandleResponse();
                if (context.Properties?.Items.TryGetValue("account_link", out var linkMode) == true && linkMode == "true")
                {
                    context.Response.Redirect("/configuracao?secao=seguranca&google=cancelado");
                    return Task.CompletedTask;
                }
                var loginOnly = context.Properties?.Items.TryGetValue("login_only", out var mode) == true && mode == "true";
                context.Response.Redirect(loginOnly ? "/login?externo=erro" : "/cadastro?externo=erro");
                return Task.CompletedTask;
            }
        };

        public static IApplicationBuilder UseIdentityConfiguration(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}
