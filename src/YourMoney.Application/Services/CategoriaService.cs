using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ICurrentUserService _currentUserService;

        public CategoriaService(ICategoriaRepository categoriaRepository, ICurrentUserService currentUserService)
        {
            _categoriaRepository = categoriaRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Categoria> GetByIdAsync(Guid id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (categoria == null)
                throw new InvalidOperationException("Categoria não encontrada.");
            return categoria;
        }

        public async Task<List<Categoria>> GetAllAsync()
        {
            return await _categoriaRepository.GetAllAsync(_currentUserService.UserId);
        }

        public async Task AdicionarAsync(Categoria categoria)
        {
            await ValidarCategoriaPaiAsync(categoria.CategoriaPaiId);
            categoria.DefinirUsuario(_currentUserService.UserId);
            await _categoriaRepository.AdicionarAsync(categoria);
        }

        public async Task AtualizarAsync(Categoria categoria)
        {
            await ValidarCategoriaPaiAsync(categoria.CategoriaPaiId, categoria.Id);
            categoria.DefinirUsuario(_currentUserService.UserId);
            await _categoriaRepository.AtualizarAsync(categoria);
        }

        public async Task RemoverAsync(Guid id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id, _currentUserService.UserId);
            if (categoria == null)
                throw new InvalidOperationException("Categoria não encontrada.");

            await _categoriaRepository.RemoverAsync(id, _currentUserService.UserId);
        }

        private async Task ValidarCategoriaPaiAsync(Guid? categoriaPaiId, Guid? categoriaId = null)
        {
            if (!categoriaPaiId.HasValue)
                return;

            if (categoriaId.HasValue && categoriaPaiId.Value == categoriaId.Value)
                throw new InvalidOperationException("Categoria não pode ser pai dela mesma.");

            if (!await _categoriaRepository.ExisteAsync(categoriaPaiId.Value, _currentUserService.UserId))
                throw new InvalidOperationException("Categoria pai não encontrada.");
        }
    }
}
