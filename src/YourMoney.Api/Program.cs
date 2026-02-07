using YourMoney.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddIdentityConfiguration(builder.Configuration)
    .AddDatabaseConfiguration(builder.Configuration, builder.Environment)
    .AddDependencyInjectionConfiguration()
    .AddApiConfiguration(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwaggerConfiguration();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}

app.UseApiConfiguration(app.Environment);

app.Run();
