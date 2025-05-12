using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Constants;
using Api.Data;
using Api.Models;
using Api.Repositories.IRepositories;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories
{
    public class KeywordRepository(AppDbContext context) : Repository<KeywordModel>(context), IKeywordRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<ResponseAPI<string>> AddKeywordAndGetIdAsync(KeywordModel keywordModel)
        {
            ResponseAPI<string> response = new ResponseAPI<string>();
            try
            {
                await _context.Keywords.AddAsync(keywordModel);

                response.SetData(keywordModel.Id);
                response.SetSuccessResponse();
                #region [Ignore method if-else for create/update keyword search] 
                /* // Lấy danh sách các từ khóa hiện có
                var existingKeywords = await _context.Keywords.ToListAsync();

                var valueKeyPairs = existingKeywords.ToDictionary(x => x.Keyword, y => y.Id);

                // Tìm từ khóa gần giống nhất
                string? closestKey = RegexHelper.FindClosestMatch(keywordModel.Keyword, valueKeyPairs.Keys.ToList());

                // Nếu không tìm thấy từ khóa gần giống, thêm từ khóa mới
                if (closestKey == null)
                {
                    await AddAsync(keywordModel);
                    response.SetSuccessResponse("Keyword added successfully.");
                    response.Data = keywordModel.Id;
                }
                else
                {
                    // Nếu tìm thấy từ khóa gần giống, kiểm tra xem từ khóa đó có tồn tại không
                    KeywordModel? existingKeyword = await GetAsync(x => x.Id == valueKeyPairs[closestKey]);

                    if (existingKeyword == null)
                    {
                        // Nếu từ khóa không tồn tại, thêm từ khóa mới
                        await AddAsync(keywordModel);
                        response.SetSuccessResponse("Keyword added successfully.");
                        response.Data = keywordModel.Id;
                    }
                    else
                    {
                        // Nếu từ khóa đã tồn tại, thêm từ khóa mới vào danh sách từ khóa liên quan
                        if (!existingKeyword.RelatedKeyword.Contains(keywordModel.Keyword))
                        {
                            existingKeyword.RelatedKeyword.Add(keywordModel.Keyword);
                        }

                        // Lưu thay đổi
                        await _context.SaveChangesAsync();
                        response.SetSuccessResponse("Keyword already exists. Related keyword added.");
                        response.Data = existingKeyword.Id; // Trả về ID của từ khóa đã tồn tại
                    }
                } */
                #endregion
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public Task<ResponseAPI<string>> DeleteKeywordAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseAPI<IEnumerable<KeywordModel>>> GetAllKeywordsAsync()
        {
            ResponseAPI<IEnumerable<KeywordModel>> response = new();
            try
            {
                var data = await GetAllAsync();

                response.SetSuccessResponse();
                response.SetData(data);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }


        public Task<ResponseAPI<IEnumerable<KeywordModel>>> GetAllKeywordsAsync(string? source = null, string? term = null, string? socialMediaInfo = null)
        {
            throw new NotImplementedException();
        }
        public Task<ResponseAPI<KeywordModel>> GetKeywordByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySocialMediaInfoAsync(string socialMediaInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceAndSocialMediaInfoAsync(string source, string socialMediaInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceAndTermAsync(string source, string term)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceAsync(string source)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceOrTermAsync(string source, string term)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsBySourceTermAndSocialMediaInfoAsync(string source, string term, string socialMediaInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsByTermAndSocialMediaInfoAsync(string term, string socialMediaInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<IEnumerable<KeywordModel>>> GetKeywordsByTermAsync(string term)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<string>> UpsertKeywordAsync(KeywordModel keywordModel)
        {
            throw new NotImplementedException();
        }
    }
}