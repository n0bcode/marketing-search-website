using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Repositories.MongoDb
{
    public interface IAnalysisLinkRepositoryMongo : IRepositoryMongo<AnalysisLink>
    {
        Task<AnalysisLink?> GetAnalysisLinkOrNot(string linkOrKeyword);
        Task AddAsync(AnalysisLink result);
        Task<AnalysisLink> AddOrUpdateText(AnalysisLink analysisLink);
    }
}