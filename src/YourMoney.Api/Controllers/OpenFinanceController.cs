using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class OpenFinanceController : ControllerBase
    {
        private readonly IOpenFinanceService _openFinanceService;

        public OpenFinanceController(IOpenFinanceService openFinanceService)
        {
            _openFinanceService = openFinanceService;
        }

        [HttpGet("sources")]
        public async Task<IActionResult> ObterFontes()
        {
            try
            {
                var response = await _openFinanceService.ObterFontesAsync();
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    title = "Nao foi possivel carregar as fontes OpenFinance.",
                    status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("transactions/preview")]
        public async Task<IActionResult> ObterPreviewTransacoes([FromQuery] string sourceId = null)
        {
            try
            {
                var response = await _openFinanceService.ObterPreviewTransacoesAsync(sourceId);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    title = "Nao foi possivel carregar o preview OpenFinance.",
                    status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
