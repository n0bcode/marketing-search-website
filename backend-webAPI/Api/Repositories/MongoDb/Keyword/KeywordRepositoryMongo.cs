using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Utils;
using MongoDB.Driver;

namespace Api.Repositories.MongoDb
{
    public class KeywordRepositoryMongo : RepositoryMongo<KeywordModel>, IKeywordRepositoryMongo
    {
        public KeywordRepositoryMongo(MongoDbContext context)
            : base(context, "Keywords")
        {
        }

        public async Task<ResponseAPI<string>> AddKeywordAndGetIdAsync(KeywordModel keywordModel)
        {
            ResponseAPI<string> response = new();
            try
            {
                await _collection.InsertOneAsync(keywordModel);

                if (string.IsNullOrEmpty(keywordModel.Id))
                {
                    // Thử lấy lại Id bằng cách truy vấn lại bản ghi vừa insert (rất hiếm khi cần)
                    var last = await _collection.Find(x => x.Keyword == keywordModel.Keyword && x.Source == keywordModel.Source)
                                               .SortByDescending(x => x.CreatedAt)
                                               .FirstOrDefaultAsync();
                    response.SetSuccessResponse(data: last?.Id ?? string.Empty);
                }
                else
                {
                    response.SetSuccessResponse(data: keywordModel.Id);
                }
                // Giả sử Id là ObjectId dạng string
                response.SetSuccessResponse(data: keywordModel.Id ?? string.Empty);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<IEnumerable<KeywordModel>>> GetAllKeywordsAsync()
        {
            var keywords = await _collection.Find(_ => true).ToListAsync();
            return new ResponseAPI<IEnumerable<KeywordModel>>
            {
                Success = true,
                Data = keywords,
                Message = "Fetched all keywords"
            };
        }
    }
}