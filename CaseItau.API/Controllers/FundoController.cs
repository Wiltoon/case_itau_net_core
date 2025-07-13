using CaseItau.API.Model;
using CaseItau.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseItau.API.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de fundos de investimento
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class FundoController : ControllerBase
    {
        private readonly IFundoService _fundoService;
        private readonly ILogger<FundoController> _logger;

        public FundoController(IFundoService fundoService, ILogger<FundoController> logger)
        {
            _fundoService = fundoService;
            _logger = logger;
        }

        /// <summary>
        /// Lista todos os fundos cadastrados
        /// </summary>
        /// <returns>Lista de fundos</returns>
        /// <response code="200">Retorna a lista de fundos</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Fundo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Fundo>>> Get()
        {
            try
            {
                var fundos = await _fundoService.GetAllFundosAsync();
                return Ok(fundos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar fundos");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Busca um fundo específico pelo código
        /// </summary>
        /// <param name="codigo">Código do fundo</param>
        /// <returns>Dados do fundo</returns>
        /// <response code="200">Retorna o fundo encontrado</response>
        /// <response code="400">Parâmetros inválidos</response>
        /// <response code="404">Fundo não encontrado</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpGet("{codigo}")]
        [ProducesResponseType(typeof(Fundo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Fundo>> Get(string codigo)
        {
            try
            {
                var fundo = await _fundoService.GetFundoByCodigoAsync(codigo);
                if (fundo == null)
                {
                    return NotFound($"Fundo com código {codigo} não encontrado");
                }
                return Ok(fundo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar fundo com código: {Codigo}", codigo);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // POST: api/Fundo
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Fundo fundo)
        {
            try
            {
                await _fundoService.CreateFundoAsync(fundo);
                return CreatedAtAction(nameof(Get), new { codigo = fundo.Codigo }, fundo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar fundo");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // PUT: api/Fundo/ITAUTESTE01
        [HttpPut("{codigo}")]
        public async Task<ActionResult> Put(string codigo, [FromBody] Fundo fundo)
        {
            try
            {
                await _fundoService.UpdateFundoAsync(codigo, fundo);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar fundo com código: {Codigo}", codigo);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // DELETE: api/Fundo/ITAUTESTE01
        [HttpDelete("{codigo}")]
        public async Task<ActionResult> Delete(string codigo)
        {
            try
            {
                await _fundoService.DeleteFundoAsync(codigo);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar fundo com código: {Codigo}", codigo);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Movimenta o patrimônio de um fundo
        /// </summary>
        /// <param name="codigo">Código do fundo</param>
        /// <param name="movimentacao">Dados da movimentação (Operation: ADD/SUB, Value: valor)</param>
        /// <returns>Status da operação</returns>
        /// <response code="200">Movimentação realizada com sucesso</response>
        /// <response code="400">Parâmetros inválidos</response>
        /// <response code="404">Fundo não encontrado</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPut("{codigo}/patrimonio")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> MovimentarPatrimonio(string codigo, [FromBody] MovimentacaoPatrimonio movimentacao)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Determina o valor a ser movimentado baseado na operação
                decimal valorMovimentacao = movimentacao.Operation.ToUpper() == "ADD" 
                    ? movimentacao.Value 
                    : -movimentacao.Value;

                await _fundoService.MovimentarPatrimonioAsync(codigo, valorMovimentacao);
                
                // Retorna o fundo atualizado
                var fundoAtualizado = await _fundoService.GetFundoByCodigoAsync(codigo);
                return Ok(new { 
                    Message = $"Patrimônio {(movimentacao.Operation.ToUpper() == "ADD" ? "aumentado" : "diminuído")} em {movimentacao.Value:C}",
                    FundoAtualizado = fundoAtualizado 
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao movimentar patrimônio do fundo: {Codigo}", codigo);
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }
}
