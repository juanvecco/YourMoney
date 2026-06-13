using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class ReceitaService : IReceitaService
    {
        private readonly IReceitaRepository _receitaRepository;
        private readonly ICurrentUserService _currentUserService;

        public ReceitaService(IReceitaRepository receitaRepository, ICurrentUserService currentUserService)
        {
            _receitaRepository = receitaRepository;
            _currentUserService = currentUserService;
        }

        public async Task AdicionarReceitaAsync(Receita receita)
        {
            if (receita.Valor <= 0)
                throw new ArgumentException("O valor da receita deve ser maior que zero.");

            receita.DefinirUsuario(_currentUserService.UserId);
            await _receitaRepository.AdicionarAsync(receita);
        }

        public async Task<CriarReceitaResponse> CriarReceitaAsync(CriarReceitaRequest request)
        {
            ValidarCriacao(request);

            var receita = new Receita(
                request.Descricao!,
                decimal.Round(request.Valor, 2, MidpointRounding.AwayFromZero),
                request.Data.Date,
                request.MesReferencia);
            receita.DefinirUsuario(_currentUserService.UserId);

            await _receitaRepository.AdicionarAsync(receita);

            return new CriarReceitaResponse
            {
                Id = receita.Id,
                Descricao = receita.Descricao,
                Valor = receita.Valor,
                Data = receita.Data,
                MesReferencia = receita.MesReferencia!.Value
            };
        }

        public async Task<Receita> GetReceitaByIdAsync(Guid id)
        {
            var receita = await _receitaRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (receita == null)
                throw new InvalidOperationException("Receita não encontrada.");

            return receita;
        }

        public async Task RemoverReceitaAsync(Guid id)
        {
            var receita = await _receitaRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (receita == null)
                throw new InvalidOperationException("Receita não encontrada.");

            await _receitaRepository.RemoverAsync(id, _currentUserService.UserId);
        }

        public async Task AtualizarAsync(Receita receita)
        {
            var existingReceita = await _receitaRepository.GetByIdAsync(receita.Id, _currentUserService.UserId);
            if (existingReceita == null)
                throw new InvalidOperationException("Receita não encontrada.");

            receita.DefinirUsuario(_currentUserService.UserId);
            await _receitaRepository.AtualizarAsync(receita);
        }

        public async Task<List<ReceitaDTO>> ListarAsync()
        {
            var receitas = await _receitaRepository.ListarAsync(_currentUserService.UserId);
            return receitas.Select(MapearReceita).ToList();
        }

        public async Task<List<ReceitaDTO>> ObterPorMesAnoAsync(int mes, int ano)
        {
            var receitas = await _receitaRepository.ObterPorMesAnoAsync(mes, ano, _currentUserService.UserId);
            return receitas.Select(MapearReceita).ToList();
        }

        private static void ValidarCriacao(CriarReceitaRequest request)
        {
            if (request == null)
                throw new ArgumentException("Dados da receita são obrigatórios.");
            if (string.IsNullOrWhiteSpace(request.Descricao))
                throw new ArgumentException("Descrição é obrigatória.");
            if (request.Descricao.Trim().Length > 255)
                throw new ArgumentException("Descrição deve ter no máximo 255 caracteres.");
            if (request.Valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero.");
            if (request.Data == default)
                throw new ArgumentException("Data é obrigatória.");
            if (request.MesReferencia == default)
                throw new ArgumentException("Mês de referência é obrigatório.");
        }

        private static ReceitaDTO MapearReceita(Receita receita)
        {
            return new ReceitaDTO
            {
                Id = receita.Id,
                Descricao = receita.Descricao,
                Valor = receita.Valor,
                Data = receita.Data,
                MesReferencia = receita.MesReferencia
                    ?? new DateTime(receita.Data.Year, receita.Data.Month, 1)
            };
        }
    }
}
