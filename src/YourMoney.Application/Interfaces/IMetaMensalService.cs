using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces
{
    public interface IMetaMensalService
    {
        Task<MetasMensaisResumoDTO> ObterResumoAsync(int? mes = null, int? ano = null);
        Task<MetaMensalDTO> CriarAsync(CriarMetaMensalDTO request);
        Task<MetaMensalDTO> AtualizarAsync(Guid id, AtualizarMetaMensalDTO request);
        Task RemoverAsync(Guid id);
    }
}
