using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using YourMoney.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços
builder.Services.AddScoped<IDespesaService, DespesaService>();
builder.Services.AddScoped<IDespesaRepository, DespesaRepository>(); 
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IReceitaRepository, ReceitaRepository>();
builder.Services.AddScoped<IReceitaService, ReceitaService>();
builder.Services.AddScoped<IInvestimentoRepository, InvestimentoRepository>();
builder.Services.AddScoped<IMetaRepository, MetaRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Adicionar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adicionar controladores
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

var app = builder.Build();

// Configure o Swagger no pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "YourMoney API V1");
        c.RoutePrefix = string.Empty; // Faz o Swagger ser acessível na raiz
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
