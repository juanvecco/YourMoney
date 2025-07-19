using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Configurations
{
    public class ContaFinanceiraConfiguration : IEntityTypeConfiguration<ContaFinanceira>
    {
        public void Configure(EntityTypeBuilder<ContaFinanceira> builder)
        {
            builder.ToTable("tbContaFinanceira");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Descricao)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.DataCriacao).IsRequired();
        }
    }
}
