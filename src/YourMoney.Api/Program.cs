using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Interfaces.Repositories;
using YourMoney.Infrastructure;
using YourMoney.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços
builder.Services.AddScoped<IDespesaService, DespesaService>();
builder.Services.AddScoped<IDespesaRepository, DespesaRepository>();

// Adicionar DbContext
builder.Services.AddDbContext<YourMoneyDbContext>(options =>
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
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
