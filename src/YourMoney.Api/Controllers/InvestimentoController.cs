using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.Interfaces;
using YourMoney.Application.DTOs;
using YourMoney.Domain.Entities;
using YourMoney.Application.Services;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestimentoController : ControllerBase
    {
        private readonly IInvestimentoService _investimentoService;
        //private readonly ILogger<InvestimentoController> _logger;

        public InvestimentoController(IInvestimentoService investimentoService)//,
            //ILogger<InvestimentoController> logger)
        {
            _investimentoService = investimentoService;
            //_logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> AdicionarInvestimento([FromBody] Investimento investimento)
        {
            try
            {
                await _investimentoService.AdicionarInvestimentoAsync(investimento);
                return CreatedAtAction(nameof(AdicionarInvestimento), new { id = investimento.Id }, investimento);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverInvestimento(Guid id)
        {
            try
            {
                await _investimentoService.RemoverInvestimentoAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarInvestimentos()
        {
            try
            {
                var investimentos = await _investimentoService.ListarAsync();
                return Ok(investimentos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarInvestimento(Guid id, [FromBody] InvestimentoDto dto)
        {
            var investimento = await _investimentoService.GetInvestimentoByIdAsync(id);
            if (investimento == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Nome))
                investimento.AtualizarNome(dto.Nome);

            investimento.AtualizarDescricao(dto.Descricao);
            investimento.AtualizarValorAtual(dto.ValorAtual);
            investimento.AtualizarData(dto.DataInvestimento);

            if (!string.IsNullOrWhiteSpace(dto.Tipo))
                investimento.AtualizarTipo(dto.Tipo);

            if (dto.Quantidade > 0)
                investimento.AtualizarQuantidade(dto.Quantidade);

            if (dto.PrecoMedio > 0)
                investimento.AtualizarPrecoMedio(dto.PrecoMedio);

            await _investimentoService.AtualizarAsync(investimento);
            return NoContent();
        }

        [HttpGet("por-referencia")]
        public async Task<IActionResult> ObterPorReferencia([FromQuery] int mes, [FromQuery] int ano)
        {
            var investimentos = await _investimentoService.ObterPorMesAnoAsync(mes, ano);
            return Ok(investimentos);
        }
        /// <summary>
        /// Obtém todos os investimentos do usuário
        /// </summary>
        /// <returns>Lista de investimentos</returns>
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<InvestimentoDto>>> ObterInvestimentos()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Obtendo lista de investimentos");
        //        var investimentos = await _investimentoService.ListarAsync();
        //        return Ok(investimentos);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao obter investimentos");
        //        return StatusCode(500, "Erro interno do servidor");
        //    }
        //}

        ///// <summary>
        ///// Obtém um investimento específico por ID
        ///// </summary>
        ///// <param name="id">ID do investimento</param>
        ///// <returns>Investimento encontrado</returns>
        //[HttpGet("{id}")]
        //public async Task<ActionResult<InvestimentoDto>> ObterInvestimentoPorId(Guid id)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Obtendo investimento com ID: {Id}", id);
        //        var investimento = await _investimentoService.GetInvestimentoByIdAsync(id);

        //        if (investimento == null)
        //        {
        //            _logger.LogWarning("Investimento com ID {Id} não encontrado", id);
        //            return NotFound($"Investimento com ID {id} não encontrado");
        //        }

        //        return Ok(investimento);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao obter investimento com ID: {Id}", id);
        //        return StatusCode(500, "Erro interno do servidor");
        //    }
        //}

        ///// <summary>
        ///// Cria um novo investimento
        ///// </summary>
        ///// <param name="criarInvestimentoDto">Dados do investimento a ser criado</param>
        ///// <returns>Investimento criado</returns>
        //[HttpPost]
        //public async Task<ActionResult<InvestimentoDto>> CriarInvestimento([FromBody] CriarInvestimentoDto criarInvestimentoDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            _logger.LogWarning("Dados inválidos para criação de investimento: {@ModelState}", ModelState);
        //            return BadRequest(ModelState);
        //        }

        //        _logger.LogInformation("Criando novo investimento: {@Investimento}", criarInvestimentoDto);
        //        var investimento = await _investimentoService.AdicionarInvestimentoAsync(criarInvestimentoDto);

        //        return CreatedAtAction(
        //            nameof(ObterInvestimentoPorId),
        //            new { id = investimento.Id },
        //            investimento);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Argumentos inválidos para criação de investimento");
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao criar investimento");
        //        return StatusCode(500, "Erro interno do servidor");
        //    }
        //}

        ///// <summary>
        ///// Atualiza um investimento existente
        ///// </summary>
        ///// <param name="id">ID do investimento</param>
        ///// <param name="atualizarInvestimentoDto">Dados atualizados do investimento</param>
        ///// <returns>Investimento atualizado</returns>
        //[HttpPut("{id}")]
        //public async Task<ActionResult<InvestimentoDto>> AtualizarInvestimento(int id, [FromBody] AtualizarInvestimentoDto atualizarInvestimentoDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            _logger.LogWarning("Dados inválidos para atualização de investimento: {@ModelState}", ModelState);
        //            return BadRequest(ModelState);
        //        }

        //        _logger.LogInformation("Atualizando investimento com ID: {Id}", id);
        //        var investimento = await _investimentoService.AtualizarAsync(id, atualizarInvestimentoDto);

        //        if (investimento == null)
        //        {
        //            _logger.LogWarning("Investimento com ID {Id} não encontrado para atualização", id);
        //            return NotFound($"Investimento com ID {id} não encontrado");
        //        }

        //        return Ok(investimento);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Argumentos inválidos para atualização de investimento");
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao atualizar investimento com ID: {Id}", id);
        //        return StatusCode(500, "Erro interno do servidor");
        //    }
        //}

        ///// <summary>
        ///// Exclui um investimento
        ///// </summary>
        ///// <param name="id">ID do investimento</param>
        ///// <returns>Resultado da operação</returns>
        //[HttpDelete("{id}")]
        //public async Task<ActionResult> ExcluirInvestimento(int id)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Excluindo investimento com ID: {Id}", id);
        //        var sucesso = await _investimentoService.ExcluirAsync(id);

        //        if (!sucesso)
        //        {
        //            _logger.LogWarning("Investimento com ID {Id} não encontrado para exclusão", id);
        //            return NotFound($"Investimento com ID {id} não encontrado");
        //        }

        //        _logger.LogInformation("Investimento com ID {Id} excluído com sucesso", id);
        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao excluir investimento com ID: {Id}", id);
        //        return StatusCode(500, "Erro interno do servidor");
        //    }
        //}

        ///// <summary>
        ///// Obtém o resumo dos investimentos
        ///// </summary>
        ///// <returns>Resumo dos investimentos</returns>
        //[HttpGet("resumo")]
        //public async Task<ActionResult<ResumoInvestimentoDto>> ObterResumo()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Obtendo resumo dos investimentos");
        //        var resumo = await _investimentoService.ObterResumoAsync();
        //        return Ok(resumo);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao obter resumo dos investimentos");
        //        return StatusCode(500, "Erro interno do servidor");
        //    }
        //}

        /// <summary>
        /// Obtém a distribuição dos investimentos por categoria
        /// </summary>
        /// <returns>Distribuição por categoria</returns>
        //[HttpGet("distribuicao")]
        //public async Task<ActionResult<IEnumerable<DistribuicaoCategoriaDto>>> ObterDistribuicao()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Obtendo distribuição dos investimentos por categoria");
        //        var distribuicao = await _investimentoService.ObterDistribuicaoAsync();
        //        return Ok(distribuicao);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao obter distribuição dos investimentos");
        //        return StatusCode(500, "Erro interno do servidor");
        //    }
        //}

        /// <summary>
        /// Atualiza as cotações dos investimentos
        /// </summary>
        /// <returns>Resultado da operação</returns>
        //[HttpPost("atualizar-cotacoes")]
        //public async Task<ActionResult> AtualizarCotacoes()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Atualizando cotações dos investimentos");
        //        await _investimentoService.AtualizarCotacoesAsync();
        //        _logger.LogInformation("Cotações atualizadas com sucesso");
        //        return Ok("Cotações atualizadas com sucesso");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao atualizar cotações dos investimentos");
        //        return StatusCode(500, "Erro interno do servidor");
        //    }
        //}
    }
}

