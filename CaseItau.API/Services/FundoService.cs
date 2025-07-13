using CaseItau.API.Model;
using CaseItau.API.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseItau.API.Services
{
    public class FundoService : IFundoService
    {
        private readonly IFundoRepository _fundoRepository;
        private readonly ILogger<FundoService> _logger;

        public FundoService(IFundoRepository fundoRepository, ILogger<FundoService> logger)
        {
            _fundoRepository = fundoRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Fundo>> GetAllFundosAsync()
        {
            try
            {
                _logger.LogInformation("Buscando todos os fundos");
                return await _fundoRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os fundos");
                throw;
            }
        }

        public async Task<Fundo?> GetFundoByCodigoAsync(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ArgumentException("Código do fundo é obrigatório", nameof(codigo));
            }

            try
            {
                _logger.LogInformation("Buscando fundo com código: {Codigo}", codigo);
                return await _fundoRepository.GetByCodigoAsync(codigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar fundo com código: {Codigo}", codigo);
                throw;
            }
        }

        public async Task CreateFundoAsync(Fundo fundo)
        {
            ValidateFundo(fundo);

            // Verificar se o fundo já existe
            var existingFundo = await _fundoRepository.GetByCodigoAsync(fundo.Codigo);
            if (existingFundo != null)
            {
                throw new InvalidOperationException($"Fundo com código {fundo.Codigo} já existe");
            }

            try
            {
                _logger.LogInformation("Criando novo fundo com código: {Codigo}", fundo.Codigo);
                await _fundoRepository.CreateAsync(fundo);
                _logger.LogInformation("Fundo criado com sucesso: {Codigo}", fundo.Codigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar fundo com código: {Codigo}", fundo.Codigo);
                throw;
            }
        }

        public async Task UpdateFundoAsync(string codigo, Fundo fundo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ArgumentException("Código do fundo é obrigatório", nameof(codigo));
            }

            ValidateFundo(fundo);

            // Verificar se o fundo existe
            var existingFundo = await _fundoRepository.GetByCodigoAsync(codigo);
            if (existingFundo == null)
            {
                throw new KeyNotFoundException($"Fundo com código {codigo} não encontrado");
            }

            try
            {
                _logger.LogInformation("Atualizando fundo com código: {Codigo}", codigo);
                await _fundoRepository.UpdateAsync(codigo, fundo);
                _logger.LogInformation("Fundo atualizado com sucesso: {Codigo}", codigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar fundo com código: {Codigo}", codigo);
                throw;
            }
        }

        public async Task DeleteFundoAsync(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ArgumentException("Código do fundo é obrigatório", nameof(codigo));
            }

            // Verificar se o fundo existe
            var existingFundo = await _fundoRepository.GetByCodigoAsync(codigo);
            if (existingFundo == null)
            {
                throw new KeyNotFoundException($"Fundo com código {codigo} não encontrado");
            }

            try
            {
                _logger.LogInformation("Deletando fundo com código: {Codigo}", codigo);
                await _fundoRepository.DeleteAsync(codigo);
                _logger.LogInformation("Fundo deletado com sucesso: {Codigo}", codigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar fundo com código: {Codigo}", codigo);
                throw;
            }
        }

        public async Task MovimentarPatrimonioAsync(string codigo, decimal valor)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ArgumentException("Código do fundo é obrigatório", nameof(codigo));
            }

            // Verificar se o fundo existe
            var existingFundo = await _fundoRepository.GetByCodigoAsync(codigo);
            if (existingFundo == null)
            {
                throw new KeyNotFoundException($"Fundo com código {codigo} não encontrado");
            }

            // Validar se o patrimônio não ficará negativo
            var patrimonioAtual = existingFundo.Patrimonio ?? 0;
            if (patrimonioAtual + valor < 0)
            {
                throw new InvalidOperationException("Operação resultaria em patrimônio negativo");
            }

            try
            {
                _logger.LogInformation("Movimentando patrimônio do fundo {Codigo}: {Valor}", codigo, valor);
                await _fundoRepository.MovimentarPatrimonioAsync(codigo, valor);
                _logger.LogInformation("Patrimônio movimentado com sucesso para o fundo: {Codigo}", codigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao movimentar patrimônio do fundo: {Codigo}", codigo);
                throw;
            }
        }

        private static void ValidateFundo(Fundo fundo)
        {
            if (fundo == null)
            {
                throw new ArgumentNullException(nameof(fundo), "Fundo é obrigatório");
            }

            if (string.IsNullOrWhiteSpace(fundo.Codigo))
            {
                throw new ArgumentException("Código do fundo é obrigatório", nameof(fundo.Codigo));
            }

            if (string.IsNullOrWhiteSpace(fundo.Nome))
            {
                throw new ArgumentException("Nome do fundo é obrigatório", nameof(fundo.Nome));
            }

            if (string.IsNullOrWhiteSpace(fundo.Cnpj))
            {
                throw new ArgumentException("CNPJ do fundo é obrigatório", nameof(fundo.Cnpj));
            }

            if (fundo.CodigoTipo <= 0)
            {
                throw new ArgumentException("Código do tipo é obrigatório e deve ser maior que zero", nameof(fundo.CodigoTipo));
            }
        }
    }
}
