using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using MongoDB.Driver;

namespace Api.Repositories.MongoDb
{
    public class AnalysisLinkRepositoryMongo : RepositoryMongo<AnalysisLink>, IAnalysisLinkRepositoryMongo
    {
        public AnalysisLinkRepositoryMongo(MongoDbContext context) : base(context, "AnalysisLinks") { }

        public async Task<AnalysisLink?> GetAnalysisLinkOrNot(string linkOrKeyword)
        {
            // Tìm bản ghi mới nhất theo LinkOrKeyword
            var filter = Builders<AnalysisLink>.Filter.Eq(x => x.LinkOrKeyword, linkOrKeyword);
            return await _collection
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(AnalysisLink result)
        {
            await _collection.InsertOneAsync(result);
        }

        public async Task<AnalysisLink> AddOrUpdateText(AnalysisLink analysisLink)
        {
            // Tìm bản ghi hiện tại theo LinkOrKeyword
            var filter = Builders<AnalysisLink>.Filter.Eq(x => x.LinkOrKeyword, analysisLink.LinkOrKeyword);
            var existing = await _collection.Find(filter).FirstOrDefaultAsync();

            if (existing != null)
            {
                // Update: giữ lại Id cũ
                analysisLink.Id = existing.Id;
                var options = new ReplaceOptions { IsUpsert = false };
                await _collection.ReplaceOneAsync(filter, analysisLink, options);
            }
            else
            {
                // Insert mới: KHÔNG gán Id, để MongoDB tự sinh
                // analysisLink.Id = null;
                await _collection.InsertOneAsync(analysisLink);
            }
            return analysisLink;
        }
    }
}