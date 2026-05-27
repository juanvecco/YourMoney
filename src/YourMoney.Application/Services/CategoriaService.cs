using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriaService(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        public async Task<Categoria> GetByIdAsync(Guid id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null)
                throw new InvalidOperationException("Categoria não encontrada.");
            return categoria;
        }

        public async Task<List<Categoria>> GetAllAsync()
        {
            return await _categoriaRepository.GetAllAsync();
        }

        public async Task AdicionarAsync(Categoria categoria)
        {
            await ValidarCategoriaPaiAsync(categoria.CategoriaPaiId);
            await _categoriaRepository.AdicionarAsync(categoria);
        }

        public async Task AtualizarAsync(Categoria categoria)
        {
            await ValidarCategoriaPaiAsync(categoria.CategoriaPaiId, categoria.Id);
            await _categoriaRepository.AtualizarAsync(categoria);
        }

        public async Task RemoverAsync(Guid id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null)
                throw new InvalidOperationException("Categoria não encontrada.");

            await _categoriaRepository.RemoverAsync(id);
        }

        private async Task ValidarCategoriaPaiAsync(Guid? categoriaPaiId, Guid? categoriaId = null)
        {
            if (!categoriaPaiId.HasValue)
                return;

            if (categoriaId.HasValue && categoriaPaiId.Value == categoriaId.Value)
                throw new InvalidOperationException("Categoria não pode ser pai dela mesma.");

            if (!await _categoriaRepository.ExisteAsync(categoriaPaiId.Value))
                throw new InvalidOperationException("Categoria pai não encontrada.");
        }
    }
}
