using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Repositories;

namespace YourMoney.Api.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjectionConfiguration(
            this IServiceCollection services)
        {
            services.AddScoped<IDespesaService, DespesaService>();
            services.AddScoped<IDespesaRepository, DespesaRepository>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<IReceitaService, ReceitaService>();
            services.AddScoped<IReceitaRepository, ReceitaRepository>();
            services.AddScoped<IInvestimentoService, InvestimentoService>();
            services.AddScoped<IInvestimentoRepository, InvestimentoRepository>();
            services.AddScoped<IMetaRepository, MetaRepository>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IContaFinanceiraService, ContaFinanceiraService>();
            services.AddScoped<IContaFinanceiraRepository, ContaFinanceiraRepository>();
            return services;
        }
    }
}
