using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class DespesaConfiguration : IEntityTypeConfiguration<Despesa>
    {
        public void Configure(EntityTypeBuilder<Despesa> builder)
        {
            builder.ToTable("tbDespesa");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Descricao).HasMaxLength(255).IsRequired();
            builder.Property(d => d.Valor).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(d => d.Data).IsRequired();
            builder.Property(d => d.Categoria).HasMaxLength(255).IsRequired();
        }
    }
}
