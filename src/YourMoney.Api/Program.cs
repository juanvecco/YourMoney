using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using YourMoney.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ============= NOVO: Sobrescreve a Connection String com variável de ambiente (Railway) =============
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] =
        Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
}
// ====================================================================================================

// Adicionar serviços (DI)
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

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// ============= CORS atualizado – funciona local e no Railway =============
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()      // Temporariamente aceita qualquer origem (ideal para MVP)
            .AllowAnyHeader()
            .AllowAnyMethod();
        // Quando o frontend estiver no ar, troque por:
        // .WithOrigins("https://seu-frontend.railway.app", "http://localhost:4200")
    });
});
// ===========================================================================

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

// ============= APLICA MIGRAÇÕES AUTOMATICAMENTE NO STARTUP =============
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Isso cria/atualiza o banco no Railway automaticamente
}
// =========================================================================

app.Run();