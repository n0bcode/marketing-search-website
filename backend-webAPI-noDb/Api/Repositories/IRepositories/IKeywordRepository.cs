using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Constants;
using Api.Models;

namespace Api.Repositories.IRepositories
{
    public interface IKeywordRepository : IRepository<KeywordModel>
    {
        Task<ResponseAPI<string>> AddKeywordAndGetIdAsync(KeywordModel keywordModel);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetAllKeywordsAsync();
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetAllKeywordsAsync(string? source = null, string? term = null, string? socialMediaInfo = null);
        Task<ResponseAPI<KeywordModel>> GetKeywordByIdAsync(string id);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceAsync(string source);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsByTermAsync(string term);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySocialMediaInfoAsync(string socialMediaInfo);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceAndTermAsync(string source, string term);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceAndSocialMediaInfoAsync(string source, string socialMediaInfo);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsByTermAndSocialMediaInfoAsync(string term, string socialMediaInfo);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceTermAndSocialMediaInfoAsync(string source, string term, string socialMediaInfo);
        Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceOrTermAsync(string source, string term);
        Task<ResponseAPI<string>> UpsertKeywordAsync(KeywordModel keywordModel);
        Task<ResponseAPI<string>> DeleteKeywordAsync(string id);

    }
}