using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Constants;
using Api.DTOs;
using Api.DTOs.SecretTokenDTO;
using Api.Models;

namespace Api.Repositories.IRepositories
{
    public interface ISecretTokenRepository : IRepository<SecretToken>
    {
        Task<SecretToken?> GetByIdAsync(string id);
        Task<ResponseAPI<IEnumerable<SecretTokenResponseDTO>>> GetAllAsync();
        Task<ResponseAPI<string>> UpsertAsync(SecretTokenRequestDTO secretTokenDTO);
        Task<bool> DeleteAsync(string id);
    }
}