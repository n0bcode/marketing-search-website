using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Repositories.IRepositories
{
    public interface IAnalysisLinkRepository : IRepository<AnalysisLink>
    {
        public Task<AnalysisLink?> GetAnalysisLinkOrNot(string link);
        public Task<AnalysisLink> AddOrUpdateText(AnalysisLink analysisLink);
    }
}