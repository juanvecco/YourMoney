using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class ReceitaRecorrenteService : IReceitaRecorrenteService
    {
        private readonly IReceitaRecorrenteRepository _recorrenteRepository;
        private readonly IReceitaService _receitaService;
        private readonly IContaFinanceiraRepository _contaRepository;
        private readonly ICurrentUserService _currentUserService;

        public ReceitaRecorrenteService(
            IReceitaRecorrenteRepository recorrenteRepository,
            IReceitaService receitaService,
            IContaFinanceiraRepository contaRepository,
            ICurrentUserService currentUserService)
        {
            _recorrenteRepository = recorrenteRepository;
            _receitaService = receitaService;
            _contaRepository = contaRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ReceitaRecorrenteResponse> CriarAsync(ReceitaRecorrenteRequest request)
        {
            var natureza = await ValidarRequestAsync(request);
            var recorrencia = new ReceitaRecorrente(
                request.Descricao!,
                request.ValorPrevisto,
                request.IdContaFinanceira,
                natureza,
                request.EhSalario,
                request.ConsideraReservaEmergencia,
                request.DataRecebimento,
                request.DataInicio,
                request.DataTermino,
                _currentUserService.UserId);

            await _recorrenteRepository.AdicionarAsync(recorrencia);
            return await MapearRecorrenciaAsync(recorrencia);
        }

        public async Task<ListarReceitasRecorrentesResponse> ListarAsync(bool? ativas = null)
        {
            var itens = await _recorrenteRepository.ListarAsync(_currentUserService.UserId, ativas);
            return new ListarReceitasRecorrentesResponse
            {
                Itens = itens.Select(MapearRecorrencia).ToList()
            };
        }

        public async Task<ReceitaRecorrenteResponse> ObterPorIdAsync(Guid id)
        {
            return MapearRecorrencia(await ObterRecorrenciaAsync(id));
        }

        public async Task<ReceitaRecorrenteResponse> AtualizarAsync(Guid id, ReceitaRecorrenteRequest request)
        {
            var natureza = await ValidarRequestAsync(request);
            var recorrencia = await ObterRecorrenciaAsync(id);
            recorrencia.Atualizar(
                request.Descricao!,
                request.ValorPrevisto,
                request.IdContaFinanceira,
                natureza,
                request.EhSalario,
                request.ConsideraReservaEmergencia,
                request.DataRecebimento,
                request.DataInicio,
                request.DataTermino);

            await _recorrenteRepository.AtualizarAsync(recorrencia);
            return await MapearRecorrenciaAsync(recorrencia);
        }

        public async Task DesativarAsync(Guid id)
        {
            var recorrencia = await ObterRecorrenciaAsync(id);
            recorrencia.Desativar();
            await _recorrenteRepository.AtualizarAsync(recorrencia);
        }

        public async Task<ReceitaRecorrenteResponse> EncerrarAsync(Guid id, EncerrarReceitaRecorrenteRequest request)
        {
            if (request == null || request.DataTermino == default)
                throw new ArgumentException("Data de término é obrigatória.");

            var recorrencia = await ObterRecorrenciaAsync(id);
            recorrencia.Encerrar(request.DataTermino);
            await _recorrenteRepository.AtualizarAsync(recorrencia);
            return await MapearRecorrenciaAsync(recorrencia);
        }

        public async Task<ListarSugestoesReceitasRecorrentesResponse> ListarSugestoesAsync(int mes, int ano)
        {
            ValidarMesAno(mes, ano);
            var usuarioId = _currentUserService.UserId;
            var mesReferencia = new DateTime(ano, mes, 1);
            var recorrencias = await _recorrenteRepository.ListarElegiveisParaMesAsync(usuarioId, mesReferencia);

            foreach (var recorrencia in recorrencias.Where(r => r.EstaElegivelParaMes(mesReferencia)))
            {
                var existente = await _recorrenteRepository.ObterOcorrenciaAsync(recorrencia.Id, mesReferencia, usuarioId);
                if (existente == null)
                {
                    await _recorrenteRepository.AdicionarOcorrenciaAsync(
                        new ReceitaRecorrenteOcorrencia(recorrencia.Id, mesReferencia, usuarioId));
                }
            }

            var ocorrencias = await _recorrenteRepository.ListarOcorrenciasPorMesAsync(usuarioId, mesReferencia);
            return new ListarSugestoesReceitasRecorrentesResponse
            {
                Mes = mes,
                Ano = ano,
                Itens = ocorrencias.Select(MapearSugestao).ToList()
            };
        }

        public async Task<CriarReceitaResponse> ConfirmarSugestaoAsync(
            Guid ocorrenciaId,
            ConfirmarSugestaoReceitaRecorrenteRequest request)
        {
            var ocorrencia = await ObterOcorrenciaAsync(ocorrenciaId);
            if (!ocorrencia.EstaPendente)
                throw new InvalidOperationException("Sugestão mensal já finalizada.");

            var recorrencia = ocorrencia.ReceitaRecorrente;
            var descricao = string.IsNullOrWhiteSpace(request?.Descricao)
                ? recorrencia.Descricao
                : request!.Descricao!.Trim();
            var valor = request?.Valor ?? recorrencia.ValorPrevisto;
            var data = request?.Data?.Date ?? recorrencia.CalcularDataSugerida(ocorrencia.MesReferencia);
            var idConta = request?.IdContaFinanceira ?? recorrencia.IdContaFinanceira;
            var natureza = ParseNatureza(request?.Natureza ?? recorrencia.Natureza.ToString());

            var conta = await ValidarDadosConfirmacaoAsync(descricao, valor, data, idConta, natureza);
            var receita = await _receitaService.CriarReceitaAsync(new CriarReceitaRequest
            {
                Descricao = descricao,
                Valor = valor,
                Data = data,
                MesReferencia = ocorrencia.MesReferencia,
                Natureza = natureza.ToString(),
                IdContaFinanceira = idConta
            });

            receita.ContaDescricao = conta.Descricao;
            ocorrencia.Confirmar(receita.Id);
            await _recorrenteRepository.AtualizarOcorrenciaAsync(ocorrencia);
            return receita;
        }

        public async Task IgnorarSugestaoAsync(Guid ocorrenciaId)
        {
            var ocorrencia = await ObterOcorrenciaAsync(ocorrenciaId);
            if (!ocorrencia.EstaPendente)
                throw new InvalidOperationException("Sugestão mensal já finalizada.");

            ocorrencia.Ignorar();
            await _recorrenteRepository.AtualizarOcorrenciaAsync(ocorrencia);
        }

        public async Task<ProjecaoReservaEmergenciaResponse> ObterProjecaoReservaAsync()
        {
            var mesAtual = ReceitaRecorrente.NormalizarMesReferencia(DateTime.Today);
            var recorrencias = await _recorrenteRepository.ListarElegiveisParaReservaAsync(
                _currentUserService.UserId,
                mesAtual);

            return new ProjecaoReservaEmergenciaResponse
            {
                Itens = recorrencias
                    .Where(r => r.EstaElegivelParaReserva(mesAtual))
                    .Select(r => new ProjecaoReservaEmergenciaItemResponse
                    {
                        ReceitaRecorrenteId = r.Id,
                        Descricao = r.Descricao,
                        ContaDescricao = r.ContaFinanceira?.Descricao ?? string.Empty,
                        EhSalario = r.EhSalario,
                        ValorMensal = r.ValorPrevisto,
                        ValorSeisMeses = r.ValorPrevisto * 6m,
                        ValorDozeMeses = r.ValorPrevisto * 12m
                    })
                    .ToList()
            };
        }

        private async Task<NaturezaReceita> ValidarRequestAsync(ReceitaRecorrenteRequest request)
        {
            if (request == null)
                throw new ArgumentException("Dados da receita recorrente são obrigatórios.");

            var natureza = ParseNatureza(request.Natureza);
            _ = new ReceitaRecorrente(
                request.Descricao ?? string.Empty,
                request.ValorPrevisto,
                request.IdContaFinanceira,
                natureza,
                request.EhSalario,
                request.ConsideraReservaEmergencia,
                request.DataRecebimento,
                request.DataInicio,
                request.DataTermino,
                _currentUserService.UserId);

            if (!await _contaRepository.ExisteAsync(request.IdContaFinanceira, _currentUserService.UserId))
                throw new ArgumentException("Conta Financeira não encontrada.");

            return natureza;
        }

        private async Task<ContaFinanceira> ValidarDadosConfirmacaoAsync(
            string descricao,
            decimal valor,
            DateTime data,
            Guid idConta,
            NaturezaReceita natureza)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória.");
            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero.");
            if (data == default)
                throw new ArgumentException("Data é obrigatória.");
            if (natureza == NaturezaReceita.Reembolso)
                throw new ArgumentException("Reembolso não é permitido para receita recorrente.");
            if (!await _contaRepository.ExisteAsync(idConta, _currentUserService.UserId))
                throw new ArgumentException("Conta Financeira não encontrada.");

            return await _contaRepository.GetByIdAsync(idConta, _currentUserService.UserId);
        }

        private async Task<ReceitaRecorrente> ObterRecorrenciaAsync(Guid id)
        {
            var recorrencia = await _recorrenteRepository.ObterPorIdAsync(id, _currentUserService.UserId);
            return recorrencia ?? throw new InvalidOperationException("Receita recorrente não encontrada.");
        }

        private async Task<ReceitaRecorrenteOcorrencia> ObterOcorrenciaAsync(Guid id)
        {
            var ocorrencia = await _recorrenteRepository.ObterOcorrenciaPorIdAsync(id, _currentUserService.UserId);
            return ocorrencia ?? throw new InvalidOperationException("Sugestão mensal não encontrada.");
        }

        private async Task<ReceitaRecorrenteResponse> MapearRecorrenciaAsync(ReceitaRecorrente recorrencia)
        {
            if (recorrencia.ContaFinanceira == null)
                recorrencia = await ObterRecorrenciaAsync(recorrencia.Id);
            return MapearRecorrencia(recorrencia);
        }

        private static ReceitaRecorrenteResponse MapearRecorrencia(ReceitaRecorrente recorrencia)
        {
            return new ReceitaRecorrenteResponse
            {
                Id = recorrencia.Id,
                Descricao = recorrencia.Descricao,
                ValorPrevisto = recorrencia.ValorPrevisto,
                IdContaFinanceira = recorrencia.IdContaFinanceira,
                ContaDescricao = recorrencia.ContaFinanceira?.Descricao ?? string.Empty,
                Natureza = recorrencia.Natureza.ToString(),
                EhSalario = recorrencia.EhSalario,
                ConsideraReservaEmergencia = recorrencia.ConsideraReservaEmergencia,
                DiaRecebimento = recorrencia.DiaRecebimento,
                DataInicio = recorrencia.DataInicio,
                DataTermino = recorrencia.DataTermino,
                Ativa = recorrencia.Ativa
            };
        }

        private static SugestaoReceitaRecorrenteResponse MapearSugestao(ReceitaRecorrenteOcorrencia ocorrencia)
        {
            var recorrencia = ocorrencia.ReceitaRecorrente;
            return new SugestaoReceitaRecorrenteResponse
            {
                OcorrenciaId = ocorrencia.Id,
                ReceitaRecorrenteId = recorrencia.Id,
                MesReferencia = ocorrencia.MesReferencia,
                Status = ocorrencia.Status.ToString(),
                Descricao = recorrencia.Descricao,
                ValorPrevisto = recorrencia.ValorPrevisto,
                DataSugerida = recorrencia.CalcularDataSugerida(ocorrencia.MesReferencia),
                IdContaFinanceira = recorrencia.IdContaFinanceira,
                ContaDescricao = recorrencia.ContaFinanceira?.Descricao ?? string.Empty,
                Natureza = recorrencia.Natureza.ToString(),
                ReceitaConfirmadaId = ocorrencia.ReceitaConfirmadaId
            };
        }

        private static NaturezaReceita ParseNatureza(string? natureza)
        {
            if (string.IsNullOrWhiteSpace(natureza))
                return NaturezaReceita.RendaDisponivel;
            if (!Enum.TryParse<NaturezaReceita>(natureza, true, out var parsed)
                || !Enum.IsDefined(typeof(NaturezaReceita), parsed)
                || parsed == NaturezaReceita.Reembolso)
                throw new ArgumentException("Natureza da receita recorrente é inválida.");
            return parsed;
        }

        private static void ValidarMesAno(int mes, int ano)
        {
            if (mes < 1 || mes > 12 || ano < 1)
                throw new ArgumentException("Mês e ano inválidos.");
        }
    }
}
