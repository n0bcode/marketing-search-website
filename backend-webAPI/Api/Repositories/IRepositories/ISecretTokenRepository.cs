using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Constants;
using Api.Models;

namespace Api.Repositories.IRepositories
{
    public interface ISecretTokenRepository : IRepository<SecretToken>
    {
        Task<SecretToken?> GetByIdAsync(Guid id);
        Task<IEnumerable<SecretToken>> GetAllAsync();
        Task<ResponseAPI<string>> CreateAsync(SecretToken secretToken);
        Task<ResponseAPI<string>> UpdateAsync(SecretToken secretToken);
        Task<bool> DeleteAsync(Guid id);
    }
}