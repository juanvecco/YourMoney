using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class InvestimentoService : IInvestimentoService
    {
        private readonly IInvestimentoRepository _investimentoRepository;
        private readonly ICurrentUserService _currentUserService;

        public InvestimentoService(
            IInvestimentoRepository investimentoRepository,
            ICurrentUserService currentUserService)
        {
            _investimentoRepository = investimentoRepository;
            _currentUserService = currentUserService;
        }

        public async Task AdicionarInvestimentoAsync(Investimento investimento)
        {
            if (investimento.ValorAtual <= 0)
                throw new ArgumentException("O valor do investimento deve ser maior que zero.");

            investimento.DefinirUsuario(_currentUserService.UserId);
            await _investimentoRepository.AdicionarAsync(investimento);
        }

        public async Task<CriarInvestimentoResponse> CriarInvestimentoAsync(CriarInvestimentoRequest request)
        {
            ValidarCriacao(request);

            var investimento = new Investimento(
                request.Nome!,
                request.Descricao ?? string.Empty,
                request.Tipo!,
                NormalizarDecimal(request.Quantidade),
                NormalizarDecimal(request.PrecoMedio),
                NormalizarDecimal(request.ValorAtual),
                request.DataInvestimento.Date,
                _currentUserService.UserId);

            await _investimentoRepository.AdicionarAsync(investimento);
            return MapearCriacao(investimento);
        }

        public async Task<Investimento> GetInvestimentoByIdAsync(Guid id)
        {
            var investimento = await _investimentoRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (investimento == null)
                throw new InvalidOperationException("Investimento não encontrado.");
            return investimento;
        }

        public async Task RemoverInvestimentoAsync(Guid id)
        {
            var investimento = await _investimentoRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (investimento == null)
                throw new InvalidOperationException("Investimento não encontrado.");
            await _investimentoRepository.RemoverAsync(id, _currentUserService.UserId);
        }

        public async Task AtualizarAsync(Investimento investimento)
        {
            var existingInvestimento = await _investimentoRepository.GetByIdAsync(
                investimento.Id,
                _currentUserService.UserId);
            if (existingInvestimento == null)
                throw new InvalidOperationException("Investimento não encontrado.");

            investimento.DefinirUsuario(_currentUserService.UserId);
            await _investimentoRepository.AtualizarAsync(investimento);
        }

        public Task<List<Investimento>> ListarAsync()
        {
            return _investimentoRepository.ListarAsync(_currentUserService.UserId);
        }

        public async Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano)
        {
            var investimentos = await _investimentoRepository.ObterPorMesAnoAsync(mes, ano, _currentUserService.UserId);
            return investimentos
                .Where(i => i.DataInvestimento.Month == mes && i.DataInvestimento.Year == ano)
                .ToList();
        }

        private static void ValidarCriacao(CriarInvestimentoRequest request)
        {
            if (request == null)
                throw new ArgumentException("Dados do investimento são obrigatórios.");
            if (string.IsNullOrWhiteSpace(request.Nome))
                throw new ArgumentException("Nome do investimento é obrigatório.");
            if (request.Nome.Trim().Length > 100)
                throw new ArgumentException("Nome do investimento deve ter no máximo 100 caracteres.");
            if ((request.Descricao?.Trim().Length ?? 0) > 500)
                throw new ArgumentException("Descrição deve ter no máximo 500 caracteres.");
            if (string.IsNullOrWhiteSpace(request.Tipo))
                throw new ArgumentException("Tipo do investimento é obrigatório.");
            if (request.Tipo.Trim().Length > 100)
                throw new ArgumentException("Tipo do investimento deve ter no máximo 100 caracteres.");
            if (request.Quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero.");
            if (request.PrecoMedio <= 0)
                throw new ArgumentException("Preço médio deve ser maior que zero.");
            if (request.ValorAtual <= 0)
                throw new ArgumentException("Valor atual deve ser maior que zero.");
            if (request.DataInvestimento == default)
                throw new ArgumentException("Data do investimento é obrigatória.");
        }

        private static decimal NormalizarDecimal(decimal valor)
        {
            return decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
        }

        private static CriarInvestimentoResponse MapearCriacao(Investimento investimento)
        {
            return new CriarInvestimentoResponse
            {
                Id = investimento.Id,
                Nome = investimento.Nome,
                Descricao = investimento.Descricao,
                Tipo = investimento.Tipo,
                Quantidade = investimento.Quantidade,
                PrecoMedio = investimento.PrecoMedio,
                ValorAtual = investimento.ValorAtual,
                DataInvestimento = investimento.DataInvestimento,
                DataResgate = investimento.DataResgate,
                Ativo = investimento.Ativo
            };
        }
    }
}
