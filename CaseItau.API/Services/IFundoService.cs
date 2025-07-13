using CaseItau.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseItau.API.Services
{
    public interface IFundoService
    {
        Task<IEnumerable<Fundo>> GetAllFundosAsync();
        Task<Fundo?> GetFundoByCodigoAsync(string codigo);
        Task CreateFundoAsync(Fundo fundo);
        Task UpdateFundoAsync(string codigo, Fundo fundo);
        Task DeleteFundoAsync(string codigo);
        Task MovimentarPatrimonioAsync(string codigo, decimal valor);
    }
}
