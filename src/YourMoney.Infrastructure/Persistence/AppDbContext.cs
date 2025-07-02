using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Infrastructure.Configurations;

namespace YourMoney.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Despesa> TbDespesa { get; set; }
        public DbSet<Despesa> Despesas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new DespesaConfiguration());
            // Configurações adicionais (se necessário)
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Receita> Receitas { get; set; }
        public DbSet<Investimento> Investimentos { get; set; }
        public DbSet<Meta> Metas { get; set; }
    }
}
