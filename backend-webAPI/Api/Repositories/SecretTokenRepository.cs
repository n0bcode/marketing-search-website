using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Constants;
using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories
{
    public class SecretTokenRepository : Repository<SecretToken>, ISecretTokenRepository
    {
        private readonly AppDbContext _db;

        public SecretTokenRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<SecretToken?> GetByIdAsync(Guid id)
        {
            var secretToken = await _db.SecretTokens.FindAsync(id);
            if (secretToken != null)
            {
                secretToken.Token = EncryptionHelper.Decrypt(secretToken.Token); // Giải mã token
            }
            return secretToken;
        }

        public async Task<IEnumerable<SecretToken>> GetAllAsync()
        {
            var secretTokens = await _db.SecretTokens.ToListAsync();
            foreach (var token in secretTokens)
            {
                token.Token = EncryptionHelper.Decrypt(token.Token); // Giải mã từng token
            }
            return secretTokens;
        }

        public async Task<ResponseAPI<string>> CreateAsync(SecretToken secretToken)
        {
            var response = new ResponseAPI<string>();

            try
            {

                if (string.IsNullOrEmpty(secretToken.Token))
                {
                    throw new ArgumentNullException(nameof(secretToken.Token), "Token không được để trống");
                }
                secretToken.Token = EncryptionHelper.Encrypt(secretToken.Token); // Mã hóa token
                await _db.SecretTokens.AddAsync(secretToken);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse($"Thêm mới thành công: {secretToken.Id}");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
                return response;

            }
            return response;
        }

        public async Task<ResponseAPI<string>> UpdateAsync(SecretToken secretToken)
        {
            var response = new ResponseAPI<string>();
            try
            {
                secretToken.Token = EncryptionHelper.Encrypt(secretToken.Token); // Mã hóa token trước khi cập nhật
                _db.SecretTokens.Update(secretToken);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse($"Cập nhật thành công: {secretToken.Id}");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
                return response;
            }
            return response;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var secretToken = await GetByIdAsync(id);
            if (secretToken == null) return false;

            _db.SecretTokens.Remove(secretToken);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
