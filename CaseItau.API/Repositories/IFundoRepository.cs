using CaseItau.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseItau.API.Repositories
{
    public interface IFundoRepository
    {
        Task<IEnumerable<Fundo>> GetAllAsync();
        Task<Fundo?> GetByCodigoAsync(string codigo);
        Task CreateAsync(Fundo fundo);
        Task UpdateAsync(string codigo, Fundo fundo);
        Task DeleteAsync(string codigo);
        Task MovimentarPatrimonioAsync(string codigo, decimal valor);
    }
}
