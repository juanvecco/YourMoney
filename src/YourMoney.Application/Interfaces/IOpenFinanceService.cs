using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces
{
    public interface IOpenFinanceService
    {
        Task<OpenFinanceSourcesResponseDTO> ObterFontesAsync();
        Task<OpenFinanceTransactionPreviewResponseDTO> ObterPreviewTransacoesAsync(string? sourceId);
    }
}
