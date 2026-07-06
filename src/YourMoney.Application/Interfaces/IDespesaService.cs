using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourMoney.Application.DTOs;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface IDespesaService
    {
        Task AdicionarDespesaAsync(Despesa despesa);
        Task<CriarDespesaResponse> CriarDespesaAsync(CriarDespesaRequest request);
        Task<Despesa> GetDespesaByIdAsync(Guid id); // Método já existente
        Task<DespesaDTO> ObterDtoPorIdAsync(Guid id);
        Task RemoverDespesaAsync(Guid id); // Método já existente
        Task AtualizarAsync(Despesa despesa); // Novo método
        Task<List<Despesa>> ListarAsync();
        Task<List<DespesaDTO>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira = null);
        Task<ConsultaDespesasResponse> ConsultarDespesasAsync(ConsultaDespesasRequest request);
        Task<ParcelamentoDespesaResponse> CriarParcelamentoAsync(ParcelamentoDespesaRequest request);
    }
}
