using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class DespesaRecorrenteService : IDespesaRecorrenteService
    {
        private readonly IDespesaRecorrenteRepository _recorrenteRepository;
        private readonly IDespesaService _despesaService;
        private readonly IContaFinanceiraRepository _contaRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ICurrentUserService _currentUserService;

        public DespesaRecorrenteService(
            IDespesaRecorrenteRepository recorrenteRepository,
            IDespesaService despesaService,
            IContaFinanceiraRepository contaRepository,
            ICategoriaRepository categoriaRepository,
            ICurrentUserService currentUserService)
        {
            _recorrenteRepository = recorrenteRepository;
            _despesaService = despesaService;
            _contaRepository = contaRepository;
            _categoriaRepository = categoriaRepository;
            _currentUserService = currentUserService;
        }

        public async Task<DespesaRecorrenteResponse> CriarAsync(DespesaRecorrenteRequest request)
        {
            await ValidarRequestAsync(request);
            var usuarioId = _currentUserService.UserId;

            var recorrencia = new DespesaRecorrente(
                request.Descricao!,
                request.ValorPrevisto,
                request.IdContaFinanceira,
                request.IdTipoDespesa,
                request.IdNaturezaDespesa,
                request.IdCategoria,
                request.DataVencimento,
                request.DataInicio,
                request.DataTermino,
                usuarioId);

            await _recorrenteRepository.AdicionarAsync(recorrencia);
            return await MapearRecorrenciaAsync(recorrencia);
        }

        public async Task<ListarDespesasRecorrentesResponse> ListarAsync(bool? ativas = null)
        {
            var recorrencias = await _recorrenteRepository.ListarAsync(_currentUserService.UserId, ativas);
            var itens = new List<DespesaRecorrenteResponse>();
            foreach (var recorrencia in recorrencias)
                itens.Add(await MapearRecorrenciaAsync(recorrencia));

            return new ListarDespesasRecorrentesResponse { Itens = itens };
        }

        public async Task<DespesaRecorrenteResponse> ObterPorIdAsync(Guid id)
        {
            var recorrencia = await ObterRecorrenciaAsync(id);
            return await MapearRecorrenciaAsync(recorrencia);
        }

        public async Task<DespesaRecorrenteResponse> AtualizarAsync(Guid id, DespesaRecorrenteRequest request)
        {
            await ValidarRequestAsync(request);
            var recorrencia = await ObterRecorrenciaAsync(id);
            recorrencia.Atualizar(
                request.Descricao!,
                request.ValorPrevisto,
                request.IdContaFinanceira,
                request.IdTipoDespesa,
                request.IdNaturezaDespesa,
                request.IdCategoria,
                request.DataVencimento,
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

        public async Task<DespesaRecorrenteResponse> EncerrarAsync(Guid id, EncerrarDespesaRecorrenteRequest request)
        {
            if (request == null || request.DataTermino == default)
                throw new ArgumentException("Data de término é obrigatória.");

            var recorrencia = await ObterRecorrenciaAsync(id);
            recorrencia.Encerrar(request.DataTermino);
            await _recorrenteRepository.AtualizarAsync(recorrencia);
            return await MapearRecorrenciaAsync(recorrencia);
        }

        public async Task<ListarSugestoesDespesasRecorrentesResponse> ListarSugestoesAsync(int mes, int ano)
        {
            ValidarMesAno(mes, ano);
            var usuarioId = _currentUserService.UserId;
            var mesReferencia = new DateTime(ano, mes, 1);
            var elegiveis = await _recorrenteRepository.ListarElegiveisParaMesAsync(usuarioId, mesReferencia);

            foreach (var recorrencia in elegiveis.Where(r => r.EstaElegivelParaMes(mesReferencia)))
            {
                var existente = await _recorrenteRepository.ObterOcorrenciaAsync(recorrencia.Id, mesReferencia, usuarioId);
                if (existente == null)
                {
                    await _recorrenteRepository.AdicionarOcorrenciaAsync(
                        new DespesaRecorrenteOcorrencia(recorrencia.Id, mesReferencia, usuarioId));
                }
            }

            var ocorrencias = await _recorrenteRepository.ListarOcorrenciasPorMesAsync(usuarioId, mesReferencia);
            return new ListarSugestoesDespesasRecorrentesResponse
            {
                Mes = mes,
                Ano = ano,
                Itens = ocorrencias.Select(MapearSugestao).ToList()
            };
        }

        public async Task<DespesaDTO> ConfirmarSugestaoAsync(Guid ocorrenciaId, ConfirmarSugestaoDespesaRecorrenteRequest request)
        {
            var ocorrencia = await ObterOcorrenciaAsync(ocorrenciaId);
            if (!ocorrencia.EstaPendente)
                throw new InvalidOperationException("Sugestão mensal já finalizada.");

            var recorrencia = ocorrencia.DespesaRecorrente;
            var descricao = string.IsNullOrWhiteSpace(request?.Descricao) ? recorrencia.Descricao : request!.Descricao!.Trim();
            var valor = request?.Valor ?? recorrencia.ValorPrevisto;
            var data = request?.Data?.Date ?? recorrencia.CalcularDataSugerida(ocorrencia.MesReferencia);
            var idConta = request?.IdContaFinanceira ?? recorrencia.IdContaFinanceira;
            var idTipo = request?.IdTipoDespesa ?? recorrencia.IdTipoDespesa;
            var idNatureza = request?.IdNaturezaDespesa ?? recorrencia.IdNaturezaDespesa;
            var idCategoria = request?.IdCategoria ?? recorrencia.IdCategoria;

            await ValidarDadosFinanceirosAsync(descricao, valor, idConta, idTipo, idNatureza, idCategoria, data, ocorrencia.MesReferencia);

            var criada = await _despesaService.CriarDespesaAsync(new CriarDespesaRequest
            {
                Descricao = descricao,
                Valor = valor,
                Data = data,
                MesReferencia = ocorrencia.MesReferencia,
                IdContaFinanceira = idConta,
                IdCategoria = idCategoria
            });

            ocorrencia.Confirmar(criada.Id);
            await _recorrenteRepository.AtualizarOcorrenciaAsync(ocorrencia);
            return await _despesaService.ObterDtoPorIdAsync(criada.Id);
        }

        public async Task IgnorarSugestaoAsync(Guid ocorrenciaId)
        {
            var ocorrencia = await ObterOcorrenciaAsync(ocorrenciaId);
            if (!ocorrencia.EstaPendente)
                throw new InvalidOperationException("Sugestão mensal já finalizada.");

            ocorrencia.Ignorar();
            await _recorrenteRepository.AtualizarOcorrenciaAsync(ocorrencia);
        }

        private async Task<DespesaRecorrente> ObterRecorrenciaAsync(Guid id)
        {
            var recorrencia = await _recorrenteRepository.ObterPorIdAsync(id, _currentUserService.UserId);
            return recorrencia ?? throw new InvalidOperationException("Despesa recorrente não encontrada.");
        }

        private async Task<DespesaRecorrenteOcorrencia> ObterOcorrenciaAsync(Guid ocorrenciaId)
        {
            var ocorrencia = await _recorrenteRepository.ObterOcorrenciaPorIdAsync(ocorrenciaId, _currentUserService.UserId);
            return ocorrencia ?? throw new InvalidOperationException("Sugestão mensal não encontrada.");
        }

        private async Task ValidarRequestAsync(DespesaRecorrenteRequest request)
        {
            if (request == null)
                throw new ArgumentException("Dados da despesa recorrente são obrigatórios.");

            await ValidarDadosFinanceirosAsync(
                request.Descricao,
                request.ValorPrevisto,
                request.IdContaFinanceira,
                request.IdTipoDespesa,
                request.IdNaturezaDespesa,
                request.IdCategoria,
                request.DataVencimento,
                DespesaRecorrente.NormalizarMesReferencia(request.DataInicio));

            if (request.DataInicio == default)
                throw new ArgumentException("Data de início é obrigatória.");
            if (request.DataTermino.HasValue && request.DataTermino.Value.Date < request.DataInicio.Date)
                throw new ArgumentException("Data de término não pode ser anterior à data de início.");
        }

        private async Task ValidarDadosFinanceirosAsync(
            string? descricao,
            decimal valor,
            Guid idConta,
            Guid idTipo,
            Guid idNatureza,
            Guid idCategoria,
            DateTime data,
            DateTime mesReferencia)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória.");
            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero.");
            if (data == default)
                throw new ArgumentException("Data é obrigatória.");
            if (idConta == Guid.Empty)
                throw new ArgumentException("Conta Financeira é obrigatória.");
            if (idTipo == Guid.Empty)
                throw new ArgumentException("Tipo é obrigatório.");
            if (idNatureza == Guid.Empty)
                throw new ArgumentException("Natureza é obrigatória.");
            if (idCategoria == Guid.Empty)
                throw new ArgumentException("Categoria é obrigatória.");

            var usuarioId = _currentUserService.UserId;
            if (!await _contaRepository.ExisteAsync(idConta, usuarioId))
                throw new ArgumentException("Conta Financeira não encontrada.");

            var categorias = await _categoriaRepository.GetAllAsync(usuarioId);
            var tipo = categorias.FirstOrDefault(c => c.Id == idTipo)
                ?? throw new ArgumentException("Tipo não encontrado.");
            var natureza = categorias.FirstOrDefault(c => c.Id == idNatureza)
                ?? throw new ArgumentException("Natureza não encontrada.");
            var categoria = categorias.FirstOrDefault(c => c.Id == idCategoria)
                ?? throw new ArgumentException("Categoria não encontrada.");

            if (tipo.CategoriaPaiId.HasValue)
                throw new ArgumentException("Tipo deve ser uma categoria raiz.");
            if (tipo.TipoTransacao != TipoTransacao.Despesa && tipo.TipoTransacao != TipoTransacao.Receita)
                throw new ArgumentException("Tipo deve ser uma categoria de despesa.");
            if (natureza.CategoriaPaiId != tipo.Id)
                throw new ArgumentException("Natureza deve pertencer ao tipo informado.");

            var idsDescendentes = ObterCategoriaEDescendentes(categorias, natureza.Id);
            if (!idsDescendentes.Contains(categoria.Id))
                throw new ArgumentException("Categoria deve pertencer à natureza informada.");

            _ = DespesaRecorrente.NormalizarMesReferencia(mesReferencia);
        }

        private async Task<DespesaRecorrenteResponse> MapearRecorrenciaAsync(DespesaRecorrente recorrencia)
        {
            var contaDescricao = recorrencia.ContaFinanceira?.Descricao
                ?? (await _contaRepository.GetByIdAsync(recorrencia.IdContaFinanceira, _currentUserService.UserId)).Descricao;
            var categorias = await _categoriaRepository.GetAllAsync(_currentUserService.UserId);

            return new DespesaRecorrenteResponse
            {
                Id = recorrencia.Id,
                Descricao = recorrencia.Descricao,
                ValorPrevisto = recorrencia.ValorPrevisto,
                IdContaFinanceira = recorrencia.IdContaFinanceira,
                ContaDescricao = contaDescricao,
                IdTipoDespesa = recorrencia.IdTipoDespesa,
                TipoDescricao = ObterDescricaoCategoria(categorias, recorrencia.IdTipoDespesa),
                IdNaturezaDespesa = recorrencia.IdNaturezaDespesa,
                NaturezaDescricao = ObterDescricaoCategoria(categorias, recorrencia.IdNaturezaDespesa),
                IdCategoria = recorrencia.IdCategoria,
                CategoriaDescricao = ObterDescricaoCategoria(categorias, recorrencia.IdCategoria),
                DiaVencimento = recorrencia.DiaVencimento,
                DataInicio = recorrencia.DataInicio,
                DataTermino = recorrencia.DataTermino,
                Ativa = recorrencia.Ativa
            };
        }

        private static SugestaoDespesaRecorrenteResponse MapearSugestao(DespesaRecorrenteOcorrencia ocorrencia)
        {
            var recorrencia = ocorrencia.DespesaRecorrente;
            return new SugestaoDespesaRecorrenteResponse
            {
                OcorrenciaId = ocorrencia.Id,
                DespesaRecorrenteId = recorrencia.Id,
                MesReferencia = ocorrencia.MesReferencia,
                Status = ocorrencia.Status.ToString(),
                Descricao = recorrencia.Descricao,
                ValorPrevisto = recorrencia.ValorPrevisto,
                DataSugerida = recorrencia.CalcularDataSugerida(ocorrencia.MesReferencia),
                IdContaFinanceira = recorrencia.IdContaFinanceira,
                ContaDescricao = recorrencia.ContaFinanceira?.Descricao ?? string.Empty,
                IdTipoDespesa = recorrencia.IdTipoDespesa,
                IdNaturezaDespesa = recorrencia.IdNaturezaDespesa,
                IdCategoria = recorrencia.IdCategoria,
                DespesaConfirmadaId = ocorrencia.DespesaConfirmadaId
            };
        }

        private static HashSet<Guid> ObterCategoriaEDescendentes(List<Categoria> categorias, Guid raizId)
        {
            var ids = new HashSet<Guid> { raizId };
            var adicionou = true;

            while (adicionou)
            {
                adicionou = false;
                foreach (var categoria in categorias)
                {
                    if (categoria.CategoriaPaiId.HasValue
                        && ids.Contains(categoria.CategoriaPaiId.Value)
                        && ids.Add(categoria.Id))
                    {
                        adicionou = true;
                    }
                }
            }

            return ids;
        }

        private static string ObterDescricaoCategoria(List<Categoria> categorias, Guid id)
        {
            return categorias.FirstOrDefault(c => c.Id == id)?.Descricao ?? string.Empty;
        }

        private static void ValidarMesAno(int mes, int ano)
        {
            if (mes < 1 || mes > 12 || ano < 1)
                throw new ArgumentException("Mês e ano inválidos.");
        }
    }
}
