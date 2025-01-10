using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure;

    public class YourMoneyDbContext : DbContext
{
    public DbSet<Despesa> Despesas { get; set; }

    public YourMoneyDbContext(DbContextOptions<YourMoneyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações adicionais, se necessário
    }
}
