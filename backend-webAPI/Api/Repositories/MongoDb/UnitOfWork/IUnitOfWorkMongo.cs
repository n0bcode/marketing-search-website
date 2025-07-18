using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;

namespace Api.Repositories.MongoDb
{
    public interface IUnitOfWorkMongo
    {
        // MongoDbContext _context { get; }
        IAnalysisLinkRepositoryMongo AnalysisLinks { get; }
        IKeywordRepositoryMongo Keywords { get; }
        ISecretTokenRepositoryMongo SecretTokens { get; }
    }
}