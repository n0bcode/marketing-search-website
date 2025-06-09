using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using MongoDB.Driver;

namespace Api.Repositories.MongoDb
{
    public class KeywordRepositoryMongo : RepositoryMongo<KeywordModel>, IKeywordRepositoryMongo
    {
        public KeywordRepositoryMongo(MongoDbContext context)
            : base(context.Keywords)
        {
        }

        public async Task<ResponseAPI<string>> AddKeywordAndGetIdAsync(KeywordModel keywordModel)
        {
            await _collection.InsertOneAsync(keywordModel);
            // Giả sử Id là ObjectId dạng string
            return new ResponseAPI<string>
            {
                Success = true,
                Data = keywordModel.Id, // Đảm bảo model có trường Id
                Message = "Keyword added successfully"
            };
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