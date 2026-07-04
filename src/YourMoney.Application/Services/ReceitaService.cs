using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class ReceitaService : IReceitaService
    {
        private readonly IReceitaRepository _receitaRepository;
        private readonly IDespesaRepository _despesaRepository;
        private readonly ICurrentUserService _currentUserService;

        public ReceitaService(
            IReceitaRepository receitaRepository,
            IDespesaRepository despesaRepository,
            ICurrentUserService currentUserService)
        {
            _receitaRepository = receitaRepository;
            _despesaRepository = despesaRepository;
            _currentUserService = currentUserService;
        }

        public async Task AdicionarReceitaAsync(Receita receita)
        {
            if (receita.Valor <= 0)
                throw new ArgumentException("O valor da receita deve ser maior que zero.");

            var usuarioId = _currentUserService.UserId;
            await ValidarNaturezaAsync(receita.Natureza, receita.DespesaVinculadaId, receita.Valor, usuarioId, receita.Id);
            receita.DefinirUsuario(usuarioId);
            await _receitaRepository.AdicionarAsync(receita);
        }

        public async Task<CriarReceitaResponse> CriarReceitaAsync(CriarReceitaRequest request)
        {
            ValidarCriacao(request);

            var usuarioId = _currentUserService.UserId;
            var natureza = ParseNatureza(request.Natureza);
            var valor = decimal.Round(request.Valor, 2, MidpointRounding.AwayFromZero);
            await ValidarNaturezaAsync(natureza, request.DespesaVinculadaId, valor, usuarioId);

            var receita = new Receita(
                request.Descricao!,
                valor,
                request.Data.Date,
                request.MesReferencia,
                usuarioId,
                natureza,
                request.DespesaVinculadaId);

            await _receitaRepository.AdicionarAsync(receita);

            return MapearCriacaoReceita(receita);
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

            await ValidarNaturezaAsync(receita.Natureza, receita.DespesaVinculadaId, receita.Valor, _currentUserService.UserId, receita.Id);
            receita.DefinirUsuario(_currentUserService.UserId);
            await _receitaRepository.AtualizarAsync(receita);
        }

        public async Task<ReceitaDTO> AtualizarReceitaAsync(Guid id, ReceitaDTO dto)
        {
            if (dto == null)
                throw new ArgumentException("Dados da receita são obrigatórios.");
            if (id == Guid.Empty || dto.Id == Guid.Empty || id != dto.Id)
                throw new ArgumentException("Identificador da receita é inválido.");

            var receita = await GetReceitaByIdAsync(id);
            var natureza = ParseNatureza(dto.Natureza);
            var valor = decimal.Round(dto.Valor, 2, MidpointRounding.AwayFromZero);

            await ValidarNaturezaAsync(natureza, dto.DespesaVinculadaId, valor, _currentUserService.UserId, id);

            receita.AtualizarDescricao(dto.Descricao ?? string.Empty);
            receita.AtualizarValor(valor);
            receita.AtualizarData(dto.Data);
            if (dto.MesReferencia.HasValue)
                receita.AtualizarMesReferencia(dto.MesReferencia.Value);
            receita.AtualizarNatureza(natureza, dto.DespesaVinculadaId);

            await _receitaRepository.AtualizarAsync(receita);
            return MapearReceita(receita);
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

        private async Task ValidarNaturezaAsync(
            NaturezaReceita natureza,
            Guid? despesaVinculadaId,
            decimal valor,
            string usuarioId,
            Guid? receitaIgnoradaId = null)
        {
            if (natureza != NaturezaReceita.Reembolso)
            {
                if (despesaVinculadaId.HasValue)
                    throw new ArgumentException("Despesa vinculada só pode ser informada para reembolso.");
                return;
            }

            if (!despesaVinculadaId.HasValue || despesaVinculadaId.Value == Guid.Empty)
                throw new ArgumentException("Despesa vinculada é obrigatória para reembolso.");

            Despesa despesa;
            try
            {
                despesa = await _despesaRepository.GetByIdAsync(despesaVinculadaId.Value, usuarioId);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("Despesa vinculada não encontrada.");
            }

            var reembolsado = await _receitaRepository.GetTotalReembolsadoPorDespesaAsync(
                despesa.Id,
                usuarioId,
                receitaIgnoradaId);
            var novoTotal = decimal.Round(reembolsado + valor, 2, MidpointRounding.AwayFromZero);
            if (novoTotal > despesa.Valor)
                throw new ArgumentException("Total de reembolsos não pode ultrapassar o valor pendente da despesa.");
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

        private static NaturezaReceita ParseNatureza(string? natureza)
        {
            if (string.IsNullOrWhiteSpace(natureza))
                return NaturezaReceita.RendaDisponivel;

            if (!Enum.TryParse<NaturezaReceita>(natureza, ignoreCase: true, out var parsed)
                || !Enum.IsDefined(typeof(NaturezaReceita), parsed))
                throw new ArgumentException("Natureza da receita é inválida.");

            return parsed;
        }

        private static CriarReceitaResponse MapearCriacaoReceita(Receita receita)
        {
            var dto = MapearReceita(receita);
            return new CriarReceitaResponse
            {
                Id = dto.Id,
                Descricao = dto.Descricao,
                Valor = dto.Valor,
                Data = dto.Data,
                MesReferencia = dto.MesReferencia!.Value,
                Natureza = dto.Natureza,
                ConsideraNasMetas = dto.ConsideraNasMetas,
                DespesaVinculadaId = dto.DespesaVinculadaId,
                DespesaVinculadaDescricao = dto.DespesaVinculadaDescricao,
                ValorAbatidoEmDespesa = dto.ValorAbatidoEmDespesa
            };
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
                    ?? new DateTime(receita.Data.Year, receita.Data.Month, 1),
                Natureza = receita.Natureza.ToString(),
                ConsideraNasMetas = receita.ConsideraNasMetas,
                DespesaVinculadaId = receita.DespesaVinculadaId,
                DespesaVinculadaDescricao = receita.DespesaVinculada?.Descricao,
                ValorAbatidoEmDespesa = receita.Natureza == NaturezaReceita.Reembolso ? receita.Valor : 0m
            };
        }
    }
}
