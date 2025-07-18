using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Repositories.MongoDb
{
    public interface IKeywordRepositoryMongo : IRepositoryMongo<KeywordModel>
    {
        Task<ResponseAPI<string>> AddKeywordAndGetIdAsync(KeywordModel keywordModel);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetAllKeywordsAsync();
    }
}