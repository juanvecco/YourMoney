using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
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
        public async Task<IActionResult> AdicionarCategoria([FromBody] CategoriaDTO dto)
        {
            try
            {
                var categoria = new Categoria(dto.Descricao, dto.TipoTransacao, dto.CategoriaPaiId);
                await _categoriaService.AdicionarAsync(categoria);
                return CreatedAtAction(nameof(ObterCategoria), new { id = categoria.Id }, MapearCategoria(categoria));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
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
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarCategoria()
        {
            try
            {
                var categorias = await _categoriaService.GetAllAsync();
                return Ok(categorias.Select(MapearCategoria));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterCategoria(Guid id)
        {
            try
            {
                var categoria = await _categoriaService.GetByIdAsync(id);
                return Ok(MapearCategoria(categoria));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarCategoria(Guid id, [FromBody] CategoriaDTO dto)
        {
            try
            {
                var categoria = await _categoriaService.GetByIdAsync(id);
                categoria.Atualizar(dto.Descricao, dto.TipoTransacao, dto.CategoriaPaiId);
                await _categoriaService.AtualizarAsync(categoria);
                return Ok(MapearCategoria(categoria));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private static CategoriaDTO MapearCategoria(Categoria categoria)
        {
            return new CategoriaDTO
            {
                Id = categoria.Id,
                Descricao = categoria.Descricao,
                TipoTransacao = categoria.TipoTransacao,
                CategoriaPaiId = categoria.CategoriaPaiId
            };
        }
    }
}
