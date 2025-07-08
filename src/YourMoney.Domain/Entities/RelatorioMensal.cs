using System;
using System.Collections.Generic;
using System.Linq;
using YourMoney.Domain.ValueObjects;

namespace YourMoney.Domain.Entities
{
    public class RelatorioMensal : BaseEntity
    {
        public int Mes { get; private set; }
        public int Ano { get; private set; }
        public Decimal TotalReceitas { get; private set; }
        public Decimal TotalDespesas { get; private set; }
        public Decimal TotalInvestimentos { get; private set; }
        public Decimal Saldo { get; private set; }
        public DateTime DataGeracao { get; private set; }

        private readonly List<DespesaPorCategoria> _despesasPorCategoria = new();
        public IReadOnlyList<DespesaPorCategoria> DespesasPorCategoria => _despesasPorCategoria.AsReadOnly();

        private readonly List<ReceitaPorCategoria> _receitasPorCategoria = new();
        public IReadOnlyList<ReceitaPorCategoria> ReceitasPorCategoria => _receitasPorCategoria.AsReadOnly();

        private RelatorioMensal() { }

        public RelatorioMensal(int mes, int ano)
        {
            Id = Guid.NewGuid();
            Mes = mes;
            Ano = ano;
            TotalReceitas = new Decimal(0);
            TotalDespesas = new Decimal(0);
            TotalInvestimentos = new Decimal(0);
            Saldo = new Decimal(0);
            DataGeracao = DateTime.Now;
        }

        public void CalcularTotais(List<Receita> receitas, List<Despesa> despesas, List<Investimento> investimentos)
        {
            TotalReceitas = receitas.Sum(r => r.Valor);
            TotalDespesas = despesas.Sum(d => d.Valor);
            TotalInvestimentos = investimentos.Sum(i => i.ValorInvestido);
            Saldo = TotalReceitas - TotalDespesas - TotalInvestimentos; // Fixed: Replaced "Subtrair" with standard subtraction operator "-"  

            CalcularDespesasPorCategoria(despesas);
        }

        private void CalcularDespesasPorCategoria(List<Despesa> despesas)
        {
            _despesasPorCategoria.Clear();
            var grupos = despesas.GroupBy(d => d.CategoriaId);

            foreach (var grupo in grupos)
            {
                var total = Convert.ToDecimal(grupo.Sum(d => Convert.ToDouble(d.Valor)));
                _despesasPorCategoria.Add(new DespesaPorCategoria(grupo.Key, total));
            }
        }
    }

    public class DespesaPorCategoria
    {
        public Guid CategoriaId { get; private set; }
        public Decimal Total { get; private set; }

        public DespesaPorCategoria(Guid categoriaId, Decimal total)
        {
            CategoriaId = categoriaId;
            Total = total;
        }
    }

    public class ReceitaPorCategoria
    {
        public Guid CategoriaId { get; private set; }
        public Decimal Total { get; private set; }

        public ReceitaPorCategoria(Guid categoriaId, Decimal total)
        {
            CategoriaId = categoriaId;
            Total = total;
        }
    }
}