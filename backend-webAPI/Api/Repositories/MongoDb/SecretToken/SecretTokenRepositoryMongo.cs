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
using Api.Utils;
using MongoDB.Driver;

namespace Api.Repositories.MongoDb
{
    public class SecretTokenRepositoryMongo : RepositoryMongo<SecretToken>, ISecretTokenRepositoryMongo
    {
        public SecretTokenRepositoryMongo(MongoDbContext context) : base(context, "SecretTokens") { }


        public async Task<SecretToken?> GetByIdAsync(string id)
        {
            var filter = Builders<SecretToken>.Filter.Eq(x => x.Id, id);
            var secretToken = await _collection.Find(filter).FirstOrDefaultAsync();
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
                var secretTokens = await _collection.Find(_ => true).ToListAsync();
                var result = new List<SecretTokenResponseDTO>();
                foreach (var token in secretTokens)
                {
                    var dto = token.Format();
                    // dto.Token = EncryptionHelper.Decrypt(dto.Token);
                    result.Add(dto);
                }
                if (result.Count == 0)
                {
                    throw new Exception("Không có dữ liệu khả thi cho bạn lựa chọn thêm.");
                }
                response.SetData(result);
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
                CheckServiceName(secretToken.Service);

                // Tìm token theo Id hoặc theo Name (ưu tiên Id)
                var filter = Builders<SecretToken>.Filter.Or(
                    Builders<SecretToken>.Filter.Eq(x => x.Id, secretToken.Id),
                    Builders<SecretToken>.Filter.Eq(x => x.Name, secretToken.Name)
                );
                var existingToken = await _collection.Find(filter).FirstOrDefaultAsync();

                if (existingToken != null)
                {
                    // Nếu token đã tồn tại nhưng khác Id, kiểm tra trùng token
                    if (existingToken.Id != secretToken.Id && existingToken.Token == EncryptionHelper.Encrypt(secretToken.Token))
                    {
                        throw new Exception($"Token đã tồn tại: {secretToken.Token}");
                    }

                    // Cập nhật các trường cần thiết
                    var update = Builders<SecretToken>.Update
                        .Set(x => x.Name, secretToken.Name)
                        .Set(x => x.Service, secretToken.Service)
                        .Set(x => x.Note, secretToken.Note)
                        .Set(x => x.Token, EncryptionHelper.Encrypt(secretToken.Token))
                        .Set(x => x.UpdatedAt, DateTime.UtcNow)
                        .Set(x => x.IsActive, secretToken.IsActive);

                    await _collection.UpdateOneAsync(
                        Builders<SecretToken>.Filter.Eq(x => x.Id, existingToken.Id),
                        update
                    );
                    response.SetSuccessResponse($"Cập nhật thành công: {existingToken.Id}");
                    response.SetData(existingToken.Id!);
                }
                else
                {
                    // Thêm mới
                    secretToken.Token = EncryptionHelper.Encrypt(secretToken.Token);
                    await _collection.InsertOneAsync(secretToken);
                    response.SetSuccessResponse($"Thêm mới thành công: {secretToken.Id}");
                    response.SetData(secretToken.Id!);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<SecretToken>.Filter.Eq(x => x.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
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