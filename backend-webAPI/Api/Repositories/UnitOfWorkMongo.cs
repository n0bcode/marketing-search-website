using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Repositories.IRepositories;

namespace Api.Repositories
{
    public class UnitOfWorkMongo : IUnitOfWorkMongo
    {
        private readonly MongoDbContext _context;

        public UnitOfWorkMongo(MongoDbContext context)
        {
            _context = context;
            // Keywords = new KeywordRepository(_context);
            // AnalysisLinks = new AnalysisLinkRepository(_context);
            // SecretTokens = new SecretTokenRepository(_context);
        }

    }
}