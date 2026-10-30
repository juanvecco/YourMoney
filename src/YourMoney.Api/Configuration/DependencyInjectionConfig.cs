using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Api.Services;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Repositories;

namespace YourMoney.Api.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjectionConfiguration(
            this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUsuarioDemonstracaoService, UsuarioDemonstracaoService>();
            services.AddScoped<IDespesaService, DespesaService>();
            services.AddScoped<IDespesaRepository, DespesaRepository>();
            services.AddScoped<IDespesaRecorrenteService, DespesaRecorrenteService>();
            services.AddScoped<IDespesaRecorrenteRepository, DespesaRecorrenteRepository>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<IReceitaService, ReceitaService>();
            services.AddScoped<IReceitaRepository, ReceitaRepository>();
            services.AddScoped<IReceitaRecorrenteService, ReceitaRecorrenteService>();
            services.AddScoped<IReceitaRecorrenteRepository, ReceitaRecorrenteRepository>();
            services.AddScoped<IInvestimentoService, InvestimentoService>();
            services.AddScoped<IInvestimentoRepository, InvestimentoRepository>();
            services.AddScoped<IGrupoObjetivoInvestimentoService, GrupoObjetivoInvestimentoService>();
            services.AddScoped<IGrupoObjetivoInvestimentoRepository, GrupoObjetivoInvestimentoRepository>();
            services.AddScoped<IMetaRepository, MetaRepository>();
            services.AddScoped<IMetaMensalService, MetaMensalService>();
            services.AddScoped<IMetaMensalRepository, MetaMensalRepository>();
            services.AddScoped<IAporteObjetivoInvestimentoService, AporteObjetivoInvestimentoService>();
            services.AddScoped<IAporteObjetivoInvestimentoRepository, AporteObjetivoInvestimentoRepository>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IContaFinanceiraService, ContaFinanceiraService>();
            services.AddScoped<IContaFinanceiraRepository, ContaFinanceiraRepository>();
            services.AddScoped<IMovimentacaoFinanceiraService, MovimentacaoFinanceiraService>();
            services.AddScoped<IOpenFinanceService, OpenFinanceService>();
            services.AddMemoryCache();
            services.AddHttpClient("Pluggy", client =>
            {
                client.BaseAddress = new Uri("https://api.pluggy.ai/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<IPluggyService, PluggyService>();
            services.AddScoped<IPasswordResetDeliveryService, SmtpPasswordResetDeliveryService>();
            return services;
        }
    }
}
