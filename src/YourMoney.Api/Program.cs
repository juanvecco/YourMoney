using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using YourMoney.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ============= CONNECTION STRING - Funciona no Railway + Local + Docker =============
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string não encontrada! Configure a variável de ambiente 'DATABASE_CONNECTION_STRING' ou o appsettings.json");

Console.WriteLine("=== CONNECTION STRING EM USO ===");
Console.WriteLine(connectionString);
Console.WriteLine("=====================================");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// ====================================================================================

// ============= INJEÇÃO DE DEPENDÊNCIA =============
builder.Services.AddScoped<IDespesaService, DespesaService>();
builder.Services.AddScoped<IDespesaRepository, DespesaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IReceitaRepository, ReceitaRepository>();
builder.Services.AddScoped<IReceitaService, ReceitaService>();
builder.Services.AddScoped<IInvestimentoRepository, InvestimentoRepository>();
builder.Services.AddScoped<IInvestimentoService, InvestimentoService>();
builder.Services.AddScoped<IMetaRepository, MetaRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IContaFinanceiraService, ContaFinanceiraService>();
builder.Services.AddScoped<IContaFinanceiraRepository, ContaFinanceiraRepository>();
// ===================================================

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "YourMoney API",
        Version = "v1",
        Description = "API para controle de orçamento familiar"
    });
});

// CORS (temporariamente aberto - depois aperta)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Swagger só em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "YourMoney API V1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// ============= APLICA MIGRAÇÕES AUTOMATICAMENTE =============
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Console.WriteLine("Aplicando migrações no banco de dados...");
    db.Database.Migrate();
    Console.WriteLine("Migrações aplicadas com sucesso!");
}
// ===========================================================

app.Run();