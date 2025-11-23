using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using YourMoney.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YourMoney.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using YourMoney.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Entity Framework Core e Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Necessário para reset de senha e outros tokens

// 2. Configuração do JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true; // Deve ser true em produção
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});

// Adicionar Autorização (necessário para o [Authorize] funcionar)
//builder.Services.AddAuthorization();

//if (!builder.Environment.IsDevelopment())
//{
//    builder.Logging.ClearProviders();                    // remove todos os providers padrão
//    builder.Logging.AddConsole();                         // mantém só o console
//    builder.Logging.SetMinimumLevel(LogLevel.Warning);   // só Warning, Error e Critical
//    // Opcional: se quiser ver Information mas não o spam do EF Core:
//    // builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
//}

// ============= CONNECTION STRING =============
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string não encontrada! Configure a variável de ambiente 'DATABASE_CONNECTION_STRING' ou o appsettings.json");

// Só mostra a connection string em desenvolvimento (evita exibir senha no Railway)
if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("=== CONNECTION STRING EM USO ===");
    Console.WriteLine(connectionString);
    Console.WriteLine("=====================================");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// ====================================================

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
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
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

// CORS (temporariamente aberto)
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
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
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