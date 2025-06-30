using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Repositories.IRepositories;

namespace Api.Repositories.MongoDb
{
    public class UnitOfWorkMongo : IUnitOfWorkMongo
    {
        private readonly MongoDbContext _context;


        public UnitOfWorkMongo(MongoDbContext context)
        {
            _context = context;
            Keywords = new KeywordRepositoryMongo(_context);
            AnalysisLinks = new AnalysisLinkRepositoryMongo(_context);
            SecretTokens = new SecretTokenRepositoryMongo(_context);
        }

        public IAnalysisLinkRepositoryMongo AnalysisLinks { get; private set; }

        public IKeywordRepositoryMongo Keywords { get; private set; }

        public ISecretTokenRepositoryMongo SecretTokens { get; private set; }
    }
}