using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Infrastructure.Configurations;

namespace YourMoney.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Despesa> Despesas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Receita> Receitas { get; set; }
        public DbSet<Investimento> Investimentos { get; set; }
        public DbSet<Meta> Metas { get; set; }
        public DbSet<MetaMensal> MetasMensais { get; set; }
        public DbSet<ContaFinanceira> ContaFinanceira { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new DespesaConfiguration());
            modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
            modelBuilder.ApplyConfiguration(new ReceitaConfiguration());
            modelBuilder.ApplyConfiguration(new InvestimentoConfiguration());
            modelBuilder.ApplyConfiguration(new MetaConfiguration());
            modelBuilder.ApplyConfiguration(new MetaMensalConfiguration());
            modelBuilder.ApplyConfiguration(new ContaFinanceiraConfiguration());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if ((property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                        && property.GetColumnType() == null)
                    {
                        property.SetColumnType("decimal(18,2)");
                    }
                }
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>(nameof(BaseEntity.UsuarioId))
                    .HasMaxLength(450)
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.UsuarioId));
            }
        }
    }
}
