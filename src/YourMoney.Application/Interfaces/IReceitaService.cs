using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourMoney.Application.DTOs;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface IReceitaService
    {
        Task AdicionarReceitaAsync(Receita receita);
        Task<CriarReceitaResponse> CriarReceitaAsync(CriarReceitaRequest request);
        Task<Receita> GetReceitaByIdAsync(Guid id);
        Task RemoverReceitaAsync(Guid id);
        Task AtualizarAsync(Receita receita);
        Task<ReceitaDTO> AtualizarReceitaAsync(Guid id, ReceitaDTO dto);
        Task<List<ReceitaDTO>> ListarAsync();
        Task<List<ReceitaDTO>> ObterPorMesAnoAsync(int mes, int ano);
    }
}
