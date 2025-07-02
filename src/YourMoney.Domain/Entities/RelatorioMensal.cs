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
        public Money TotalReceitas { get; private set; }
        public Money TotalDespesas { get; private set; }
        public Money TotalInvestimentos { get; private set; }
        public Money Saldo { get; private set; }
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
            TotalReceitas = new Money(0);
            TotalDespesas = new Money(0);
            TotalInvestimentos = new Money(0);
            Saldo = new Money(0);
            DataGeracao = DateTime.Now;
        }

        public void CalcularTotais(List<Receita> receitas, List<Despesa> despesas, List<Investimento> investimentos)
        {
            TotalReceitas = new Money(receitas.Sum(r => r.Valor.Valor));
            TotalDespesas = new Money(despesas.Sum(d => d.Valor.Valor));
            TotalInvestimentos = new Money(investimentos.Sum(i => i.ValorInvestido.Valor));
            Saldo = TotalReceitas.Subtrair(TotalDespesas).Subtrair(TotalInvestimentos);

            CalcularDespesasPorCategoria(despesas);
            CalcularReceitasPorCategoria(receitas);
        }

        private void CalcularDespesasPorCategoria(List<Despesa> despesas)
        {
            _despesasPorCategoria.Clear();
            var grupos = despesas.GroupBy(d => d.CategoriaId);

            foreach (var grupo in grupos)
            {
                var total = new Money(grupo.Sum(d => d.Valor.Valor));
                _despesasPorCategoria.Add(new DespesaPorCategoria(grupo.Key, total));
            }
        }

        private void CalcularReceitasPorCategoria(List<Receita> receitas)
        {
            _receitasPorCategoria.Clear();
            var grupos = receitas.GroupBy(r => r.CategoriaId);

            foreach (var grupo in grupos)
            {
                var total = new Money(grupo.Sum(r => r.Valor.Valor));
                _receitasPorCategoria.Add(new ReceitaPorCategoria(grupo.Key, total));
            }
        }
    }

    public class DespesaPorCategoria
    {
        public Guid CategoriaId { get; private set; }
        public Money Total { get; private set; }

        public DespesaPorCategoria(Guid categoriaId, Money total)
        {
            CategoriaId = categoriaId;
            Total = total;
        }
    }

    public class ReceitaPorCategoria
    {
        public Guid CategoriaId { get; private set; }
        public Money Total { get; private set; }

        public ReceitaPorCategoria(Guid categoriaId, Money total)
        {
            CategoriaId = categoriaId;
            Total = total;
        }
    }
}