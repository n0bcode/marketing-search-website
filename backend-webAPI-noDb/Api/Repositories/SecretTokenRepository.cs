using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Constants;
using Api.Data;
using Api.DTOs;
using Api.DTOs.SecretTokenDTO;
using Api.Helpers;
using Api.Models;
using Api.Repositories.IRepositories;
using Api.Utils;
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

        public async Task<SecretToken?> GetByIdAsync(string id)
        {
            var secretToken = await _db.SecretTokens.FindAsync(id);
            if (secretToken != null)
            {
                secretToken.Token = EncryptionHelper.Decrypt(secretToken.Token); // Giải mã token
            }
            return secretToken;
        }

        public async Task<ResponseAPI<IEnumerable<SecretTokenResponseDTO>>> GetAllAsync()
        {
            var response = new ResponseAPI<IEnumerable<SecretTokenResponseDTO>>();

            try
            {
                var secretTokens = await _db.SecretTokens.Select(x => x.Format()).ToListAsync();
                if (secretTokens.Count == 0)
                {
                    throw new Exception("Không có dữ liệu khả thi cho bạn lựa chọn thêm.");
                }
                response.SetData(secretTokens);
                response.SetSuccessResponse("Lấy danh sách thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);

            }
            return response;
        }
        public async Task<ResponseAPI<string>> UpsertAsync(SecretTokenRequestDTO secretTokenDTO)
        {
            var response = new ResponseAPI<string>();
            try
            {
                if (string.IsNullOrEmpty(secretTokenDTO.Token))
                {
                    throw new ArgumentNullException(nameof(secretTokenDTO.Token), "Token không được để trống");
                }
                SecretToken secretToken = secretTokenDTO.Format();
                // Kiểm tra ServiceName
                CheckServiceName(secretToken.Service);
                // Tìm token theo Id hoặc theo Name (ưu tiên Id)
                var existingToken = await _db.SecretTokens
                    .FirstOrDefaultAsync(x => x.Id == secretToken.Id || x.Name == secretToken.Name);

                if (existingToken != null)
                {
                    // Nếu token đã tồn tại nhưng khác Id, kiểm tra trùng token
                    if (existingToken.Id != secretToken.Id && existingToken.Token == EncryptionHelper.Encrypt(secretToken.Token))
                    {
                        throw new Exception($"Token đã tồn tại: {secretToken.Token}");
                    }

                    // Cập nhật các trường cần thiết
                    existingToken.Name = secretToken.Name;
                    existingToken.Service = secretToken.Service;
                    existingToken.Note = secretToken.Note;
                    existingToken.Token = EncryptionHelper.Encrypt(secretToken.Token);
                    existingToken.UpdatedAt = DateTime.UtcNow;
                    existingToken.IsActive = secretToken.IsActive;

                    _db.SecretTokens.Update(existingToken);
                    await _db.SaveChangesAsync();
                    response.SetSuccessResponse($"Cập nhật thành công: {existingToken.Id}");
                }
                else
                {
                    // Thêm mới
                    secretToken.Token = EncryptionHelper.Encrypt(secretToken.Token);
                    await _db.SecretTokens.AddAsync(secretToken);
                    await _db.SaveChangesAsync();
                    response.SetSuccessResponse($"Thêm mới thành công: {secretToken.Id}");
                }
                response.SetData(secretToken.Id);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var secretToken = await GetByIdAsync(id);
            if (secretToken == null) return false;

            _db.SecretTokens.Remove(secretToken);
            await _db.SaveChangesAsync();
            return true;
        }
        #region [Private Methods]
        private void CheckServiceName(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName), "Service name không được để trống");
            }
            if (!TypeServices.ActiveServices.Contains(serviceName))
            {
                throw new ArgumentException($"Dịch vụ không hỗ trợ để lưu token '{serviceName}' không hợp lệ");
            }
        }
        #endregion
    }
}
