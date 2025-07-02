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

        public async Task<List<Categoria>> GetByTipoAsync(TipoTransacao tipo)
        {
            return await _categoriaRepository.GetByTipoAsync(tipo);
        }

        public async Task<List<Categoria>> GetAtivasAsync()
        {
            return await _categoriaRepository.GetAtivasAsync();
        }

        public async Task AdicionarAsync(Categoria categoria)
        {
            if (await _categoriaRepository.ExisteNomeAsync(categoria.Nome, categoria.TipoTransacao))
                throw new InvalidOperationException("Já existe uma categoria com este nome e tipo.");
            await _categoriaRepository.AdicionarAsync(categoria);
        }

        public async Task AtualizarAsync(Categoria categoria)
        {
            if (await _categoriaRepository.ExisteNomeAsync(categoria.Nome, categoria.TipoTransacao, categoria.Id))
                throw new InvalidOperationException("Já existe uma categoria com este nome e tipo.");
            await _categoriaRepository.AtualizarAsync(categoria);
        }

        public async Task RemoverAsync(Guid id)
        {
            await _categoriaRepository.RemoverAsync(id);
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            return await _categoriaRepository.ExisteAsync(id);
        }

        public async Task<bool> ExisteNomeAsync(string nome, TipoTransacao tipo, Guid? ignorarId = null)
        {
            return await _categoriaRepository.ExisteNomeAsync(nome, tipo, ignorarId);
        }
    }
}