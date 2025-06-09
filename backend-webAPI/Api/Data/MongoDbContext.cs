using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using MongoDB.Driver;

namespace Api.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _db;
        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _db = client.GetDatabase("marketingSearch");
        }

        public IMongoCollection<AnalysisLink> AnalysisLinks => _db.GetCollection<AnalysisLink>("AnalysisLinks");
        public IMongoCollection<KeywordModel> Keywords => _db.GetCollection<KeywordModel>("Keywords");
        public IMongoCollection<SecretToken> SecretTokens => _db.GetCollection<SecretToken>("SecretTokens");
    }
}