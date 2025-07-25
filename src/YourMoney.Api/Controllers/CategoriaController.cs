using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;
        public CategoriaController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpPost]

        public async Task<IActionResult> AdicionarCategoria([FromBody] Categoria categoria)
        {
            try
            {
                await _categoriaService.AdicionarAsync(categoria);
                return CreatedAtAction(nameof(AdicionarCategoria), new { id = categoria.Id }, categoria);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverCategoria(Guid id)
        {
            try
            {
                await _categoriaService.RemoverAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarCategoria()
        {
            try
            {
                var categorias = await _categoriaService.GetAllAsync();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarCategoria(Guid id, [FromBody] CategoriaDTO dto)
        {
            var categoria = await _categoriaService.GetByIdAsync(id);

            categoria.AtualizarDescricao(dto.Descricao);

            await _categoriaService.AtualizarAsync(categoria);
            return NoContent();

        }
    }
}
