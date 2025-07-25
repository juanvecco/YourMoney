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
            await _categoriaRepository.AdicionarAsync(categoria);
        }

        public async Task AtualizarAsync(Categoria categoria)
        {
            await _categoriaRepository.AtualizarAsync(categoria);
        }

        public async Task RemoverAsync(Guid id)
        {
            await _categoriaRepository.RemoverAsync(id);
        }
    }
}